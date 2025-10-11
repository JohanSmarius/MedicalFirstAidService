using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Domain;
using DomainService;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IStaffAssignmentRepository, StaffAssignmentRepository>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IShiftRepository, ShiftRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IStaffService, StaffService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/events", async (Event newEvent, IEventService eventService) =>
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

app.MapPut("/events/{id:int}", async (int id, Event updatedEvent, IEventService eventService) =>
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

app.MapPost("/shifts", async (Shift newShift, IShiftService shiftService) =>
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

app.MapPut("/shifts/{id:int}", async (int id, Shift updatedShift, IShiftService shiftService) =>
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

app.MapPost("/staff", async (Staff newStaff, IStaffService staffService) =>
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

app.MapPut("/staff/{id:int}", async (int id, Staff updatedStaff, IStaffService staffService) =>
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

app.MapGet("/events/{eventId:int}/shifts", async (int eventId, IShiftService shiftService) =>
{
    var shifts = await shiftService.GetShiftsByEventIdAsync(eventId);
    return Results.Ok(shifts);
});

app.MapPost("/events/{eventId:int}/shifts", async (int eventId, Shift newShift, IShiftService shiftService) =>
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

app.MapPut("/events/{eventId:int}/shifts/{shiftId:int}", async (int eventId, int shiftId, Shift updatedShift, IShiftService shiftService) =>
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

app.MapPost("/shifts/{shiftId:int}/staff/{staffId:int}", async (int shiftId, int staffId, IShiftService shiftService, IStaffService staffService) =>
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

app.MapDelete("/shifts/{shiftId:int}/staff/{staffId:int}", async (int shiftId, int staffId, IShiftService shiftService, IStaffService staffService) =>
{
    try
    {
        var shift = await shiftService.GetShiftByIdAsync(shiftId);
        var staff = await staffService.GetStaffByIdAsync(staffId);
        await shiftService.RemoveStaffFromShiftAsync(shift, staff);
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

app.Run();
