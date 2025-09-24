using Final_back.Models;
using Final_back.Requests;

namespace Final_back.Services.Abstraction
{
    public interface IEventService
    {
        Event Add(Event model);
        List<Event> GetAll();
        Event? Get(int id);
        Event? Update(int id, Event req, int organizerId);
        Task<Event> CreateWithTicketsAsync(CreateEventWithTicketsRequest dto, int organizerId);
        public bool Delete(int id);
        List<Location> GetAllLocations();
    }
}
