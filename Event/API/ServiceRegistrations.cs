using Event.DomainServices;
using Event.Infrastructure;
using Event.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using api = Event.API;
using ds = Event.DomainServices;

namespace Event.API
{
    public static class ServiceRegistrations
    {
        public static IHostApplicationBuilder AddEventModule(this IHostApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<EventDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IStaffAssignmentRepository, StaffAssignmentRepository>();
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<ds.IEventService, ds.EventService>();
            builder.Services.AddScoped<IShiftRepository, ShiftRepository>();
            builder.Services.AddScoped<IStaffRepository, StaffRepository>();
            builder.Services.AddScoped<IShiftService, ShiftService>();
            builder.Services.AddScoped<ds.IShiftService, ds.ShiftService>();
            builder.Services.AddScoped<api.IShiftService, api.ShiftService>();
            builder.Services.AddScoped<api.IStaffService, api.StaffService>();

            return builder;
        }
    }
}
