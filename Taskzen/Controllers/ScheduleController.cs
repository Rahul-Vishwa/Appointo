using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskzen.DTOs;
using Taskzen.Interfaces;
using Taskzen.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Taskzen.Entities;
using Taskzen.Helpers;
using Schedule = Taskzen.DTOs;

namespace Taskzen.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class ScheduleController(ISchedule schedule, AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] AddScheduleDto scheduleData)
    {
        var userId = await User.GetUserId(dbContext);
        if (userId != null)
        {
            scheduleData.CreatedBy = (int)userId;
            var newSchedule = await schedule.SaveSchedule(scheduleData);
            return CreatedAtAction(nameof(Save), new { id = newSchedule.Id }, newSchedule);
        }    
        
        return BadRequest("User not found");
    }

    [HttpGet]
    public async Task<IActionResult> GetSchedules()
    {
        var schedules = await schedule.GetSchedules();
        return Ok(schedules);
    }
}