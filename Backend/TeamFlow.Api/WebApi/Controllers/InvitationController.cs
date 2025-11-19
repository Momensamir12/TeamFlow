using App.Application.Common;
using App.Application.Dto;
using App.Application.Interfaces;
using App.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Authorize]
[Route("api/invitations")]
[ApiController]
public class InvitationController : ControllerBase
{
    private readonly InvitationService _invitationService;
    private readonly ICurrentUserService _currentUserService;

    public InvitationController(InvitationService invitationService, ICurrentUserService currentUserService)
    {
        _invitationService = invitationService;
        _currentUserService = currentUserService;
    }

    [HttpPost("send")]
    public async Task<ActionResult<ApiResponse<string>>> SendInvitation(InvitationRequestDto dto)
    {
        var userId = _currentUserService.UserId;
        await _invitationService.SendInvitationToWorkspaceAsync(dto, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Invitation sent successfully"));
    }

    [HttpPost("accept")]
    public async Task<ActionResult<ApiResponse<string>>> AcceptInvitation([FromBody] AcceptInvitationDto dto)
    {
        var userId = _currentUserService.UserId;
        await _invitationService.AcceptInvitationAsync(dto.Token, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Invitation accepted successfully"));
    }

    [HttpGet("validate")]
    public async Task<ActionResult<ApiResponse<InvitationDetailsDto>>> ValidateInvitation([FromQuery] string token)
    {
        var details = await _invitationService.GetInvitationDetailsAsync(token);
        return Ok(ApiResponse<InvitationDetailsDto>.SuccessResponse(details, "Invitation details retrieved"));
    }
}