using App.Domain.Model;

public class ProjectTest
{
    [Fact]
    public void AddMember_ShouldThrow_WhenMemberExists()
    {
        var userId = Guid.NewGuid();
        var project = SetupProject();
        var role = ProjectRole.Viewer;

        project.AddMember(userId, role);

        Assert.Throws<InvalidOperationException>(() => project.AddMember(userId, role));
    }

    [Fact]
    public void RemoveMember_ShouldRemoveMember_WhenMemberExists()
    {
        var project = SetupProject();
        var userId = Guid.NewGuid();
        project.AddMember(userId, ProjectRole.Viewer);

        project.RemoveMember(userId);

        Assert.Empty(project.Members);
    }

    [Fact]
    public void RemoveMember_ShouldThrowInvalidOperationException_WhenMemberDoesNotExist()
    {
        var project = SetupProject();
        var userId = Guid.NewGuid();

        var exception = Assert.Throws<InvalidOperationException>(
            () => project.RemoveMember(userId)
        );

        Assert.Equal("User is not a member of this project", exception.Message);
    }
    private static Project SetupProject()
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            Description = "Test Description",
            WorkspaceId = Guid.NewGuid(),
            CreatedByUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            IsArchived = false
        };
    }
};