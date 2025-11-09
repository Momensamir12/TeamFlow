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
    private readonly IWorkspaceAuthorizer _workspaceAuthorizer;
    private readonly IProjectAuthorizer _projectAuthorizer;
    private readonly ITaskRepository _taskRepository;
    private readonly UserValidator _userValidator;

    public ProjectService(
        IProjectRepository projectRepository,
        IWorkspaceRepository workspaceRepository,
        ITaskRepository taskRepository,
        IWorkspaceAuthorizer workspaceAuthorizer,
        IProjectAuthorizer projectAuthorizer,
        UserValidator userValidator)
    {
        _projectRepository = projectRepository;
        _workspaceRepository = workspaceRepository;
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
        
        var tasks = await _taskRepository.GetProjectTasksAsync(projectId);
        var members = await _projectRepository.GetProjectMembersAsync(projectId);
        
        return new ProjectDetailsDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            WorkspaceId = project.WorkspaceId,
            Tasks = tasks.Select(t => new UserTaskDto { Id = t.Id, Title = t.Title, Status = ((int)t.Status)}).ToList(),
            Members = members.Select(m => new ProjectMemberDto { UserId = m.UserId, Role = m.Role }).ToList()
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
}