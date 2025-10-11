using Event.Domain;
using Event.DomainServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.API
{
    public static class EndpointRegistrations
    {
        public static WebApplication MapEventEndpoints(this WebApplication app)
        {
            app.MapPost("/events", async (EventDTO newEvent, IEventService eventService) =>
            {
                var created = await eventService.CreateEventAsync(newEvent);
                return Results.Created($"/events/{created.Id}", created);
            });

            app.MapGet("/events/{id:int}", async (int id, IEventService eventService) =>
            {
                try
                {
                    var fetchedEvent = await eventService.GetEventByIdAsync(id);
                    return Results.Ok(fetchedEvent);
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }
            });

            app.MapGet("/events", async (IEventService eventService) =>
            {
                var events = await eventService.GetAllEventsAsync();
                return Results.Ok(events);
            });

            app.MapPut("/events/{id:int}", async (int id, EventDTO updatedEvent, IEventService eventService) =>
            {
                if (id != updatedEvent.Id) return Results.BadRequest("ID mismatch");
                try
                {
                    var result = await eventService.UpdateEventAsync(updatedEvent);
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

            app.MapDelete("/events/{id:int}", async (int id, IEventService eventService) =>
            {
                try
                {
                    var fetchedEvent = await eventService.GetEventByIdAsync(id);
                    await eventService.CancelEventAsync(fetchedEvent);
                    return Results.NoContent();
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }
            });

            app.MapPost("/shifts", async (ShiftDTO newShift, IShiftService shiftService) =>
            {
                var created = await shiftService.CreateShiftAsync(newShift);
                return Results.Created($"/shifts/{created.Id}", created);
            });

            app.MapGet("/shifts/{id:int}", async (int id, IShiftService shiftService) =>
            {
                try
                {
                    var shift = await shiftService.GetShiftByIdAsync(id);
                    return Results.Ok(shift);
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }
            });

            app.MapPut("/shifts/{id:int}", async (int id, ShiftDTO updatedShift, IShiftService shiftService) =>
            {
                if (id != updatedShift.Id) return Results.BadRequest("ID mismatch");
                try
                {
                    var result = await shiftService.UpdateShiftAsync(updatedShift);
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

            app.MapDelete("/shifts/{id:int}", async (int id, IShiftService shiftService) =>
            {
                try
                {
                    var shift = await shiftService.GetShiftByIdAsync(id);
                    await shiftService.CancelShiftAsync(shift);
                    return Results.NoContent();
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }
            });


            app.MapGet("/events/{eventId:int}/shifts", async (int eventId, IShiftService shiftService) =>
            {
                var shifts = await shiftService.GetShiftsByEventIdAsync(eventId);
                return Results.Ok(shifts);
            });

            app.MapPost("/events/{eventId:int}/shifts", async (int eventId, ShiftDTO newShift, IShiftService shiftService) =>
            {
                newShift.EventId = eventId;
                var created = await shiftService.CreateShiftAsync(newShift);
                return Results.Created($"/events/{eventId}/shifts/{created.Id}", created);
            });

            app.MapGet("/events/{eventId:int}/shifts/{shiftId:int}", async (int eventId, int shiftId, IShiftService shiftService) =>
            {
                try
                {
                    var shift = await shiftService.GetShiftByIdAsync(shiftId);
                    if (shift.EventId != eventId) return Results.NotFound();
                    return Results.Ok(shift);
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }
            });

            app.MapPut("/events/{eventId:int}/shifts/{shiftId:int}", async (int eventId, int shiftId, ShiftDTO updatedShift, IShiftService shiftService) =>
            {
                if (shiftId != updatedShift.Id) return Results.BadRequest("ID mismatch");
                try
                {
                    var existing = await shiftService.GetShiftByIdAsync(shiftId);
                    if (existing.EventId != eventId) return Results.NotFound();
                    var result = await shiftService.UpdateShiftAsync(updatedShift);
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

            app.MapDelete("/events/{eventId:int}/shifts/{shiftId:int}", async (int eventId, int shiftId, IShiftService shiftService) =>
            {
                try
                {
                    var shift = await shiftService.GetShiftByIdAsync(shiftId);
                    if (shift.EventId != eventId) return Results.NotFound();
                    await shiftService.CancelShiftAsync(shift);
                    return Results.NoContent();
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }
            });

            app.MapPost("/shifts/{shiftId:int}/staff/{staffId:int}", async(int shiftId, int staffId, [FromServices] IShiftService shiftService, [FromServices]IStaffService staffService) =>
            {
                try
                {
                    var shift = await shiftService.GetShiftByIdAsync(shiftId);
                    var staff = await staffService.GetStaffByIdAsync(staffId);
                    await shiftService.AddStaffToShiftAsync(shift, staff);
                    return Results.Ok();
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

            app.MapDelete("/shifts/{shiftId:int}/staff/{staffId:int}", async (int shiftId, int staffId, IShiftService shiftService) =>
            {
                try
                {
                    var shift = await shiftService.GetShiftByIdAsync(shiftId);
                    return Results.Ok();
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


            return app;
        }
    }
}
