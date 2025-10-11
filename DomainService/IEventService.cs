using Domain;

namespace DomainService;

public interface IEventService
{
    Task<Event> CreateEventAsync(Event newEvent);
    Task<Event> GetEventByIdAsync(int id);
    Task<List<Event>> GetAllEventsAsync();
    Task<Event> UpdateEventAsync(Event e);
    Task CancelEventAsync(Event eventToCancel);
}