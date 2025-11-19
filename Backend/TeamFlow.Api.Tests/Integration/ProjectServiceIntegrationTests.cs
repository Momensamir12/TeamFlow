using App.Application.Dto;
using App.Application.Interfaces;
using App.Application.Services;
using App.Domain.Model;
using App.Infrastructure.Authorization;
using App.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace TeamFlow.Api.Tests.Integration;

public class ProjectServiceIntegrationTests : BaseIntegrationTest
{
    private readonly ProjectService _projectService;
    private readonly Mock<IWorkspaceAuthorizer> _workspaceAuthorizerMock;
    private readonly Mock<IProjectAuthorizer> _projectAuthorizerMock;
    private readonly Workspace _testWorkspace;

    public ProjectServiceIntegrationTests()
    {
        var projectRepository = new EFProjectRepository(AppDbContext);
        var workspaceRepository = new EFWorkspaceRepository(AppDbContext);
        var userRepository = new EFUserRepository(AppDbContext);
        var mediatorMock = new Mock<MediatR.IMediator>();
        var domainEventDispatcher = new App.Application.EventDispatcher.DomainEventDispatcher(mediatorMock.Object);
        var taskRepository = new EFTaskRepository(AppDbContext, domainEventDispatcher);
        
        _workspaceAuthorizerMock = new Mock<IWorkspaceAuthorizer>();
        _projectAuthorizerMock = new Mock<IProjectAuthorizer>();
        var userValidator = new UserValidator(userRepository);

        _projectService = new ProjectService(projectRepository, workspaceRepository, userRepository, taskRepository,
            _workspaceAuthorizerMock.Object, _projectAuthorizerMock.Object, userValidator, Mapper);

        _testWorkspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "Test Workspace",
            Description = "Test workspace description",
            OwnerId = TestUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        AppDbContext.Workspaces.Add(_testWorkspace);
        AppDbContext.SaveChanges();

        // Setup default mock behaviors for happy path
        _workspaceAuthorizerMock.Setup(a => a.EnsureIsMemberAsync(It.IsAny<Workspace>())).Returns(Task.CompletedTask);
        _workspaceAuthorizerMock.Setup(a => a.EnsureHasAccessAsync(It.IsAny<Workspace>())).Returns(Task.CompletedTask);
        _projectAuthorizerMock.Setup(a => a.EnsureHasAccessAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);
        _projectAuthorizerMock.Setup(a => a.EnsureIsAdminAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);
        _projectAuthorizerMock.Setup(a => a.HasAccessAsync(It.IsAny<Project>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task CreateProject_WithValidData_ShouldCreateProjectAndAddCreatorAsAdmin()
    {
        // Arrange
        var createDto = new CreateProjectDto
        {
            Name = "Test Project",
            Description = "Test Description",
            WorkspaceId = _testWorkspace.Id
        };

        // Act
        await _projectService.CreateProjectAsync(createDto, TestUser.Id);

        // Assert
        var projectInDb = await AppDbContext.Projects.Include(p => p.Members).FirstOrDefaultAsync();
        Assert.NotNull(projectInDb);
        Assert.Equal(createDto.Name, projectInDb.Name);
        Assert.Equal(createDto.Description, projectInDb.Description);
        Assert.Equal(_testWorkspace.Id, projectInDb.WorkspaceId);
        Assert.Equal(TestUser.Id, projectInDb.CreatedByUserId);
        Assert.Contains(projectInDb.Members, m => m.UserId == TestUser.Id && m.Role == ProjectRole.Admin);
    }

    [Fact]
    public async Task CreateProject_WithNonExistentWorkspace_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateProjectDto
        {
            Name = "Test Project",
            Description = "Test Description",
            WorkspaceId = Guid.NewGuid() // Non-existent workspace
        };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _projectService.CreateProjectAsync(createDto, TestUser.Id));
    }

    [Fact]
    public async Task CreateProject_WhenUserIsNotWorkspaceMember_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var createDto = new CreateProjectDto
        {
            Name = "Test Project",
            Description = "Test Description",
            WorkspaceId = _testWorkspace.Id
        };
        
        _workspaceAuthorizerMock.Setup(a => a.EnsureIsMemberAsync(It.IsAny<Workspace>()))
            .ThrowsAsync(new UnauthorizedAccessException("Only workspace members can perform this action"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _projectService.CreateProjectAsync(createDto, TestUser.Id));
    }

    [Fact]
    public async Task UpdateProject_WithValidData_ShouldUpdateProject()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Description = "Original Description",
            WorkspaceId = _testWorkspace.Id,
            CreatedByUserId = TestUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        project.AddMember(TestUser.Id, ProjectRole.Admin);
        AppDbContext.Projects.Add(project);
        AppDbContext.SaveChanges();

        var updateDto = new UpdateProjectDto
        {
            ProjectId = project.Id,
            Name = "Updated Name",
            Description = "Updated Description"
        };

        // Act
        await _projectService.UpdateProjectAsync(updateDto, TestUser.Id);

        // Assert
        var updatedProject = await AppDbContext.Projects.FindAsync(project.Id);
        Assert.NotNull(updatedProject);
        Assert.Equal("Updated Name", updatedProject.Name);
        Assert.Equal("Updated Description", updatedProject.Description);
    }

    [Fact]
    public async Task UpdateProject_WhenUserIsNotAdmin_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = _testWorkspace.Id,
            CreatedByUserId = TestUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        AppDbContext.Projects.Add(project);
        AppDbContext.SaveChanges();

        var updateDto = new UpdateProjectDto
        {
            ProjectId = project.Id,
            Name = "Updated Name"
        };

        _projectAuthorizerMock.Setup(a => a.EnsureIsAdminAsync(It.IsAny<Project>()))
            .ThrowsAsync(new UnauthorizedAccessException("Only project admins can perform this action"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _projectService.UpdateProjectAsync(updateDto, TestUser.Id));
    }

    [Fact]
    public async Task AddMemberToProject_WithWorkspaceMember_ShouldAddMember()
    {
        // Arrange
        var newUser = TestUserFactory.CreateTestUser("newuser", "newuser@example.com");
        AppDbContext.Users.Add(newUser);
        
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            Description = "Description",
            WorkspaceId = _testWorkspace.Id,
            CreatedByUserId = TestUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        project.AddMember(TestUser.Id, ProjectRole.Admin);
        AppDbContext.Projects.Add(project);
        
        var workspaceMember = new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            WorkspaceId = _testWorkspace.Id,
            UserId = newUser.Id,
            Role = WorkspaceRole.Member,
            JoinedAt = DateTime.UtcNow
        };
        AppDbContext.WorkspaceMembers.Add(workspaceMember);
        AppDbContext.SaveChanges();

        var addMemberDto = new AddProjectMemberDto
        {
            ProjectId = project.Id,
            UserId = newUser.Id,
            Role = ProjectRole.Member
        };

        // Act
        await _projectService.AddMemberToProjectAsync(addMemberDto, TestUser.Id);

        // Assert
        var updatedProject = await AppDbContext.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == project.Id);
        var member = updatedProject?.Members.FirstOrDefault(m => m.UserId == newUser.Id);
        Assert.NotNull(member);
        Assert.Equal(ProjectRole.Member, member.Role);
    }

    [Fact]
    public async Task AddMemberToProject_WhenUserNotInWorkspace_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var newUser = TestUserFactory.CreateTestUser("newuser", "newuser@example.com");
        AppDbContext.Users.Add(newUser);
        
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = _testWorkspace.Id,
            CreatedByUserId = TestUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        project.AddMember(TestUser.Id, ProjectRole.Admin);
        AppDbContext.Projects.Add(project);
        AppDbContext.SaveChanges();

        var addMemberDto = new AddProjectMemberDto
        {
            ProjectId = project.Id,
            UserId = newUser.Id,
            Role = ProjectRole.Member
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _projectService.AddMemberToProjectAsync(addMemberDto, TestUser.Id));
    }

    [Fact]
    public async Task RemoveMemberFromProject_ShouldRemoveMember()
    {
        // Arrange
        var memberUser = TestUserFactory.CreateTestUser("memberuser", "member@example.com");
        AppDbContext.Users.Add(memberUser);
        
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = _testWorkspace.Id,
            CreatedByUserId = TestUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        project.AddMember(TestUser.Id, ProjectRole.Admin);
        project.AddMember(memberUser.Id, ProjectRole.Member);
        AppDbContext.Projects.Add(project);
        AppDbContext.SaveChanges();

        // Act
        await _projectService.RemoveMemberFromProjectAsync(project.Id, memberUser.Id, TestUser.Id);

        // Assert
        var updatedProject = await AppDbContext.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == project.Id);
        var member = updatedProject?.Members.FirstOrDefault(m => m.UserId == memberUser.Id);
        Assert.Null(member);
    }

    [Fact]
    public async Task GetUserProjects_ShouldReturnAllUserProjects()
    {
        // Arrange
        var project1 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Project 1",
            WorkspaceId = _testWorkspace.Id,
            CreatedByUserId = TestUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        project1.AddMember(TestUser.Id, ProjectRole.Admin);

        var project2 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Project 2",
            WorkspaceId = _testWorkspace.Id,
            CreatedByUserId = TestUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        project2.AddMember(TestUser.Id, ProjectRole.Member);

        AppDbContext.Projects.AddRange(project1, project2);
        AppDbContext.SaveChanges();

        // Act
        var result = await _projectService.GetUserProjectsAsync(_testWorkspace.Id, TestUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Project 1");
        Assert.Contains(result, p => p.Name == "Project 2");
    }

    [Fact]
    public async Task GetProjectDetails_ShouldReturnProjectWithMembers()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = _testWorkspace.Id,
            CreatedByUserId = TestUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        project.AddMember(TestUser.Id, ProjectRole.Admin);
        AppDbContext.Projects.Add(project);
        AppDbContext.SaveChanges();

        // Act
        var result = await _projectService.GetProjectDetailsAsync(project.Id, TestUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.Id);
        Assert.Equal("Test Project", result.Name);
        Assert.NotEmpty(result.Members);
    }

    [Fact]
    public async Task GetProjectDetails_WhenUserHasNoAccess_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = _testWorkspace.Id,
            CreatedByUserId = TestUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        AppDbContext.Projects.Add(project);
        AppDbContext.SaveChanges();

        _projectAuthorizerMock.Setup(a => a.EnsureHasAccessAsync(It.IsAny<Project>()))
            .ThrowsAsync(new UnauthorizedAccessException("You don't have access to this project"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _projectService.GetProjectDetailsAsync(project.Id, TestUser.Id));
    }
}
