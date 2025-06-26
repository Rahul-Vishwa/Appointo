using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskzen.DTOs;
using Taskzen.Helpers;
using Taskzen.Interfaces;

namespace Taskzen.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class UserController(IUser user) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddUser(AddUserDto userData)
    {
        var newUser = await user.AddUser(userData);
        return CreatedAtAction(nameof(AddUser), new { id = newUser.Id }, newUser);
    }

    [HttpPost("AssignRole")]
    public async Task<IActionResult> AssignRole(AssignRoleDto role)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                     User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        
        var roleAssigned = await user.AssignRole(role.Role, userId);
        
        return roleAssigned ? Ok() : StatusCode(500);
    }
    
    [HttpGet("GetRole")]
    public IActionResult GetRole()
    {
        var role = User.FindFirst("https://taskzen.com/role")?.Value;
        return Ok(new { role });
    }
}