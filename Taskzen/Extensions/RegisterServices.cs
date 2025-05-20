using Taskzen.Interfaces;
using Taskzen.Repositories;

namespace Taskzen.Extensions;

public static class RegisterServices
{
    public static void RegisterInterfaces(this IServiceCollection services)
    {
        services.AddScoped<IUser, UserRepository>();
        services.AddScoped<IAppointment, AppointmentRepository>();
        services.AddScoped<ISchedule, ScheduleRepository>();
    }
}


