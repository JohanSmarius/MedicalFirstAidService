using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Event.Domain;
using Event.DomainServices;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Event.Tests
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<IStaffAssignmentRepository> _mockAssignmentRepository;
        private readonly Mock<ILogger<EventService>> _mockLogger;
        private readonly EventService _eventService;

        public EventServiceTests()
        {
            _mockEventRepository = new Mock<IEventRepository>();
            _mockAssignmentRepository = new Mock<IStaffAssignmentRepository>();
            _mockLogger = new Mock<ILogger<EventService>>();
            
            _eventService = new EventService(
                _mockEventRepository.Object,
                _mockAssignmentRepository.Object,
                _mockLogger.Object);
        }

        #region CreateEventAsync Tests

        [Fact]
        public async Task CreateEventAsync_ValidEvent_ReturnsCreatedEvent()
        {
            // Arrange
            var newEvent = new Domain.Event
            {
                Name = "Test Event",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location",
                Description = "Test Description",
                Shifts = new List<Shift>()
            };

            var expectedResult = new Domain.Event
            {
                Id = 1,
                Name = newEvent.Name,
                StartDate = newEvent.StartDate,
                EndDate = newEvent.EndDate,
                Location = newEvent.Location,
                Description = newEvent.Description,
                Status = EventStatus.Requested,
                NotificationSent = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Shifts = new List<Shift> { new Shift() }
            };

            _mockEventRepository
                .Setup(r => r.CreateEventAsync(It.IsAny<Domain.Event>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _eventService.CreateEventAsync(newEvent);

            // Assert
            Assert.Equal(expectedResult, result);
            Assert.Equal(EventStatus.Requested, newEvent.Status);
            Assert.False(newEvent.NotificationSent);
            Assert.NotEmpty(newEvent.Shifts);
            Assert.Equal("Full Event duration", newEvent.Shifts[0].Name);
            _mockEventRepository.Verify(r => r.CreateEventAsync(newEvent), Times.Once);
        }

        [Fact]
        public async Task CreateEventAsync_EndDateBeforeStartDate_ThrowsDomainException()
        {
            // Arrange
            var newEvent = new Domain.Event
            {
                Name = "Test Event",
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(1),
                Location = "Test Location"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() => 
                _eventService.CreateEventAsync(newEvent));
            
            Assert.Equal("End date must be after start date.", exception.Message);
            _mockEventRepository.Verify(r => r.CreateEventAsync(It.IsAny<Domain.Event>()), Times.Never);
        }

        [Fact]
        public async Task CreateEventAsync_StartDateInPast_ThrowsDomainException()
        {
            // Arrange
            var newEvent = new Domain.Event
            {
                Name = "Test Event",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                Location = "Test Location"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() => 
                _eventService.CreateEventAsync(newEvent));
            
            Assert.Equal("Start date cannot be in the past.", exception.Message);
            _mockEventRepository.Verify(r => r.CreateEventAsync(It.IsAny<Domain.Event>()), Times.Never);
        }
        
        #endregion

        #region UpdateEventAsync Tests
        
        [Fact]
        public async Task UpdateEventAsync_ValidEvent_ReturnsUpdatedEvent()
        {
            // Arrange
            var existingEvent = new Domain.Event
            {
                Id = 1,
                Name = "Original Name",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Original Location",
                Status = EventStatus.Requested,
                Shifts = new List<Shift>()
            };

            var updatedEvent = new Domain.Event
            {
                Id = 1,
                Name = "Updated Name",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Updated Location",
                Status = EventStatus.Planned,
                Shifts = new List<Shift>()
            };

            _mockEventRepository
                .Setup(r => r.GetEventByIdAsync(1))
                .ReturnsAsync(existingEvent);
                
            _mockEventRepository
                .Setup(r => r.UpdateEventAsync(It.IsAny<Domain.Event>()))
                .ReturnsAsync(existingEvent);

            // Act
            var result = await _eventService.UpdateEventAsync(updatedEvent);

            // Assert
            Assert.Equal(existingEvent, result);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(1), Times.Once);
            _mockEventRepository.Verify(r => r.UpdateEventAsync(existingEvent), Times.Once);
        }
        
        [Fact]
        public async Task UpdateEventAsync_EndDateBeforeStartDate_ThrowsDomainException()
        {
            // Arrange
            var updatedEvent = new Domain.Event
            {
                Id = 1,
                Name = "Test Event",
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(1),
                Location = "Test Location"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() => 
                _eventService.UpdateEventAsync(updatedEvent));
            
            Assert.Equal("End date must be after start date.", exception.Message);
            _mockEventRepository.Verify(r => r.UpdateEventAsync(It.IsAny<Domain.Event>()), Times.Never);
        }
        
        [Fact]
        public async Task UpdateEventAsync_EventNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var updatedEvent = new Domain.Event
            {
                Id = 999,
                Name = "Test Event",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location"
            };

            _mockEventRepository
                .Setup(r => r.GetEventByIdAsync(999))
                .ReturnsAsync((Domain.Event)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _eventService.UpdateEventAsync(updatedEvent));
            
            Assert.Equal($"Event {updatedEvent.Id} not found", exception.Message);
            _mockEventRepository.Verify(r => r.UpdateEventAsync(It.IsAny<Domain.Event>()), Times.Never);
        }
        
        [Fact]
        public async Task UpdateEventAsync_HasConflictingShifts_ThrowsDomainException()
        {
            // Arrange
            var baseTime = DateTime.UtcNow;
            
            // Create an existing event with shifts
            var existingEvent = new Domain.Event
            {
                Id = 1,
                Name = "Original Name",
                StartDate = baseTime.AddDays(1),
                EndDate = baseTime.AddDays(3),
                Location = "Original Location",
                Status = EventStatus.Requested,
                Shifts = new List<Shift>
                {
                    new Shift 
                    { 
                        Id = 1, 
                        StartTime = baseTime.AddDays(1), 
                        EndTime = baseTime.AddDays(3) 
                    }
                }
            };
            
            // Setup the updated event with date changes that would cause a conflict
            var updatedEvent = new Domain.Event
            {
                Id = 1,
                Name = "Updated Name",
                StartDate = baseTime.AddDays(1).AddHours(12), // Later start date than the shift starts
                EndDate = baseTime.AddDays(3),
                Location = "Updated Location",
                Status = EventStatus.Planned,
                Shifts = new List<Shift>
                {
                    // Same shift as in existing event
                    existingEvent.Shifts[0]
                }
            };

            // Setup the repository to return the existing event
            _mockEventRepository
                .Setup(r => r.GetEventByIdAsync(1))
                .ReturnsAsync(existingEvent);
                
            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() => 
                _eventService.UpdateEventAsync(updatedEvent));
            
            Assert.Contains("shift(s) would fall outside the new event timeframe", exception.Message);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(1), Times.Once);
            _mockEventRepository.Verify(r => r.UpdateEventAsync(It.IsAny<Domain.Event>()), Times.Never);
        }
        
        #endregion

        #region GetEventByIdAsync Tests
        
        [Fact]
        public async Task GetEventByIdAsync_ExistingEvent_ReturnsEvent()
        {
            // Arrange
            var existingEvent = new Domain.Event
            {
                Id = 1,
                Name = "Test Event",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Location = "Test Location",
                Status = EventStatus.Requested
            };

            _mockEventRepository
                .Setup(r => r.GetEventByIdAsync(1))
                .ReturnsAsync(existingEvent);

            // Act
            var result = await _eventService.GetEventByIdAsync(1);

            // Assert
            Assert.Equal(existingEvent, result);
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(1), Times.Once);
        }
        
        [Fact]
        public async Task GetEventByIdAsync_NonExistingEvent_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockEventRepository
                .Setup(r => r.GetEventByIdAsync(999))
                .ReturnsAsync((Domain.Event)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _eventService.GetEventByIdAsync(999));
            
            Assert.Equal("Event 999 not found", exception.Message);
        }
        
        #endregion

        #region GetAllEventsAsync Tests
        
        [Fact]
        public async Task GetAllEventsAsync_ReturnsAllEvents()
        {
            // Arrange
            var events = new List<Domain.Event>
            {
                new Domain.Event { Id = 1, Name = "Event 1" },
                new Domain.Event { Id = 2, Name = "Event 2" },
                new Domain.Event { Id = 3, Name = "Event 3" }
            };

            _mockEventRepository
                .Setup(r => r.GetAllEventsAsync())
                .ReturnsAsync(events);

            // Act
            var result = await _eventService.GetAllEventsAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(events, result);
            _mockEventRepository.Verify(r => r.GetAllEventsAsync(), Times.Once);
        }
        
        #endregion

        #region CancelEventAsync Tests
        
        [Fact]
        public async Task CancelEventAsync_ExistingEvent_CancelsEventAndAllShiftsAndAssignments()
        {
            // Arrange
            var assignment1 = new StaffAssignment { Id = 1, Status = AssignmentStatus.Assigned };
            var assignment2 = new StaffAssignment { Id = 2, Status = AssignmentStatus.Confirmed };
            
            var shift = new Shift
            {
                Id = 1,
                Status = ShiftStatus.Open,
                StaffAssignments = new List<StaffAssignment> { assignment1, assignment2 }
            };
            
            var existingEvent = new Domain.Event
            {
                Id = 1,
                Name = "Test Event",
                Status = EventStatus.Planned,
                Shifts = new List<Shift> { shift }
            };

            _mockEventRepository
                .Setup(r => r.GetEventByIdAsync(1))
                .ReturnsAsync(existingEvent);
                
            _mockEventRepository
                .Setup(r => r.UpdateEventAsync(It.IsAny<Domain.Event>()))
                .ReturnsAsync(existingEvent);
                
            _mockAssignmentRepository
                .Setup(r => r.UpdateAssignmentAsync(It.IsAny<StaffAssignment>()))
                .ReturnsAsync((StaffAssignment sa) => sa);
                
            // Act
            await _eventService.CancelEventAsync(new Domain.Event { Id = 1 });

            // Assert
            Assert.Equal(EventStatus.Cancelled, existingEvent.Status);
            Assert.Equal(ShiftStatus.Cancelled, shift.Status);
            Assert.Equal(AssignmentStatus.Cancelled, assignment1.Status);
            Assert.Equal(AssignmentStatus.Cancelled, assignment2.Status);
            
            _mockEventRepository.Verify(r => r.GetEventByIdAsync(1), Times.Once);
            _mockEventRepository.Verify(r => r.UpdateEventAsync(existingEvent), Times.Once);
            _mockAssignmentRepository.Verify(r => r.UpdateAssignmentAsync(It.IsAny<StaffAssignment>()), Times.Exactly(2));
        }
        
        [Fact]
        public async Task CancelEventAsync_AlreadyCancelledEvent_DoesNothing()
        {
            // Arrange
            var existingEvent = new Domain.Event
            {
                Id = 1,
                Name = "Test Event",
                Status = EventStatus.Cancelled
            };

            _mockEventRepository
                .Setup(r => r.GetEventByIdAsync(1))
                .ReturnsAsync(existingEvent);

            // Act
            await _eventService.CancelEventAsync(new Domain.Event { Id = 1 });

            // Assert
            _mockEventRepository.Verify(r => r.UpdateEventAsync(It.IsAny<Domain.Event>()), Times.Never);
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("already cancelled")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }
        
        [Fact]
        public async Task CancelEventAsync_EventNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockEventRepository
                .Setup(r => r.GetEventByIdAsync(999))
                .ReturnsAsync((Domain.Event)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _eventService.CancelEventAsync(new Domain.Event { Id = 999 }));
            
            Assert.Equal("Event 999 not found", exception.Message);
            _mockEventRepository.Verify(r => r.UpdateEventAsync(It.IsAny<Domain.Event>()), Times.Never);
        }
        
        [Fact]
        public async Task CancelEventAsync_NullEvent_ThrowsArgumentNullException()
        {
            // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _eventService.CancelEventAsync(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            
            Assert.Equal("eventToCancel", exception.ParamName);
        }
        
        #endregion
    }
}