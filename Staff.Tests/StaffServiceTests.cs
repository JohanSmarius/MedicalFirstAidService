using Moq;
using Staff.Domain;
using Staff.DomainServices;
using StaffEvents;
using Microsoft.Extensions.Logging;
using MassTransit;
using Xunit;

namespace Staff.Tests;

public class StaffServiceTests
{
    private readonly Mock<IStaffRepository> _staffRepositoryMock;
    private readonly Mock<ILogger<StaffService>> _loggerMock;
    private readonly Mock<IBus> _busMock;
    private readonly StaffService _staffService;

    public StaffServiceTests()
    {
        _staffRepositoryMock = new Mock<IStaffRepository>();
        _loggerMock = new Mock<ILogger<StaffService>>();
        _busMock = new Mock<IBus>();
        _staffService = new StaffService(_staffRepositoryMock.Object, _loggerMock.Object, _busMock.Object);
    }

    [Fact]
    public async Task Given_ValidStaff_When_CreateStaffAsync_Then_StaffIsCreatedAndEventIsPublished()
    {
        // Arrange
        var newStaff = new Domain.Staff
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Role = StaffRole.FirstAider
        };

        _staffRepositoryMock.Setup(r => r.IsEmailUniqueAsync(newStaff.Email))
            .ReturnsAsync(true);
        
        _staffRepositoryMock.Setup(r => r.CreateStaffAsync(It.IsAny<Domain.Staff>()))
            .ReturnsAsync((Domain.Staff staff) => { staff.Id = 1; return staff; });

        // Act
        var result = await _staffService.CreateStaffAsync(newStaff);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.True(result.IsActive);
        Assert.NotEqual(default, result.CreatedAt);
        
        _staffRepositoryMock.Verify(r => r.CreateStaffAsync(It.IsAny<Domain.Staff>()), Times.Once);
        _busMock.Verify(b => b.Publish(It.Is<StafCreatedEvent>(e => e.Id == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Given_ExistingEmail_When_CreateStaffAsync_Then_ThrowsDomainException()
    {
        // Arrange
        var newStaff = new Domain.Staff
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com"
        };

        _staffRepositoryMock.Setup(r => r.IsEmailUniqueAsync(newStaff.Email))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _staffService.CreateStaffAsync(newStaff));
        
        Assert.Equal("Email address is already in use.", exception.Message);
        _staffRepositoryMock.Verify(r => r.CreateStaffAsync(It.IsAny<Domain.Staff>()), Times.Never);
        _busMock.Verify(b => b.Publish(It.IsAny<StafCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Given_ValidStaffId_When_GetStaffByIdAsync_Then_ReturnsStaff()
    {
        // Arrange
        var staffId = 1;
        var existingStaff = new Domain.Staff
        {
            Id = staffId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        _staffRepositoryMock.Setup(r => r.GetStaffByIdAsync(staffId))
            .ReturnsAsync(existingStaff);

        // Act
        var result = await _staffService.GetStaffByIdAsync(staffId);

        // Assert
        Assert.Equal(staffId, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        _staffRepositoryMock.Verify(r => r.GetStaffByIdAsync(staffId), Times.Once);
    }

    [Fact]
    public async Task Given_InvalidStaffId_When_GetStaffByIdAsync_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var staffId = 999;
        _staffRepositoryMock.Setup(r => r.GetStaffByIdAsync(staffId))
            .ReturnsAsync((Domain.Staff)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _staffService.GetStaffByIdAsync(staffId));
        
        Assert.Equal($"Staff {staffId} not found", exception.Message);
    }

    [Fact]
    public async Task Given_ValidRequest_When_GetAllStaffAsync_Then_ReturnsAllStaff()
    {
        // Arrange
        var staffList = new List<Domain.Staff>
        {
            new Domain.Staff { Id = 1, FirstName = "John", LastName = "Doe" },
            new Domain.Staff { Id = 2, FirstName = "Jane", LastName = "Smith" }
        };

        _staffRepositoryMock.Setup(r => r.GetAllStaffAsync())
            .ReturnsAsync(staffList);

        // Act
        var result = await _staffService.GetAllStaffAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Id == 1 && s.FirstName == "John");
        Assert.Contains(result, s => s.Id == 2 && s.FirstName == "Jane");
        _staffRepositoryMock.Verify(r => r.GetAllStaffAsync(), Times.Once);
    }

    [Fact]
    public async Task Given_ValidStaff_When_UpdateStaffAsync_Then_StaffIsUpdated()
    {
        // Arrange
        var staffId = 1;
        var updatedStaff = new Domain.Staff
        {
            Id = staffId,
            FirstName = "John",
            LastName = "Updated",
            Email = "john.updated@example.com",
            Role = StaffRole.TeamLeader
        };

        var existingStaff = new Domain.Staff
        {
            Id = staffId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Role = StaffRole.FirstAider,
            IsActive = true
        };

        _staffRepositoryMock.Setup(r => r.IsEmailUniqueAsync(updatedStaff.Email, updatedStaff.Id))
            .ReturnsAsync(true);
        
        _staffRepositoryMock.Setup(r => r.GetStaffByIdAsync(staffId))
            .ReturnsAsync(existingStaff);

        _staffRepositoryMock.Setup(r => r.UpdateStaffAsync(It.IsAny<Domain.Staff>()))
            .ReturnsAsync((Domain.Staff staff) => staff);

        // Act
        var result = await _staffService.UpdateStaffAsync(updatedStaff);

        // Assert
        Assert.Equal(staffId, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Updated", result.LastName);
        Assert.Equal("john.updated@example.com", result.Email);
        Assert.Equal(StaffRole.TeamLeader, result.Role);
        Assert.NotEqual(default, result.UpdatedAt);
        
        _staffRepositoryMock.Verify(r => r.UpdateStaffAsync(It.IsAny<Domain.Staff>()), Times.Once);
    }

    [Fact]
    public async Task Given_ExistingEmail_When_UpdateStaffAsync_Then_ThrowsDomainException()
    {
        // Arrange
        var staffId = 1;
        var updatedStaff = new Domain.Staff
        {
            Id = staffId,
            FirstName = "John",
            LastName = "Doe",
            Email = "taken@example.com"
        };

        _staffRepositoryMock.Setup(r => r.IsEmailUniqueAsync(updatedStaff.Email, updatedStaff.Id))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _staffService.UpdateStaffAsync(updatedStaff));
        
        Assert.Equal("Email address is already in use.", exception.Message);
        _staffRepositoryMock.Verify(r => r.UpdateStaffAsync(It.IsAny<Domain.Staff>()), Times.Never);
    }

    [Fact]
    public async Task Given_InvalidStaffId_When_UpdateStaffAsync_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var staffId = 999;
        var updatedStaff = new Domain.Staff
        {
            Id = staffId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        _staffRepositoryMock.Setup(r => r.IsEmailUniqueAsync(updatedStaff.Email, updatedStaff.Id))
            .ReturnsAsync(true);
        
        _staffRepositoryMock.Setup(r => r.GetStaffByIdAsync(staffId))
            .ReturnsAsync((Domain.Staff)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _staffService.UpdateStaffAsync(updatedStaff));
        
        Assert.Equal($"Staff {staffId} not found", exception.Message);
        _staffRepositoryMock.Verify(r => r.UpdateStaffAsync(It.IsAny<Domain.Staff>()), Times.Never);
    }

    [Fact]
    public async Task Given_ActiveStaff_When_ResignStaffAsync_Then_StaffIsDeactivatedAndEventIsPublished()
    {
        // Arrange
        var staffId = 1;
        var staff = new Domain.Staff
        {
            Id = staffId,
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };

        _staffRepositoryMock.Setup(r => r.UpdateStaffAsync(It.IsAny<Domain.Staff>()))
            .ReturnsAsync((Domain.Staff s) => s);

        // Act
        await _staffService.ResignStaffAsync(staff);

        // Assert
        Assert.False(staff.IsActive);
        Assert.NotEqual(default, staff.UpdatedAt);
        
        _staffRepositoryMock.Verify(r => r.UpdateStaffAsync(staff), Times.Once);
        _busMock.Verify(b => b.Publish(It.Is<StaffResignedEvent>(e => e.Id == staffId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Given_InactiveStaff_When_ResignStaffAsync_Then_DoesNothing()
    {
        // Arrange
        var staffId = 1;
        var staff = new Domain.Staff
        {
            Id = staffId,
            FirstName = "John",
            LastName = "Doe",
            IsActive = false
        };

        // Act
        await _staffService.ResignStaffAsync(staff);

        // Assert
        Assert.False(staff.IsActive);
        _staffRepositoryMock.Verify(r => r.UpdateStaffAsync(It.IsAny<Domain.Staff>()), Times.Never);
        _busMock.Verify(b => b.Publish(It.IsAny<StaffResignedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}