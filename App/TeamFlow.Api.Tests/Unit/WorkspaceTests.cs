using App.Domain.Model;
using Xunit;

namespace App.Tests.Unit;

public class WorkspaceTests
{
    #region Archive Tests

    [Fact]
    public void Archive_ShouldSetIsArchivedToTrue()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        workspace.IsArchived = false;
        
        // Act
        workspace.Archive();
        
        // Assert
        Assert.True(workspace.IsArchived);
    }

    [Fact]
    public void Archive_ShouldWork_WhenAlreadyArchived()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        workspace.IsArchived = true;
        
        // Act
        workspace.Archive();
        
        // Assert
        Assert.True(workspace.IsArchived);
    }

    #endregion

    #region AddMember Tests

    [Fact]
    public void AddMember_ShouldAddMember_WhenMemberDoesNotExist()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        var userId = Guid.NewGuid();
        var role = WorkspaceRole.Viewer;
        
        // Act
        workspace.AddMember(userId, role);
        
        // Assert
        Assert.Single(workspace.Members);
        Assert.Contains(workspace.Members, m => m.UserId == userId && m.Role == role);
    }

    [Fact]
    public void AddMember_ShouldThrowInvalidOperationException_WhenMemberAlreadyExists()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        var userId = Guid.NewGuid();
        var role = WorkspaceRole.Viewer;
        
        workspace.AddMember(userId, role);
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => workspace.AddMember(userId, role)
        );
        
        Assert.Equal("User is already a member of this workspace", exception.Message);
    }

    [Fact]
    public void AddMember_ShouldAddMultipleMembers_WhenDifferentUsers()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();
        
        // Act
        workspace.AddMember(userId1, WorkspaceRole.Viewer);
        workspace.AddMember(userId2, WorkspaceRole.Member);
        workspace.AddMember(userId3, WorkspaceRole.Admin);
        
        // Assert
        Assert.Equal(3, workspace.Members.Count);
        Assert.Contains(workspace.Members, m => m.UserId == userId1);
        Assert.Contains(workspace.Members, m => m.UserId == userId2);
        Assert.Contains(workspace.Members, m => m.UserId == userId3);
    }

    [Fact]
    public void AddMember_ShouldAllowReAddingPreviouslyRemovedMember()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        var userId = Guid.NewGuid();
        
        workspace.AddMember(userId, WorkspaceRole.Viewer);
        workspace.RemoveMember(userId);
        
        // Act
        workspace.AddMember(userId, WorkspaceRole.Admin);
        
        // Assert
        Assert.Equal(2, workspace.Members.Count);
        Assert.Single(workspace.Members.Where(m => m.UserId == userId && m.RemovedAt == null));
        Assert.Equal(WorkspaceRole.Admin, 
            workspace.Members.First(m => m.UserId == userId && m.RemovedAt == null).Role);
    }

    #endregion

    #region RemoveMember Tests

    [Fact]
    public void RemoveMember_ShouldSetRemovedAt_WhenMemberExists()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        var userId = Guid.NewGuid();
        workspace.AddMember(userId, WorkspaceRole.Viewer);
        var beforeRemove = DateTime.UtcNow.AddSeconds(-1);
        
        // Act
        workspace.RemoveMember(userId);
        var afterRemove = DateTime.UtcNow.AddSeconds(1);
        
        // Assert
        var member = workspace.Members.First();
        Assert.NotNull(member.RemovedAt);
        Assert.InRange(member.RemovedAt.Value, beforeRemove, afterRemove);
    }

    [Fact]
    public void RemoveMember_ShouldThrowInvalidOperationException_WhenMemberDoesNotExist()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        var userId = Guid.NewGuid();
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => workspace.RemoveMember(userId)
        );
        
        Assert.Equal("User is not a member of this workspace", exception.Message);
    }

    [Fact]
    public void RemoveMember_ShouldThrow_WhenRemovingAlreadyRemovedMember()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        var userId = Guid.NewGuid();
        workspace.AddMember(userId, WorkspaceRole.Viewer);
        workspace.RemoveMember(userId);
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => workspace.RemoveMember(userId)
        );
    }

    [Fact]
    public void RemoveMember_ShouldOnlyRemoveSpecifiedMember_WhenMultipleMembersExist()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();
        
        workspace.AddMember(userId1, WorkspaceRole.Viewer);
        workspace.AddMember(userId2, WorkspaceRole.Member);
        workspace.AddMember(userId3, WorkspaceRole.Admin);
        
        // Act
        workspace.RemoveMember(userId2);
        
        // Assert
        Assert.Equal(3, workspace.Members.Count); // All members still in collection
        Assert.Single(workspace.Members.Where(m => m.RemovedAt != null)); // But only one removed
        Assert.NotNull(workspace.Members.First(m => m.UserId == userId2).RemovedAt);
        Assert.Null(workspace.Members.First(m => m.UserId == userId1).RemovedAt);
        Assert.Null(workspace.Members.First(m => m.UserId == userId3).RemovedAt);
    }

    [Fact]
    public void RemoveMember_ShouldKeepMemberInCollection()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        var userId = Guid.NewGuid();
        workspace.AddMember(userId, WorkspaceRole.Viewer);
        
        // Act
        workspace.RemoveMember(userId);
        
        // Assert
        Assert.Single(workspace.Members); // Member still in collection
        Assert.NotNull(workspace.Members.First().RemovedAt); // But marked as removed
    }

    #endregion

    #region UpdateMemberRole Tests

    [Fact]
    public void UpdateMemberRole_ShouldUpdateRole_WhenMemberExists()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        var userId = Guid.NewGuid();
        workspace.AddMember(userId, WorkspaceRole.Viewer);
        
        // Act
        workspace.UpdateMemberRole(userId, WorkspaceRole.Admin);
        
        // Assert
        var member = workspace.Members.First();
        Assert.Equal(WorkspaceRole.Admin, member.Role);
    }

    [Fact]
    public void UpdateMemberRole_ShouldThrowInvalidOperationException_WhenMemberDoesNotExist()
    {
        // Arrange
        var workspace = CreateValidWorkspace();
        var userId = Guid.NewGuid();
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => workspace.UpdateMemberRole(userId, WorkspaceRole.Admin)
        );
        
        Assert.Equal("User is not a member of this workspace", exception.Message);
    }

    [Fact]
    public void UpdateMemberRole_ShouldOnlyUpdateSpecifiedMember_WhenMultipleMembersExist()
    {
        var workspace = CreateValidWorkspace();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();
        
        workspace.AddMember(userId1, WorkspaceRole.Viewer);
        workspace.AddMember(userId2, WorkspaceRole.Viewer);
        workspace.AddMember(userId3, WorkspaceRole.Viewer);
        
        workspace.UpdateMemberRole(userId2, WorkspaceRole.Admin);
        
        Assert.Equal(WorkspaceRole.Viewer, workspace.Members.First(m => m.UserId == userId1).Role);
        Assert.Equal(WorkspaceRole.Admin, workspace.Members.First(m => m.UserId == userId2).Role);
        Assert.Equal(WorkspaceRole.Viewer, workspace.Members.First(m => m.UserId == userId3).Role);
    }

    #endregion

    #region Helper Methods

    private static Workspace CreateValidWorkspace()
    {
        return new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "Test Workspace",
            Description = "Test Description",
            OwnerId = Guid.NewGuid(),
            Code = "TEST123",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}