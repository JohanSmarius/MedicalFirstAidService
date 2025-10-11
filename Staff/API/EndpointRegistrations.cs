using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Staff.DomainServices;
using domain = Staff.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace Staff.API
{
    public static class EndpointRegistrations
    {
        public static WebApplication MapStaffEndpoints(this WebApplication app)
        {
            app.MapPost("/staff", async (StaffDTO newStaff, IStaffService staffService) =>
            {
                var created = await staffService.CreateStaffAsync(newStaff);
                return Results.Created($"/staff/{created.Id}", created);
            });

            app.MapGet("/staff", async (IStaffService staffService) =>
            {
                var staff = await staffService.GetAllStaffAsync();
                return Results.Ok(staff);
            });

            app.MapGet("/staff/{id:int}", async (int id, IStaffService staffService) =>
            {
                try
                {
                    var staff = await staffService.GetStaffByIdAsync(id);
                    return Results.Ok(staff);
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }
            });

            app.MapPut("/staff/{id:int}", async (int id, StaffDTO updatedStaff, IStaffService staffService) =>
            {
                if (id != updatedStaff.Id) return Results.BadRequest("ID mismatch");
                try
                {
                    var result = await staffService.UpdateStaffAsync(updatedStaff);
                    return Results.Ok(result);
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }
                catch (DomainException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });

            app.MapDelete("/staff/{id:int}", async (int id, IStaffService staffService) =>
            {
                try
                {
                    var staff = await staffService.GetStaffByIdAsync(id);
                    await staffService.ResignStaffAsync(staff);
                    return Results.NoContent();
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }
            });

            return app;
        }
    }
}
