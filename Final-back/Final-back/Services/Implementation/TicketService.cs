using System;
using System.Collections.Generic;
using System.Linq;
using Final_back.Data;
using Final_back.Models;
using Final_back.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace Final_back.Services.Implementation
{
    public class TicketService : ITicketService
    {
        private readonly DataContext _db;
        public TicketService(DataContext db) => _db = db;

        public Ticket? Get(int id) => _db.Tickets.Find(id);

        public List<Ticket> GetByEvent(int eventId)
            => _db.Tickets.Where(t => t.EventId == eventId).ToList();

        /*  legacy single-purchase method (keep)  */
        public Purchase? Buy(int ticketId, int quantity, int userId)
        {
            var ticket = _db.Tickets.Include(t => t.Event)
                                    .FirstOrDefault(t => t.Id == ticketId);
            if (ticket is null || ticket.Status == StatusEnums.TicketStatus.SoldOut)
                return null;

            var sold = _db.Purchases
                          .Where(p => p.TicketId == ticketId &&
                                      p.Status == StatusEnums.PurchaseStatus.Completed)
                          .Sum(p => p.Quantity);

            if (sold + quantity > ticket.Event.Capacity) return null;

            var purchase = new Purchase
            {
                TicketId = ticketId,
                UserId = userId,
                Quantity = quantity,
                TotalAmount = ticket.Price * quantity,
                PurchaseDate = DateTime.UtcNow,
                Status = StatusEnums.PurchaseStatus.Completed
            };

            _db.Purchases.Add(purchase);

            if (sold + quantity >= ticket.Event.Capacity)
                ticket.Status = StatusEnums.TicketStatus.SoldOut;

            _db.SaveChanges();
            return purchase;
        }

        public List<int> SellTickets(int eventId, int quantity, int userId)
        {
            if (quantity <= 0) return new();

            var ev = _db.Events.Find(eventId);
            if (ev == null) return new();

            // how many are already sold?
            int sold = _db.Purchases
                          .Where(p => p.Ticket.EventId == eventId &&
                                      p.Status == StatusEnums.PurchaseStatus.Completed)
                          .Sum(p => p.Quantity);

            if (sold + quantity > ev.TicketQuantity) return new(); // would exceed pool

            // create ONE purchase row for this user
            var purchase = new Purchase
            {
                UserId = userId,
                Quantity = quantity,
                TotalAmount = quantity * 10.00m, // or fetch price from Event
                Status = StatusEnums.PurchaseStatus.Completed,
                PurchaseDate = DateTime.UtcNow
            };

            _db.Purchases.Add(purchase);
            _db.SaveChanges();

            // return a fake id list (or simply the purchase id repeated)
            return Enumerable.Repeat(purchase.Id, quantity).ToList();
        }

        /*  new bulk-buy method  */
        public List<int> BuyMultiple(int eventId, int quantity, int userId)
        {
            if (quantity <= 0) return new();

            var evt = _db.Events.Find(eventId);
            if (evt is null) return new();

            var sold = _db.Tickets
                          .Count(t => t.EventId == eventId &&
                                      t.Status == StatusEnums.TicketStatus.SoldOut);

            if (sold + quantity > evt.Capacity) return new(); // oversell guard

            var ticketIds = new List<int>();
            const decimal unitPrice = 10.00m; // or pass via DTO / read from Event

            for (int i = 0; i < quantity; i++)
            {
                var ticket = new Ticket
                {
                    EventId = eventId,
                    Type = "Standard",
                    Price = unitPrice,
                    Status = StatusEnums.TicketStatus.SoldOut,
                    Purchases = new List<Purchase>
                    {
                        new Purchase
                        {
                            UserId      = userId,
                            Quantity    = 1,
                            TotalAmount = unitPrice,
                            PurchaseDate= DateTime.UtcNow,
                            Status      = StatusEnums.PurchaseStatus.Completed
                        }
                    }
                };
                _db.Tickets.Add(ticket);
                _db.SaveChanges();          // save to get Id
                ticketIds.Add(ticket.Id);
            }
            return ticketIds;
        }

        public bool Validate(int ticketId, int userId)
            => _db.Purchases.Any(p =>
                   p.TicketId == ticketId &&
                   p.UserId == userId &&
                   p.Status == StatusEnums.PurchaseStatus.Completed);

        public List<Ticket> AddTicketsForEvent(int eventId, List<Ticket> tickets)
        {
            if (!_db.Events.Any(e => e.Id == eventId))
                throw new ArgumentException("Event does not exist.");

            foreach (var t in tickets)
            {
                t.EventId = eventId;
                t.Status = StatusEnums.TicketStatus.Available;
                _db.Tickets.Add(t);
            }
            _db.SaveChanges();
            return tickets;
        }
    }
}