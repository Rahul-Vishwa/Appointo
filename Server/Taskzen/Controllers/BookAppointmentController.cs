using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskzen.Data;
using Taskzen.DTOs;
using Taskzen.Helpers;
using Taskzen.Interfaces;

namespace Taskzen.Controllers;

[ApiController]
[Authorize(Roles = "Customer")]
[Route("[controller]")]
public class BookAppointmentController(IBookAppointment appointment, AppDbContext dbContext) : Controller
{
    [HttpGet("GetBookedSlots")]
    public async Task<IActionResult> GetBookedSlots([FromQuery] GetBookedSlotsDto slot)
    {
        var bookedSlots = await appointment.GetBookedSlots(slot); 
        return Ok(bookedSlots);
    }
    
    [HttpPost]
    public async Task<IActionResult> SaveAppointment([FromBody] SaveAppointmentDto saveAppointment)
    {
        var userId = await User.GetUserId(dbContext);
        
        if (userId != null)
        {
            saveAppointment.CreatedBy = (int)userId;
            
            var newAppointment = await appointment.SaveAppointment(saveAppointment);

            if (newAppointment.Success)
            {
                return CreatedAtAction(nameof(SaveAppointment), new { id = newAppointment.Appointment?.Id }, newAppointment);
            }

            return BadRequest(newAppointment.Message);
        }
        return BadRequest("User not found.");
    }
}