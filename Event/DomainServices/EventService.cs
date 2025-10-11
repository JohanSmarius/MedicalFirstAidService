using Event.Domain;
using domain = Event.Domain;
using Microsoft.Extensions.Logging;

namespace Event.DomainServices;

internal class EventService : IEventService
{
    private readonly IEventRepository _repository;
    private readonly IStaffAssignmentRepository _assignmentRepository;
    private readonly ILogger<EventService> _logger;
    private readonly EventChangeHandler _domainService = new();
        
    public EventService(
        IEventRepository repository,
        IStaffAssignmentRepository assignmentRepository,
        ILogger<EventService> logger)
    {
        _repository = repository;
        _assignmentRepository = assignmentRepository;
        _logger = logger;
    }

    public async Task<domain.Event> CreateEventAsync(domain.Event newEvent)
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

    public async Task<domain.Event> UpdateEventAsync(domain.Event updated)
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

        // Persist final state
        await _repository.UpdateEventAsync(existing);
        return existing;
    }

    public async Task<domain.Event> GetEventByIdAsync(int id)
    {
        return await _repository.GetEventByIdAsync(id) ??
            throw new InvalidOperationException($"Event {id} not found");
    }

    public async Task<List<domain.Event>> GetAllEventsAsync()
    {
        return await _repository.GetAllEventsAsync();
    }

    public async Task CancelEventAsync(domain.Event eventToCancel)
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

        _logger.LogInformation("Event {EventId} has been cancelled.", existing.Id);
    }
}