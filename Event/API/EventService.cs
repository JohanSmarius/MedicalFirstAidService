using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ds = Event.DomainServices;

namespace Event.API;

internal class EventService : IEventService
{
    private readonly ds.IEventService internalEventService;

    public EventService(ds.IEventService internalEventService)
    {
        this.internalEventService = internalEventService;
    }

    public async Task<EventDTO> CreateEventAsync(EventDTO newEvent)
    {
        var domainEvent = ToDomain(newEvent);
        var created = await internalEventService.CreateEventAsync(domainEvent);
        return ToDTO(created);
    }

    public async Task<EventDTO> GetEventByIdAsync(int id)
    {
        var domainEvent = await internalEventService.GetEventByIdAsync(id);
        return ToDTO(domainEvent);
    }

    public async Task<List<EventDTO>> GetAllEventsAsync()
    {
        var domainEvents = await internalEventService.GetAllEventsAsync();
        return domainEvents.Select(ToDTO).ToList();
    }

    public async Task<EventDTO> UpdateEventAsync(EventDTO updatedEvent)
    {
        var domainEvent = ToDomain(updatedEvent);
        var updated = await internalEventService.UpdateEventAsync(domainEvent);
        return ToDTO(updated);
    }

    public async Task CancelEventAsync(EventDTO eventToCancel)
    {
        var domainEvent = ToDomain(eventToCancel);
        await internalEventService.CancelEventAsync(domainEvent);
    }

    private static Event.Domain.Event ToDomain(EventDTO dto)
    {
        return new Event.Domain.Event
        {
            Id = dto.Id,
            Name = dto.Name,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Location = dto.Location,
            Description = dto.Description,
            Status = (Event.Domain.EventStatus)dto.Status,
            ContactPerson = dto.ContactPerson,
            ContactPhone = dto.ContactPhone,
            ContactEmail = dto.ContactEmail,
            Shifts = dto.Shifts?.Select(s => new Event.Domain.Shift
            {
                Id = s.Id,
                EventId = s.EventId,
                Name = s.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                RequiredStaff = s.RequiredStaff,
                Description = s.Description,
                Status = (Event.Domain.ShiftStatus)s.Status,
            }).ToList() ?? new List<Event.Domain.Shift>()
        };
    }

    private static EventDTO ToDTO(Event.Domain.Event domain)
    {
        return new EventDTO
        {
            Id = domain.Id,
            Name = domain.Name,
            StartDate = domain.StartDate,
            EndDate = domain.EndDate,
            Location = domain.Location,
            Description = domain.Description,
            Status = (EventStatusDTO)domain.Status,
            ContactPerson = domain.ContactPerson,
            ContactPhone = domain.ContactPhone,
            ContactEmail = domain.ContactEmail,
            Shifts = domain.Shifts?.Select(s => new ShiftDTO
            {
                Id = s.Id,
                EventId = s.EventId,
                Name = s.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                RequiredStaff = s.RequiredStaff,
                Description = s.Description,
                Status = (ShiftStatusDTO)s.Status
            }).ToList() ?? new List<ShiftDTO>()
        };
    }
}
