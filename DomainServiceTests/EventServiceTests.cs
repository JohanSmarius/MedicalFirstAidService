using Domain;
using DomainService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DomainServiceTests;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IStaffAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<ILogger<EventService>> _loggerMock;
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _assignmentRepositoryMock = new Mock<IStaffAssignmentRepository>();
        _loggerMock = new Mock<ILogger<EventService>>();
        _eventService = new EventService(
            _eventRepositoryMock.Object,
            _emailServiceMock.Object,
            _assignmentRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateEventAsync_ValidEvent_CreatesAndReturnsEvent()
    {
        // Arrange
        var newEvent = new Event
        {
            Name = "Test Event",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "Test Location"
        };
        var createdEvent = new Event { Id = 1, Name = "Test Event" };
        _eventRepositoryMock.Setup(r => r.CreateEventAsync(It.IsAny<Event>())).ReturnsAsync(createdEvent);

        // Act
        var result = await _eventService.CreateEventAsync(newEvent);

        // Assert
        Assert.Equal(createdEvent, result);
        Assert.Equal(EventStatus.Requested, newEvent.Status);
        Assert.False(newEvent.NotificationSent);
        Assert.NotEqual(default, newEvent.CreatedAt);
        Assert.NotEqual(default, newEvent.UpdatedAt);
        Assert.Single(newEvent.Shifts);
        _eventRepositoryMock.Verify(r => r.CreateEventAsync(newEvent), Times.Once);
    }

    [Fact]
    public async Task CreateEventAsync_StartDateAfterEndDate_ThrowsDomainException()
    {
        // Arrange
        var newEvent = new Event
        {
            Name = "Test Event",
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(1),
            Location = "Test Location"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _eventService.CreateEventAsync(newEvent));
        Assert.Equal("End date must be after start date.", exception.Message);
    }

    [Fact]
    public async Task CreateEventAsync_StartDateInPast_ThrowsDomainException()
    {
        // Arrange
        var newEvent = new Event
        {
            Name = "Test Event",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            Location = "Test Location"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _eventService.CreateEventAsync(newEvent));
        Assert.Equal("Start date cannot be in the past.", exception.Message);
    }

    [Fact]
    public async Task UpdateEventAsync_ValidUpdate_UpdatesAndReturnsEvent()
    {
        // Arrange
        var existingEvent = new Event
        {
            Id = 1,
            Name = "Existing Event",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Status = EventStatus.Requested,
            Shifts = new List<Shift>()
        };
        var updatedEvent = new Event
        {
            Id = 1,
            Name = "Updated Event",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Status = EventStatus.Planned
        };
        _eventRepositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync(existingEvent);
        _eventRepositoryMock.Setup(r => r.UpdateEventAsync(existingEvent)).ReturnsAsync(existingEvent);

        // Act
        var result = await _eventService.UpdateEventAsync(updatedEvent);

        // Assert
        Assert.Equal(existingEvent, result);
        _eventRepositoryMock.Verify(r => r.GetEventByIdAsync(1), Times.Once);
        _eventRepositoryMock.Verify(r => r.UpdateEventAsync(existingEvent), Times.Once);
    }

    [Fact]
    public async Task UpdateEventAsync_StartDateAfterEndDate_ThrowsDomainException()
    {
        // Arrange
        var updatedEvent = new Event
        {
            Id = 1,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _eventService.UpdateEventAsync(updatedEvent));
        Assert.Equal("End date must be after start date.", exception.Message);
    }

    [Fact]
    public async Task UpdateEventAsync_EventNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var updatedEvent = new Event 
        { 
            Id = 1,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2)
        };
        _eventRepositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync((Event?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _eventService.UpdateEventAsync(updatedEvent));
        Assert.Equal("Event 1 not found", exception.Message);
    }

    [Fact]
    public async Task CancelEventAsync_ValidEvent_CancelsEventAndAssignments()
    {
        // Arrange
        var eventToCancel = new Event { Id = 1 };
        var existingEvent = new Event
        {
            Id = 1,
            Status = EventStatus.Confirmed,
            Shifts = new List<Shift>
            {
                new Shift
                {
                    Id = 1,
                    StaffAssignments = new List<StaffAssignment>
                    {
                        new StaffAssignment { Id = 1, Status = AssignmentStatus.Assigned },
                        new StaffAssignment { Id = 2, Status = AssignmentStatus.Confirmed }
                    }
                }
            }
        };
        _eventRepositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync(existingEvent);
        _eventRepositoryMock.Setup(r => r.UpdateEventAsync(existingEvent)).ReturnsAsync(existingEvent);
        _assignmentRepositoryMock.Setup(r => r.UpdateAssignmentAsync(It.IsAny<StaffAssignment>())).ReturnsAsync((StaffAssignment a) => a);

        // Act
        await _eventService.CancelEventAsync(eventToCancel);

        // Assert
        Assert.Equal(EventStatus.Cancelled, existingEvent.Status);
        Assert.Equal(ShiftStatus.Cancelled, existingEvent.Shifts[0].Status);
        Assert.Equal(AssignmentStatus.Cancelled, existingEvent.Shifts[0].StaffAssignments[0].Status);
        Assert.Equal(AssignmentStatus.Cancelled, existingEvent.Shifts[0].StaffAssignments[1].Status);
        _eventRepositoryMock.Verify(r => r.UpdateEventAsync(existingEvent), Times.Once);
        _assignmentRepositoryMock.Verify(r => r.UpdateAssignmentAsync(It.IsAny<StaffAssignment>()), Times.Exactly(2));
        _emailServiceMock.Verify(e => e.SendEventCancellationNotificationAsync(existingEvent), Times.Once);
    }

    [Fact]
    public async Task CancelEventAsync_EventNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _eventService.CancelEventAsync(null!));
    }

    [Fact]
    public async Task CancelEventAsync_EventNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventToCancel = new Event { Id = 1 };
        _eventRepositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync((Event?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _eventService.CancelEventAsync(eventToCancel));
        Assert.Equal("Event 1 not found", exception.Message);
    }

    [Fact]
    public async Task CancelEventAsync_AlreadyCancelled_DoesNothing()
    {
        // Arrange
        var eventToCancel = new Event { Id = 1 };
        var existingEvent = new Event { Id = 1, Status = EventStatus.Cancelled };
        _eventRepositoryMock.Setup(r => r.GetEventByIdAsync(1)).ReturnsAsync(existingEvent);

        // Act
        await _eventService.CancelEventAsync(eventToCancel);

        // Assert
        _eventRepositoryMock.Verify(r => r.UpdateEventAsync(It.IsAny<Event>()), Times.Never);
        _assignmentRepositoryMock.Verify(r => r.UpdateAssignmentAsync(It.IsAny<StaffAssignment>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendEventCancellationNotificationAsync(It.IsAny<Event>()), Times.Never);
    }
}
