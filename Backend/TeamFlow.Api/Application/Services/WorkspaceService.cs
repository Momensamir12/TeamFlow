using App.Application.Common;
using App.Application.Dto;
using App.Domain.Model;
using App.Infrastructure.Authorization;
using App.Infrastructure.Repositories;
using AutoMapper;

namespace App.Application.Services;

public class WorkspaceService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly UserValidator _userValidator;
    private readonly IWorkspaceAuthorizer _workspaceAuthorizer;
    private readonly ILogger<WorkspaceService> _logger;
    private readonly IMapper _mapper;

    public WorkspaceService(
        IWorkspaceRepository workspaceRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        UserValidator userValidator,
        IWorkspaceAuthorizer workspaceAuthorizer,
        ILogger<WorkspaceService> logger,
        IMapper mapper)
    {
        _workspaceRepository = workspaceRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _userValidator = userValidator;
        _workspaceAuthorizer = workspaceAuthorizer;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task CreateWorkspaceAsync(CreateWorkspaceDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        await _userValidator.ActiveUserAsync(userId);

        var workspace = new Workspace
        {
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = userId,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow,
            Code = TokenGenerator.GenerateSecureToken()
        };

        workspace.AddMember(userId, WorkspaceRole.Admin);
        await _workspaceRepository.AddAsync(workspace, cancellationToken);
        await _workspaceRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Workspace created: {WorkspaceId} by user: {UserId}", workspace.Id, userId);
    }

    public async Task UpdateWorkspaceAsync(UpdateWorkspaceDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        await _userValidator.ActiveUserAsync(userId);

        var workspace = await _workspaceRepository.GetByIdAsync(dto.WorkspaceId, cancellationToken);

        // Only workspace admins can update workspace
        await _workspaceAuthorizer.EnsureIsAdminAsync(workspace);

        workspace.Name = dto.Name;
        workspace.Description = dto.Description;

        await _workspaceRepository.UpdateAsync(workspace, cancellationToken);
        await _workspaceRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Workspace updated: {WorkspaceId} by user: {UserId}", workspace.Id, userId);
    }

    public async Task<List<WorkspaceListDto>> GetUserWorkspacesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _userValidator.ActiveUserAsync(userId);

        var workspaces = await _workspaceRepository.GetUserWorkspacesAsync(userId, cancellationToken);
        
        var workspaceListDtos = new List<WorkspaceListDto>();
        
        foreach (var workspace in workspaces)
        {
            var memberCount = await _workspaceRepository.GetWorkspaceMembersAsync(workspace.Id, cancellationToken);
            var projectCount = await _projectRepository.GetWorkspaceProjectCountAsync(workspace.Id, cancellationToken);
            
            var dto = _mapper.Map<WorkspaceListDto>(workspace);
            dto.MemberCount = memberCount.Count;
            dto.ProjectCount = projectCount;
            
            workspaceListDtos.Add(dto);
        }

        return workspaceListDtos;
    }

    public async Task<WorkspaceDetailsDto> GetWorkspaceDetailsAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        await _userValidator.ActiveUserAsync(userId);

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);

        // Ensure user has access to this workspace
        await _workspaceAuthorizer.EnsureHasAccessAsync(workspace);

        var members = await _workspaceRepository.GetWorkspaceMembersAsync(workspaceId, cancellationToken);
        var projects = await _projectRepository.GetWorkspaceProjectsAsync(workspaceId, cancellationToken);
        var owner = await _userRepository.GetByIdAsync(workspace.OwnerId, cancellationToken);

        var memberDtos = new List<WorkspaceMemberDto>();
        foreach (var member in members)
        {
            var user = await _userRepository.GetByIdAsync(member.UserId, cancellationToken);
            var memberDto = _mapper.Map<WorkspaceMemberDto>(member);
            memberDto.UserName = user?.FirstName ?? string.Empty;
            memberDto.UserEmail = user?.Email ?? string.Empty;
            memberDtos.Add(memberDto);
        }

        var projectDtos = _mapper.Map<List<ProjectSummaryDto>>(projects);

        var detailsDto = _mapper.Map<WorkspaceDetailsDto>(workspace);
        detailsDto.OwnerName = owner?.FirstName ?? string.Empty;
        detailsDto.Members = memberDtos;
        detailsDto.Projects = projectDtos;

        return detailsDto;
    }

    public async Task ArchiveWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

        // Only workspace admins can archive workspace
        await _workspaceAuthorizer.EnsureIsAdminAsync(workspace);

        workspace.IsArchived = true;

        await _workspaceRepository.UpdateAsync(workspace);
        await _workspaceRepository.SaveChangesAsync();

        _logger.LogInformation("Workspace archived: {WorkspaceId} by user: {UserId}", workspaceId, userId);
    }

    public async Task UnarchiveWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

        await _workspaceAuthorizer.EnsureIsAdminAsync(workspace);

        workspace.IsArchived = false;

        await _workspaceRepository.UpdateAsync(workspace);
        await _workspaceRepository.SaveChangesAsync();

        _logger.LogInformation("Workspace unarchived: {WorkspaceId} by user: {UserId}", workspaceId, userId);
    }

    public async Task JoinWorkspaceByCode(Guid userId, string code)
    {
        await _userValidator.ActiveUserAsync(userId);
        
        var workspace = await _workspaceRepository.GetByCodeAsync(code);
        
        workspace.AddMember(userId, WorkspaceRole.Viewer);
        await _workspaceRepository.UpdateAsync(workspace);
        await _workspaceRepository.SaveChangesAsync();

        _logger.LogInformation("User {UserId} joined workspace {WorkspaceId} via code", userId, workspace.Id);
    }

    public async Task<string> GetWorkspaceCodeAsync(Guid workspaceId, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

        await _workspaceAuthorizer.EnsureIsMemberAsync(workspace);

        return workspace.Code;
    }

    public async Task RegenerateWorkspaceCodeAsync(Guid workspaceId, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

        await _workspaceAuthorizer.EnsureIsAdminAsync(workspace);

        workspace.Code = TokenGenerator.GenerateSecureToken();

        await _workspaceRepository.UpdateAsync(workspace);
        await _workspaceRepository.SaveChangesAsync();

        _logger.LogInformation("Workspace code regenerated: {WorkspaceId} by user: {UserId}", workspaceId, userId);
    }

    public async Task RemoveMemberAsync(Guid workspaceId, Guid memberUserId, Guid currentUserId)
    {
        await _userValidator.ActiveUserAsync(currentUserId);

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

        await _workspaceAuthorizer.EnsureIsAdminAsync(workspace);

        if (memberUserId == workspace.OwnerId)
        {
            throw new InvalidOperationException("Cannot remove the workspace owner");
        }

        workspace.RemoveMember(memberUserId);
        await _workspaceRepository.UpdateAsync(workspace);
        await _workspaceRepository.SaveChangesAsync();

        _logger.LogInformation("Member {MemberUserId} removed from workspace {WorkspaceId} by user: {CurrentUserId}", 
            memberUserId, workspaceId, currentUserId);
    }

    public async Task UpdateMemberRoleAsync(Guid workspaceId, Guid memberUserId, WorkspaceRole newRole, Guid currentUserId)
    {
        await _userValidator.ActiveUserAsync(currentUserId);

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

        await _workspaceAuthorizer.EnsureIsAdminAsync(workspace);

        if (memberUserId == workspace.OwnerId)
        {
            throw new InvalidOperationException("Cannot change the workspace owner's role");
        }

        workspace.UpdateMemberRole(memberUserId, newRole);
        await _workspaceRepository.UpdateAsync(workspace);
        await _workspaceRepository.SaveChangesAsync();

        _logger.LogInformation("Member {MemberUserId} role updated to {NewRole} in workspace {WorkspaceId} by user: {CurrentUserId}", 
            memberUserId, newRole, workspaceId, currentUserId);
    }

    public async Task<int> GetUserRoleInWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var member =await _workspaceRepository.GetMemberAsync(workspaceId, userId);
        
        if (member == null)
        {
            throw new InvalidOperationException("User is not a member of this workspace");
        }

        return (int)member.Role;
    }
}