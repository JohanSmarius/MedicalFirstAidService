using Domain;
using Microsoft.Extensions.Logging;

namespace DomainService;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;
    private readonly IEmailService _emailService;
    private readonly IStaffAssignmentRepository _assignmentRepository;
    private readonly ILogger<EventService> _logger;
    private readonly EventChangeHandler _domainService = new();
        
    public EventService(
        IEventRepository repository,
        IEmailService emailService,
        IStaffAssignmentRepository assignmentRepository,
        ILogger<EventService> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _assignmentRepository = assignmentRepository;
        _logger = logger;
    }

    public async Task<Event> CreateEventAsync(Event newEvent)
    {
        // Validate dates
        if (newEvent.StartDate >= newEvent.EndDate)
        {
            throw new DomainException("End date must be after start date.");
        }

        if (newEvent.StartDate < DateTime.UtcNow)
        {
            throw new DomainException("Start date cannot be in the past.");
        }

        newEvent.Status = EventStatus.Requested;
        newEvent.NotificationSent = false;
        newEvent.CreatedAt = DateTime.UtcNow;
        newEvent.UpdatedAt = DateTime.UtcNow;

        // add a shift covering the entire event duration if none exist
        newEvent.Shifts.Add(new Shift
        {
            Name = "Full Event duration",
            StartTime = newEvent.StartDate,
            EndTime = newEvent.EndDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Description = "Auto-generated shift covering the entire event duration",
            RequiredStaff = 1
        });

        var created = await _repository.CreateEventAsync(newEvent);

        return created;
    }

    public async Task<Event> UpdateEventAsync(Event updated)
    {
        // Validate dates
        if (updated.StartDate >= updated.EndDate)
        {
            throw new DomainException("End date must be after start date.");
        }

        // Check if date changes affect existing shifts
        if (updated.Shifts.Any() &&
            (updated.StartDate != updated.StartDate || updated.EndDate != updated.EndDate))
        {
            var conflictingShifts = updated.Shifts.Where(s =>
                s.StartTime < updated.StartDate || s.EndTime > updated.EndDate).ToList();

            if (conflictingShifts.Any())
            {
                throw new DomainException($"Cannot change event dates. {conflictingShifts.Count} shift(s) would fall outside the new event timeframe.");
            }
        }

        // Load current state
        var existing = await _repository.GetEventByIdAsync(updated.Id) ??
            throw new InvalidOperationException($"Event {updated.Id} not found");

        // Apply domain logic
        var decision = _domainService.ApplyChanges(existing, updated);

        // Perform side-effects (email notifications) based on decision
        if (decision.ShouldSendPlannedNotification)
        {
            try
            {
                await _emailService.SendEventPlannedNotificationAsync(existing);
                if (decision.PromoteToConfirmedAfterPlanned)
                {
                    existing.Status = EventStatus.Confirmed;
                    existing.NotificationSent = true;
                }
                _logger.LogInformation("Planned notification sent for Event {EventId}", existing.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed sending planned notification for Event {EventId}", existing.Id);
            }
        }

        if (decision.ShouldSendInvoiceNotification)
        {
            try
            {
                await _emailService.SendEventInvoiceNotificationAsync(existing);
                existing.NotificationSent = true;
                _logger.LogInformation("Invoice notification sent for Event {EventId}", existing.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed sending invoice notification for Event {EventId}", existing.Id);
            }
        }

        // Persist final state
        await _repository.UpdateEventAsync(existing);
        return existing;
    }

    public async Task<Event> GetEventByIdAsync(int id)
    {
        return await _repository.GetEventByIdAsync(id) ??
            throw new InvalidOperationException($"Event {id} not found");
    }

    public async Task<List<Event>> GetAllEventsAsync()
    {
        return await _repository.GetAllEventsAsync();
    }

    public async Task CancelEventAsync(Event eventToCancel)
    {
        if (eventToCancel == null)
        {
            throw new ArgumentNullException(nameof(eventToCancel));
        }

        // Load the existing event with shifts and assignments
        var existing = await _repository.GetEventByIdAsync(eventToCancel.Id) ??
            throw new InvalidOperationException($"Event {eventToCancel.Id} not found");

        if (existing.Status == EventStatus.Cancelled)
        {
            _logger.LogWarning("Event {EventId} is already cancelled.", existing.Id);
            return;
        }

        // Cancel the event
        existing.Status = EventStatus.Cancelled;
        existing.UpdatedAt = DateTime.UtcNow;

        // Cancel all shifts
        foreach (var shift in existing.Shifts)
        {
            shift.Status = ShiftStatus.Cancelled;
            shift.UpdatedAt = DateTime.UtcNow;

            // Cancel all assignments for this shift
            foreach (var assignment in shift.StaffAssignments)
            {
                if (assignment.Status == AssignmentStatus.Assigned || assignment.Status == AssignmentStatus.Confirmed)
                {
                    assignment.Status = AssignmentStatus.Cancelled;
                    assignment.UpdatedAt = DateTime.UtcNow;
                    await _assignmentRepository.UpdateAssignmentAsync(assignment);
                    _logger.LogInformation("Cancelled assignment {AssignmentId} for cancelled event {EventId}", assignment.Id, existing.Id);
                }
            }
        }

        // Persist the event (shifts should be updated via cascade or explicit)
        await _repository.UpdateEventAsync(existing);

        // Send cancellation notification
        try
        {
            await _emailService.SendEventCancellationNotificationAsync(existing);
            _logger.LogInformation("Cancellation notification sent for Event {EventId}", existing.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed sending cancellation notification for Event {EventId}", existing.Id);
        }

        _logger.LogInformation("Event {EventId} has been cancelled.", existing.Id);
    }
}