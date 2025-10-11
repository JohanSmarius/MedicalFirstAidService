using System;
using System.Collections.Generic;
using System.Text;

namespace Event.API
{
    public interface IEventService
    {
        Task CancelEventAsync(EventDTO eventToCancel);
        Task<EventDTO> CreateEventAsync(EventDTO newEvent);
        Task<List<EventDTO>> GetAllEventsAsync();
        Task<EventDTO> GetEventByIdAsync(int id);
        Task<EventDTO> UpdateEventAsync(EventDTO updatedEvent);
    }
}
