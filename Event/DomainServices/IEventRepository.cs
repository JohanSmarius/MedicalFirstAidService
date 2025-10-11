using domain = Event.Domain;

namespace Event.DomainServices;

/// <summary>
/// Service for managing events
/// </summary>
internal interface IEventRepository
{
    Task<List<domain.Event>> GetAllEventsAsync();
    Task<domain.Event?> GetEventByIdAsync(int id);
    Task<domain.Event> CreateEventAsync(domain.Event eventModel);
    Task<domain.Event> UpdateEventAsync(domain.Event eventModel);
    Task DeleteEventAsync(int id);
    Task<List<domain.Event>> GetUpcomingEventsAsync();
    Task<List<domain.Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
}
