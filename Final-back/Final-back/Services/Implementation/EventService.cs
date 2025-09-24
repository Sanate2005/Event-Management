using Final_back.Data;
using Final_back.Models;
using Final_back.Requests;
using Final_back.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using System;

namespace Final_back.Services.Implementation
{
    public class EventService : IEventService
    {
        private readonly DataContext _db;
        public EventService(DataContext db) => _db = db;

        public Event Add(Event req)
        {
            req.Id = 0;
            req.OrganizerId = 2; // Set to a default value
            _db.Events.Add(req);
            _db.SaveChanges();
            return req;
        }


        public List<Event> GetAll()
            => _db.Events.Include(e => e.Location).ToList();

        public Event? Get(int id)
            => _db.Events
                  .Include(e => e.Location)
                  .Include(e => e.Tickets)
                  .FirstOrDefault(e => e.Id == id);

        public Event? Update(int id, Event req, int organizerId)
        {
            var ev = _db.Events.Find(id);
            if (ev is null || ev.OrganizerId != organizerId) return null;

            ev.Title = req.Title;
            ev.Description = req.Description;
            ev.StartDate = req.StartDate;
            ev.EndDate = req.EndDate;
            ev.Capacity = req.Capacity;
            ev.Location = req.Location;

            _db.SaveChanges();
            return ev;
        }

        public bool Delete(int id)
        {
            var ev = _db.Events.Find(id);
            if (ev is null) return false;

            _db.Events.Remove(ev);
            _db.SaveChanges();
            return true;
        }


        public async Task<Event> CreateWithTicketsAsync(CreateEventWithTicketsRequest dto, int organizerId)
        {
            // 1. event
            var ev = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Location = dto.Location,
                Capacity = dto.Capacity,
                TicketQuantity = dto.TicketQuantity,
                OrganizerId = organizerId,   // real user
                Status = StatusEnums.EventStatus.Draft
            };
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();          // need Id for tickets

            // 2. tickets
            var tickets = dto.Tickets.Select(t => new Ticket
            {
                EventId = ev.Id,
                Type = t.Type,
                Price = t.Price,
                Quantity = t.Quantity,   // <-- creator’s max
                Status = StatusEnums.TicketStatus.Available
            }).ToList();

            _db.Tickets.AddRange(tickets);
            await _db.SaveChangesAsync();

            return ev;
        }

        public List<Location> GetAllLocations()
    => _db.Locations.AsNoTracking().ToList();




    }
}
