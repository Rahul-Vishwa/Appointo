using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskzen.DTOs;
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
}