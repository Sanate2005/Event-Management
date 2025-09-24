using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Final_back.Data;
using Final_back.Models;
using Microsoft.EntityFrameworkCore;
using Final_back.Services.Abstraction;

namespace Final_back.Services.Implementation
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly DataContext _db;
        public AnalyticsService(DataContext db) => _db = db;

        public object GetEventAnalytics(int eventId)
        {
            var ticketsSold = _db.Purchases
                                 .Where(p => p.Ticket.EventId == eventId &&
                                             p.Status == StatusEnums.PurchaseStatus.Completed)
                                 .Sum(p => p.Quantity);

            var revenue = _db.Purchases
                             .Where(p => p.Ticket.EventId == eventId &&
                                         p.Status == StatusEnums.PurchaseStatus.Completed)
                             .Sum(p => p.TotalAmount);

            return new { TicketsSold = ticketsSold, Revenue = revenue };
        }

        public object GetAttendance(int eventId)
        {
            var checkedIn = _db.Participants.Count(p => p.EventId == eventId && p.Attendance);
            var registered = _db.Participants.Count(p => p.EventId == eventId);
            return new { Registered = registered, CheckedIn = checkedIn };
        }
    }
}
