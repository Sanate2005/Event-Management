using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Final_back.Data;
using Final_back.Models;
using Microsoft.EntityFrameworkCore;
using Final_back.Services.Abstraction;
using System;

namespace Final_back.Services.Implementation
{
    public class ParticipantService : IParticipantService
    {
        private readonly DataContext _db;
        public ParticipantService(DataContext db) => _db = db;

        public Participant? Register(int eventId, int userId, int ticketId)
        {
            var ok = _db.Purchases.Any(p =>
                p.TicketId == ticketId &&
                p.UserId == userId &&
                p.Status == StatusEnums.PurchaseStatus.Completed);

            if (!ok) return null;

            var entry = new Participant
            {
                EventId = eventId,
                UserId = userId,
                TicketId = ticketId
            };
            _db.Participants.Add(entry);
            _db.SaveChanges();
            return entry;
        }

        public List<Participant> GetByEvent(int eventId)
            => _db.Participants.Include(p => p.User)
                               .Where(p => p.EventId == eventId)
                               .ToList();

        public Participant? Get(int id)
            => _db.Participants.Include(p => p.User)
                               .Include(p => p.Event)
                               .FirstOrDefault(p => p.Id == id);
    }
}
