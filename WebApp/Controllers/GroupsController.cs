using Application.Chat;
using Application.Chat.Dto;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController(IGroupService groupService) : ControllerBase
{
    private string CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    [HttpPost]
    public async Task<ActionResult<Response<ChatGroupDto>>> Create([FromBody] CreateGroupRequest request)
    {
        var result = await groupService.EnsureGroupExistsAsync(CurrentUserId, request.Name);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("join")]
    public async Task<ActionResult<Response<bool>>> Join([FromBody] GroupNameRequest request)
    {
        var result = await groupService.JoinGroupAsync(CurrentUserId, request.Name);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("leave")]
    public async Task<ActionResult<Response<bool>>> Leave([FromBody] GroupNameRequest request)
    {
        var result = await groupService.LeaveGroupAsync(CurrentUserId, request.Name);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("my")]
    public async Task<ActionResult<Response<List<ChatGroupDto>>>> MyGroups()
    {
        var result = await groupService.GetUserGroupsAsync(CurrentUserId);
        return StatusCode(result.StatusCode, result);
    }
}

public class CreateGroupRequest
{
    public string Name { get; set; } = default!;
}

public class GroupNameRequest
{
    public string Name { get; set; } = default!;
}


