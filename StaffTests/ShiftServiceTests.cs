using Domain;
using DomainService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DomainServiceTests;

public class ShiftServiceTests
{
    private readonly Mock<IShiftRepository> _shiftRepositoryMock;
    private readonly Mock<IStaffRepository> _staffRepositoryMock;
    private readonly Mock<IStaffAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<ShiftService>> _loggerMock;
    private readonly ShiftService _shiftService;

    public ShiftServiceTests()
    {
        _shiftRepositoryMock = new Mock<IShiftRepository>();
        _staffRepositoryMock = new Mock<IStaffRepository>();
        _assignmentRepositoryMock = new Mock<IStaffAssignmentRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ShiftService>>();
        _shiftService = new ShiftService(
            _assignmentRepositoryMock.Object,
            _shiftRepositoryMock.Object,
            _staffRepositoryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateShiftAsync_ValidShift_CreatesAndReturnsShift()
    {
        // Arrange
        var newShift = new Shift
        {
            EventId = 1,
            Name = "Test Shift",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            RequiredStaff = 2,
            Description = "Test Description"
        };
        var createdShift = new Shift { Id = 1, Name = "Test Shift" };
        _shiftRepositoryMock.Setup(r => r.CreateShiftAsync(It.IsAny<Shift>())).ReturnsAsync(createdShift);

        // Act
        var result = await _shiftService.CreateShiftAsync(newShift);

        // Assert
        Assert.Equal(createdShift, result);
        Assert.Equal(ShiftStatus.Open, newShift.Status);
        Assert.NotEqual(default, newShift.CreatedAt);
        _shiftRepositoryMock.Verify(r => r.CreateShiftAsync(newShift), Times.Once);
    }

    [Fact]
    public async Task CreateShiftAsync_StartTimeAfterEndTime_ThrowsDomainException()
    {
        // Arrange
        var newShift = new Shift
        {
            EventId = 1,
            Name = "Test Shift",
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _shiftService.CreateShiftAsync(newShift));
        Assert.Equal("End time must be after start time.", exception.Message);
    }

    [Fact]
    public async Task CreateShiftAsync_StartTimeInPast_ThrowsDomainException()
    {
        // Arrange
        var newShift = new Shift
        {
            EventId = 1,
            Name = "Test Shift",
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _shiftService.CreateShiftAsync(newShift));
        Assert.Equal("Start time cannot be in the past.", exception.Message);
    }

    [Fact]
    public async Task GetShiftByIdAsync_ValidId_ReturnsShift()
    {
        // Arrange
        var shift = new Shift { Id = 1, Name = "Test Shift" };
        _shiftRepositoryMock.Setup(r => r.GetShiftByIdAsync(1)).ReturnsAsync(shift);

        // Act
        var result = await _shiftService.GetShiftByIdAsync(1);

        // Assert
        Assert.Equal(shift, result);
        _shiftRepositoryMock.Verify(r => r.GetShiftByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetShiftByIdAsync_ShiftNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _shiftRepositoryMock.Setup(r => r.GetShiftByIdAsync(1)).ReturnsAsync((Shift?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _shiftService.GetShiftByIdAsync(1));
        Assert.Equal("Shift 1 not found", exception.Message);
    }

    [Fact]
    public async Task UpdateShiftAsync_ValidUpdate_UpdatesAndReturnsShift()
    {
        // Arrange
        var existingShift = new Shift
        {
            Id = 1,
            Name = "Existing Shift",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            RequiredStaff = 1
        };
        var updatedShift = new Shift
        {
            Id = 1,
            Name = "Updated Shift",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            RequiredStaff = 2
        };
        _shiftRepositoryMock.Setup(r => r.GetShiftByIdAsync(1)).ReturnsAsync(existingShift);
        _shiftRepositoryMock.Setup(r => r.UpdateShiftAsync(existingShift)).ReturnsAsync(existingShift);

        // Act
        var result = await _shiftService.UpdateShiftAsync(updatedShift);

        // Assert
        Assert.Equal(existingShift, result);
        Assert.Equal("Updated Shift", existingShift.Name);
        Assert.Equal(2, existingShift.RequiredStaff);
        Assert.NotEqual(default, existingShift.UpdatedAt);
        _shiftRepositoryMock.Verify(r => r.GetShiftByIdAsync(1), Times.Once);
        _shiftRepositoryMock.Verify(r => r.UpdateShiftAsync(existingShift), Times.Once);
    }

    [Fact]
    public async Task UpdateShiftAsync_StartTimeAfterEndTime_ThrowsDomainException()
    {
        // Arrange
        var updatedShift = new Shift
        {
            Id = 1,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _shiftService.UpdateShiftAsync(updatedShift));
        Assert.Equal("End time must be after start time.", exception.Message);
    }

    [Fact]
    public async Task UpdateShiftAsync_ShiftNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var updatedShift = new Shift 
        { 
            Id = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };
        _shiftRepositoryMock.Setup(r => r.GetShiftByIdAsync(1)).ReturnsAsync((Shift?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _shiftService.UpdateShiftAsync(updatedShift));
        Assert.Equal("Shift 1 not found", exception.Message);
    }

    [Fact]
    public async Task CancelShiftAsync_ValidShift_CancelsShiftAndAssignments()
    {
        // Arrange
        var shiftToCancel = new Shift { Id = 1 };
        var existingShift = new Shift
        {
            Id = 1,
            Status = ShiftStatus.Open,
            StaffAssignments = new List<StaffAssignment>
            {
                new StaffAssignment { Id = 1, Status = AssignmentStatus.Assigned },
                new StaffAssignment { Id = 2, Status = AssignmentStatus.Confirmed }
            }
        };
        _shiftRepositoryMock.Setup(r => r.GetShiftByIdAsync(1)).ReturnsAsync(existingShift);
        _shiftRepositoryMock.Setup(r => r.UpdateShiftAsync(existingShift)).ReturnsAsync(existingShift);
        _assignmentRepositoryMock.Setup(r => r.UpdateAssignmentAsync(It.IsAny<StaffAssignment>())).ReturnsAsync((StaffAssignment a) => a);

        // Act
        await _shiftService.CancelShiftAsync(shiftToCancel);

        // Assert
        Assert.Equal(ShiftStatus.Cancelled, existingShift.Status);
        Assert.Equal(AssignmentStatus.Cancelled, existingShift.StaffAssignments[0].Status);
        Assert.Equal(AssignmentStatus.Cancelled, existingShift.StaffAssignments[1].Status);
        _shiftRepositoryMock.Verify(r => r.UpdateShiftAsync(existingShift), Times.Once);
        _assignmentRepositoryMock.Verify(r => r.UpdateAssignmentAsync(It.IsAny<StaffAssignment>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CancelShiftAsync_ShiftNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _shiftService.CancelShiftAsync(null!));
    }

    [Fact]
    public async Task CancelShiftAsync_ShiftNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var shiftToCancel = new Shift { Id = 1 };
        _shiftRepositoryMock.Setup(r => r.GetShiftByIdAsync(1)).ReturnsAsync((Shift?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _shiftService.CancelShiftAsync(shiftToCancel));
        Assert.Equal("Shift 1 not found", exception.Message);
    }

    [Fact]
    public async Task CancelShiftAsync_AlreadyCancelled_DoesNothing()
    {
        // Arrange
        var shiftToCancel = new Shift { Id = 1 };
        var existingShift = new Shift { Id = 1, Status = ShiftStatus.Cancelled };
        _shiftRepositoryMock.Setup(r => r.GetShiftByIdAsync(1)).ReturnsAsync(existingShift);

        // Act
        await _shiftService.CancelShiftAsync(shiftToCancel);

        // Assert
        _shiftRepositoryMock.Verify(r => r.UpdateShiftAsync(It.IsAny<Shift>()), Times.Never);
        _assignmentRepositoryMock.Verify(r => r.UpdateAssignmentAsync(It.IsAny<StaffAssignment>()), Times.Never);
    }

    [Fact]
    public async Task AddStaffToShiftAsync_Valid_AddsStaff()
    {
        // Arrange
        var shift = new Shift { Id = 1, StartTime = DateTime.UtcNow.AddHours(1), EndTime = DateTime.UtcNow.AddHours(2), Event = new Event { Id = 1 } };
        var staff = new Staff { Id = 1, FirstName = "John", LastName = "Doe", IsActive = true };
        _assignmentRepositoryMock.Setup(r => r.IsStaffAvailableAsync(1, shift.StartTime, shift.EndTime)).ReturnsAsync(true);
        _assignmentRepositoryMock.Setup(r => r.CreateAssignmentAsync(It.IsAny<StaffAssignment>())).ReturnsAsync(new StaffAssignment());

        // Act
        await _shiftService.AddStaffToShiftAsync(shift, staff);

        // Assert
        _assignmentRepositoryMock.Verify(r => r.CreateAssignmentAsync(It.Is<StaffAssignment>(a => a.ShiftId == 1 && a.StaffId == 1 && a.Status == AssignmentStatus.Assigned)), Times.Once);
        _emailServiceMock.Verify(e => e.SendStaffAssignmentNotificationAsync(staff, shift, shift.Event), Times.Once);
    }

    [Fact]
    public async Task AddStaffToShiftAsync_StaffNotActive_ThrowsDomainException()
    {
        // Arrange
        var shift = new Shift { Id = 1 };
        var staff = new Staff { Id = 1, FirstName = "John", LastName = "Doe", IsActive = false };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _shiftService.AddStaffToShiftAsync(shift, staff));
        Assert.Equal("Staff member John Doe is not active.", exception.Message);
    }

    [Fact]
    public async Task AddStaffToShiftAsync_ShiftInPast_ThrowsDomainException()
    {
        // Arrange
        var shift = new Shift { Id = 1, StartTime = DateTime.UtcNow.AddHours(-1) };
        var staff = new Staff { Id = 1, FirstName = "John", LastName = "Doe", IsActive = true };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _shiftService.AddStaffToShiftAsync(shift, staff));
        Assert.Equal("Cannot assign staff to a shift that has already started.", exception.Message);
    }

    [Fact]
    public async Task AddStaffToShiftAsync_StaffNotAvailable_ThrowsDomainException()
    {
        // Arrange
        var shift = new Shift { Id = 1, StartTime = DateTime.UtcNow.AddHours(1), EndTime = DateTime.UtcNow.AddHours(2) };
        var staff = new Staff { Id = 1, FirstName = "John", LastName = "Doe", IsActive = true };
        _assignmentRepositoryMock.Setup(r => r.IsStaffAvailableAsync(1, shift.StartTime, shift.EndTime)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _shiftService.AddStaffToShiftAsync(shift, staff));
        Assert.Equal("Staff member John Doe is not available for this shift.", exception.Message);
    }

    [Fact]
    public async Task RemoveStaffFromShiftAsync_Valid_RemovesStaff()
    {
        // Arrange
        var shift = new Shift { Id = 1 };
        var staff = new Staff { Id = 1, FirstName = "John", LastName = "Doe" };
        var assignment = new StaffAssignment { Id = 1, StaffId = 1, Status = AssignmentStatus.Assigned };
        _assignmentRepositoryMock.Setup(r => r.GetAssignmentsByShiftIdAsync(1)).ReturnsAsync(new List<StaffAssignment> { assignment });
        _assignmentRepositoryMock.Setup(r => r.UpdateAssignmentAsync(assignment)).ReturnsAsync(assignment);

        // Act
        await _shiftService.RemoveStaffFromShiftAsync(shift, staff);

        // Assert
        Assert.Equal(AssignmentStatus.Cancelled, assignment.Status);
        _assignmentRepositoryMock.Verify(r => r.UpdateAssignmentAsync(assignment), Times.Once);
    }

    [Fact]
    public async Task RemoveStaffFromShiftAsync_StaffNotAssigned_ThrowsDomainException()
    {
        // Arrange
        var shift = new Shift { Id = 1 };
        var staff = new Staff { Id = 1, FirstName = "John", LastName = "Doe" };
        _assignmentRepositoryMock.Setup(r => r.GetAssignmentsByShiftIdAsync(1)).ReturnsAsync(new List<StaffAssignment>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _shiftService.RemoveStaffFromShiftAsync(shift, staff));
        Assert.Equal("Staff member John Doe is not assigned to this shift.", exception.Message);
    }

    [Fact]
    public async Task RemoveStaffFromShiftAsync_AlreadyCheckedIn_ThrowsDomainException()
    {
        // Arrange
        var shift = new Shift { Id = 1 };
        var staff = new Staff { Id = 1, FirstName = "John", LastName = "Doe" };
        var assignment = new StaffAssignment { Id = 1, StaffId = 1, Status = AssignmentStatus.CheckedIn };
        _assignmentRepositoryMock.Setup(r => r.GetAssignmentsByShiftIdAsync(1)).ReturnsAsync(new List<StaffAssignment> { assignment });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _shiftService.RemoveStaffFromShiftAsync(shift, staff));
        Assert.Equal("Cannot remove staff from a shift they have already checked into.", exception.Message);
    }
}