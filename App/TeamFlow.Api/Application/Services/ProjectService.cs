using App.Application.Dto;
using App.Application.Interfaces;
using App.Domain.Model;
using App.Infrastructure.Authorization;
using App.Infrastructure.Repositories;

namespace App.Application.Services;

public class ProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWorkspaceAuthorizer _workspaceAuthorizer;
    private readonly IProjectAuthorizer _projectAuthorizer;
    private readonly ITaskRepository _taskRepository;
    private readonly UserValidator _userValidator;

    public ProjectService(
        IProjectRepository projectRepository,
        IWorkspaceRepository workspaceRepository,
        IUserRepository userRepository,
        ITaskRepository taskRepository,
        IWorkspaceAuthorizer workspaceAuthorizer,
        IProjectAuthorizer projectAuthorizer,
        UserValidator userValidator)
    {
        _projectRepository = projectRepository;
        _workspaceRepository = workspaceRepository;
        _userRepository = userRepository;
        _taskRepository = taskRepository;
        _workspaceAuthorizer = workspaceAuthorizer;
        _projectAuthorizer = projectAuthorizer;
        _userValidator = userValidator;
    }

    public async Task CreateProjectAsync(CreateProjectDto dto, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var workspace = await _workspaceRepository.GetByIdAsync(dto.WorkspaceId);
        
        await _workspaceAuthorizer.EnsureIsMemberAsync(workspace);

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            WorkspaceId = dto.WorkspaceId,
            CreatedByUserId = userId,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        project.AddMember(userId, ProjectRole.Admin);

        await _projectRepository.AddAsync(project);
        await _projectRepository.SaveChangesAsync();
    }

    public async Task UpdateProjectAsync(UpdateProjectDto dto, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var project = await _projectRepository.GetByIdAsync(dto.ProjectId);

        // Only project admins can update
        await _projectAuthorizer.EnsureIsAdminAsync(project);

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project);
        await _projectRepository.SaveChangesAsync();
    }

    public async Task AddMemberToProjectAsync(AddProjectMemberDto dto, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var project = await _projectRepository.GetByIdAsync(dto.ProjectId);

        // Only project admins can add members
        await _projectAuthorizer.EnsureIsAdminAsync(project);

        // Verify the user is a workspace member
        var workspace = await _workspaceRepository.GetByIdAsync(project.WorkspaceId);
        if (!await _workspaceRepository.IsMemberAsync(workspace.Id, dto.UserId))
            throw new InvalidOperationException("User must be a workspace member first");

        project.AddMember(dto.UserId, dto.Role);
        

        await _projectRepository.SaveChangesAsync();
    }

    public async Task RemoveMemberFromProjectAsync(Guid projectId, Guid memberUserId, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var project = await _projectRepository.GetByIdAsync(projectId);

        // Only project admins can remove members
        await _projectAuthorizer.EnsureIsAdminAsync(project);

        project.RemoveMember(memberUserId);

        await _projectRepository.UpdateAsync(project);
        await _projectRepository.SaveChangesAsync();
    }

    public async Task<List<ProjectDto>> GetUserProjectsAsync(Guid workspaceId, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
        await _workspaceAuthorizer.EnsureHasAccessAsync(workspace);

        var projects = await _projectRepository.GetByWorkspaceIdAsync(workspaceId);
        
        // Return only projects user has access to
        var userProjects = new List<ProjectDto>();
        foreach (var project in projects)
        {
            if (await _projectAuthorizer.HasAccessAsync(project))
            {
                userProjects.Add(new ProjectDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    WorkspaceId = project.WorkspaceId,
                    IsArchived = project.IsArchived,
                    CreatedAt = project.CreatedAt
                });
            }
        }

        return userProjects;
    }

    public async Task<ProjectDetailsDto> GetProjectDetailsAsync(Guid projectId, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);
        var project = await _projectRepository.GetByIdAsync(projectId);
        await _projectAuthorizer.EnsureHasAccessAsync(project);
        
        var workspace = await _workspaceRepository.GetByIdAsync(project.WorkspaceId);
        var tasks = await _taskRepository.GetProjectTasksAsync(projectId);
        var members = await _projectRepository.GetProjectMembersAsync(projectId);
        
        var memberDtos = new List<ProjectMemberDto>();
        foreach (var member in members)
        {
            var user = await _userRepository.GetByIdAsync(member.UserId);
            memberDtos.Add(new ProjectMemberDto
            {
                Id = member.Id,
                UserId = member.UserId,
                UserName = user?.FirstName ?? string.Empty,
                UserEmail = user?.Email ?? string.Empty,
                Role = (int)member.Role,  // Cast enum to int
                AddedAt = member.AddedAt
            });
        }
        
        return new ProjectDetailsDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            WorkspaceId = project.WorkspaceId,
            WorkspaceName = workspace?.Name ?? string.Empty,
            CreatedByUserId = project.CreatedByUserId,
            CreatedByUserName = string.Empty, // TODO: Fetch creator name if needed
            IsArchived = project.IsArchived,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            Members = memberDtos
        };
    }

    public async Task<List<ProjectListDto>> GetWorkspaceProjectsAsync(Guid workspaceId, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
        await _workspaceAuthorizer.EnsureHasAccessAsync(workspace);
        
        var projects = await _projectRepository.GetWorkspaceProjectsAsync(workspaceId);
        return projects.Select(p => new ProjectListDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
WorkspaceId = p.WorkspaceId
        }).ToList();
    }

    public async Task<int> GetUserRoleInProjectAsync(Guid projectId, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var member = await _projectRepository.GetMemberAsync(projectId, userId);
        if (member == null)
        {
            throw new InvalidOperationException("User is not a member of this project");
        }

        return (int)member.Role;
    }
    public async Task<List<UserTaskDto>> GetProjectTasksAsync (Guid projectId)
    {
        var tasks = await _taskRepository.GetProjectTasksAsync(projectId);
        var tasksDto = tasks.Select(t => new UserTaskDto 
        { 
            Id = t.Id, 
            Title = t.Title, 
            Description = t.Description,
            Status = (int)t.Status,
            Priority = (int)t.Priority,
            Deadline = t.Deadline,
            AssigneeId = t.AssigneeId,
            OwnerId = t.OwnerId,
            ProjectId = t.ProjectId
        }).ToList();

        return tasksDto;
    }
}