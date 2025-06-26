using Taskzen.DTOs;

namespace Taskzen.Interfaces;

public interface IDashboard
{
    Task<GetAnalyticsTodayDto> GetAnalyticsToday();
    Task<List<AppointmentCountByDateDto>> GetAppointmentCountPast7Days();
    Task<List<AppointmentCountByDateDto>> GetAppointmentCountThisMonth();
    Task<List<AppointmentCountByDateDto>> GetCancelledApointmentsPast7Days();
    Task<List<AppointmentCountByDateDto>> GetCancelledApointmentsThisMonth();
    Task<GetPercetageDto> GetPercentages();
}