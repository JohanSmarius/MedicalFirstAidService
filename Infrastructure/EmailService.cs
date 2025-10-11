using DomainService;
using Microsoft.Extensions.Logging;
using Domain;

namespace Infrastructure;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendStaffAssignmentNotificationAsync(Staff staff, Shift shift, Event @event)
    {
        _logger.LogInformation("Sending staff assignment notification to {StaffName} for Shift {ShiftId} in Event {EventId}", staff.FullName, shift.Id, @event.Id);
        return Task.CompletedTask;
    }

    public Task SendEventPlannedNotificationAsync(Event @event)
    {
        _logger.LogInformation("Sending planned notification for Event {EventId}", @event.Id);
        return Task.CompletedTask;
    }

    public Task SendEventConfirmationNotificationAsync(Event @event)
    {
        _logger.LogInformation("Sending confirmation notification for Event {EventId}", @event.Id);
        return Task.CompletedTask;
    }

    public Task SendEventInvoiceNotificationAsync(Event @event)
    {
        _logger.LogInformation("Sending invoice notification for Event {EventId}", @event.Id);
        return Task.CompletedTask;
    }

    public Task SendEventCancellationNotificationAsync(Event @event)
    {
        _logger.LogInformation("Sending cancellation notification for Event {EventId}", @event.Id);
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);
        return Task.CompletedTask;
    }
}