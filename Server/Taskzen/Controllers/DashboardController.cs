using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskzen.DTOs;
using Taskzen.Interfaces;

namespace Taskzen.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("[controller]")]
public class DashboardController(IDashboard dashboard) : ControllerBase
{
    [HttpGet("GetAnalyticsToday")]
    public async Task<IActionResult> GetAnalyticsToday()
    {
        var analytics = await dashboard.GetAnalyticsToday();
        return Ok(analytics);
    }
    
    [HttpGet("GetAppointmentCountPast7Days")]
    public async Task<IActionResult> GetAppointmentCountPast7Days()
    {
        var analytics = await dashboard.GetAppointmentCountPast7Days();
        return Ok(analytics);
    }
    
    [HttpGet("GetAppointmentCountThisMonth")]
    public async Task<IActionResult> GetAppointmentCountThisMonth()
    {
        var analytics = await dashboard.GetAppointmentCountThisMonth();
        return Ok(analytics);
    }
    
    [HttpGet("GetCancelledAppointmentCountPast7Days")]
    public async Task<IActionResult> GetCancelledAppointmentCountPast7Days()
    {
        var analytics = await dashboard.GetCancelledApointmentsPast7Days();
        return Ok(analytics);
    }
    
    [HttpGet("GetCancelledAppointmentCountThisMonth")]
    public async Task<IActionResult> GetCancelledAppointmentCountThisMonth()
    {
        var analytics = await dashboard.GetCancelledApointmentsThisMonth();
        return Ok(analytics);
    }
    
    [HttpGet("GetPercentageAnalytics")]
    public async Task<IActionResult> GetPercentages()
    {
        var analytics = await dashboard.GetPercentages();
        return Ok(analytics);
    }
}