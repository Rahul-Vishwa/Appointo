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
[Authorize(Roles = "Admin")]
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
            var scheduleResult = await schedule.SaveSchedule(scheduleData);

            if (scheduleResult.Schedule != null)
            {
                return CreatedAtAction(nameof(Save), new { id = scheduleResult.Schedule.Id }, scheduleResult.Schedule);
            }
            return BadRequest(scheduleResult.Message);
        }    
        
        return BadRequest("User not found");
    }

    [HttpGet]
    public async Task<IActionResult> GetSchedules(int page, int pageSize)
    {
        var schedules = await schedule.GetSchedules(page, pageSize);
        return Ok(schedules);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var userId = await User.GetUserId(dbContext);

        if (userId != null)
        {
            var deletedSchedule = await schedule.DeleteSchedule(id, (int)userId);
            if (deletedSchedule != null)
            {
                return NoContent();
            }
            return BadRequest("Schedule not found.");
        }
        return BadRequest("User not found.");
    }
}