using App.Application.Common;
using App.Application.Dto;
using App.Application.Interfaces;
using App.Domain.Model;
using App.Infrastructure.Repositories;
using App.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace App.Application.Services;

public class InvitationService
{
    private readonly IEmailService _emailService;
    private readonly IWorkspaceInvitationRepository _invitationRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUserRepository _userRepository;
    private readonly TemplateRenderer _templateRenderer;
    private readonly ILogger<InvitationService> _logger;
    private readonly string _frontendUrl;

    public InvitationService(
        IEmailService emailService,
        IWorkspaceInvitationRepository invitationRepository,
        IWorkspaceRepository workspaceRepository,
        IUserRepository userRepository,
        TemplateRenderer templateRenderer,
        IOptions<AppSettings> appSettings,
        ILogger<InvitationService> logger)
    {
        _emailService = emailService;
        _invitationRepository = invitationRepository;
        _workspaceRepository = workspaceRepository;
        _userRepository = userRepository;
        _templateRenderer = templateRenderer;
        _logger = logger;
        _frontendUrl = appSettings.Value.FrontendUrl.TrimEnd('/');
    }

    public async Task SendInvitationToWorkspaceAsync(InvitationRequestDto dto, Guid currentUserId)
    {
        WorkspaceInvitation? invitation = null;
        
        try
        {
            _logger.LogInformation("Starting invitation process for email: {Email} to workspace: {WorkspaceId}", dto.Email, dto.WorkspaceId);

            var workspace = await _workspaceRepository.GetByIdAsync(dto.WorkspaceId);
            _logger.LogInformation("Workspace found: {WorkspaceName}", workspace.Name);

            if (await _invitationRepository.HasPendingInvitationAsync(dto.WorkspaceId, dto.Email))
            {
                _logger.LogWarning("Pending invitation already exists for email: {Email}", dto.Email);
                throw new InvalidOperationException("Invitation already sent to this email");
            }

            var token = TokenGenerator.GenerateSecureToken();
            _logger.LogInformation("Generated secure token for invitation");

            invitation = new WorkspaceInvitation
            {
                Id = Guid.NewGuid(),
                WorkspaceId = dto.WorkspaceId,
                Email = dto.Email.Trim().ToLower(),
                Role = dto.Role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsAccepted = false,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Saving invitation to database");
            await _invitationRepository.AddAsync(invitation);
            await _invitationRepository.SaveChangesAsync();
            _logger.LogInformation("Invitation saved successfully with ID: {InvitationId}", invitation.Id);

            var invitationLink = $"{_frontendUrl}/accept-invitation?token={token}";
            _logger.LogInformation("Generated invitation link: {InvitationLink}", invitationLink);

            _logger.LogInformation("Fetching inviter user with ID: {UserId}", currentUserId);
            
            _logger.LogInformation("Rendering email template");
            var emailBody = _templateRenderer.Render("EmailTemplate", new Dictionary<string, string>
            {
                { "WorkspaceName", workspace.Name },
                { "Role", dto.Role.ToString() },
                { "InvitationLink", invitationLink }
            });
            _logger.LogInformation("Email template rendered successfully");

            var emailSubject = $"You've been invited to join {workspace.Name}";

            _logger.LogInformation("Sending email to: {Email}", dto.Email);
            var emailSent = await _emailService.SendEmailAsync(dto.Email, emailSubject, emailBody, isHtml: true);
            
            if (!emailSent)
            {
                _logger.LogError("Failed to send email to: {Email}", dto.Email);
                throw new InvalidOperationException("Failed to send invitation email");
            }
            
            _logger.LogInformation("Email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending invitation. Email: {Email}, WorkspaceId: {WorkspaceId}",
                dto.Email, dto.WorkspaceId);
            
            if (invitation != null)
            {
                try
                {
                    _logger.LogWarning("Deleting invitation {InvitationId} due to error", invitation.Id);
                    await _invitationRepository.DeleteAsync(invitation);
                    await _invitationRepository.SaveChangesAsync();
                    _logger.LogInformation("Invitation deleted successfully");
                }
                catch (Exception deleteEx)
                {
                    _logger.LogError(deleteEx, "Failed to delete invitation {InvitationId} after error", invitation.Id);
                }
            }
                
            throw;
        }
    }

    public async Task AcceptInvitationAsync(string token, Guid userId)
    {
        try
        {
            _logger.LogInformation("Accepting invitation with token: {Token} for user: {UserId}", token, userId);

            var invitation = await _invitationRepository.GetByTokenAsync(token);
            if (invitation == null)
            {
                _logger.LogWarning("Invitation not found for token: {Token}", token);
                throw new KeyNotFoundException("Invitation not found");
            }

            _logger.LogInformation("Invitation found: {InvitationId}", invitation.Id);

            if (invitation.IsAccepted)
            {
                _logger.LogWarning("Invitation {InvitationId} has already been accepted", invitation.Id);
                throw new InvalidOperationException("Invitation has already been accepted");
            }

            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Invitation {InvitationId} has expired at {ExpiresAt}", invitation.Id, invitation.ExpiresAt);
                throw new InvalidOperationException("Invitation has expired");
            }

            _logger.LogInformation("Fetching workspace: {WorkspaceId}", invitation.WorkspaceId);
            var workspace = await _workspaceRepository.GetByIdAsync(invitation.WorkspaceId);
            
            _logger.LogInformation("Adding user {UserId} as member to workspace {WorkspaceId} with role {Role}", 
                userId, workspace.Id, invitation.Role);
            workspace.AddMember(userId, invitation.Role);

            invitation.IsAccepted = true;
            invitation.AcceptedAt = DateTime.UtcNow;

            _logger.LogInformation("Updating invitation and workspace");
            await _invitationRepository.UpdateAsync(invitation);
            await _workspaceRepository.UpdateAsync(workspace);
            await _workspaceRepository.SaveChangesAsync();
            await _invitationRepository.SaveChangesAsync();
            
            _logger.LogInformation("Invitation accepted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while accepting invitation. Token: {Token}, UserId: {UserId}", 
                token, userId);
            throw;
        }
    }

    public async Task<InvitationDetailsDto> GetInvitationDetailsAsync(string token)
    {
        try
        {
            _logger.LogInformation("Fetching invitation details for token: {Token}", token);

            var invitation = await _invitationRepository.GetByTokenAsync(token);
            if (invitation == null)
            {
                _logger.LogWarning("Invitation not found for token: {Token}", token);
                throw new KeyNotFoundException("Invitation not found");
            }

            var workspace = await _workspaceRepository.GetByIdAsync(invitation.WorkspaceId);

            return new InvitationDetailsDto
            {
                WorkspaceName = workspace.Name,
                Role = (int)invitation.Role,
                RoleName = invitation.Role.ToString(),
                ExpiresAt = invitation.ExpiresAt,
                IsExpired = invitation.ExpiresAt < DateTime.UtcNow,
                IsAccepted = invitation.IsAccepted
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching invitation details. Token: {Token}", token);
            throw;
        }
    }
}