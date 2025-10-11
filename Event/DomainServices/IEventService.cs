using domain = Event.Domain;

namespace Event.DomainServices;

internal interface IEventService
{
    Task<domain.Event> CreateEventAsync(domain.Event newEvent);
    Task<domain.Event> GetEventByIdAsync(int id);
    Task<List<domain.Event>> GetAllEventsAsync();
    Task<domain.Event> UpdateEventAsync(domain.Event e);
    Task CancelEventAsync(domain.Event eventToCancel);
}