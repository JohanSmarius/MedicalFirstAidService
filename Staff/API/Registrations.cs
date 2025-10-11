using Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using domain = Staff.DomainServices;
using api = Staff.API;
using Staff.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Staff.API
{
    public static class Registrations
    {
        public static IHostApplicationBuilder AddStaffModule(this IHostApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<StaffDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped<api.IStaffService, api.StaffService>();
            builder.Services.AddScoped<domain.IStaffService, domain.StaffService>();
            builder.Services.AddScoped<domain.IStaffRepository, StaffRepository>();

            return builder;
        }

        

    }
}
