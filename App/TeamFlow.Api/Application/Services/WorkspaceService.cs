using App.Domain.Model;
using App.Infrastructure.Repositories;

namespace App.Application.Services;

public class WorkspaceService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly UserValidator _userValidator;

    public WorkspaceService(
        IWorkspaceRepository workspaceRepository,
        UserValidator userValidator)
    {
        _workspaceRepository = workspaceRepository;
        _userValidator = userValidator;
    }
    
    public async Task CreateWorkspaceAsync (CreateWorkspaceDto dto, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var workspace = new Workspace
        {
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = userId,
            IsArchived = false,
            CreatedAt = DateTime.Now
        };
        await _workspaceRepository.AddAsync(workspace);
        await _workspaceRepository.SaveChangesAsync();
    }
};