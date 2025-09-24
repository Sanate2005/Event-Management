using Final_back.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Final_back.Data;               // <-- gives you DataContext
using Final_back.Models;

namespace Final_back.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analytics;
        private readonly DataContext _context;   // <-- ADD
        public AnalyticsController(DataContext context) => _context = context;

        [Authorize]
        [HttpGet("analytics/all")]
        public async Task<ActionResult<object>> GetAllAnalytics()
        {
            var rows = await _context.Events
    .Include(e => e.Tickets)
        .ThenInclude(t => t.Purchases)
    .Include(e => e.Participants)
    .Select(e => new
    {
        EventId = e.Id,
        Title = e.Title,
        StartDate = e.StartDate,
        TicketsSold = e.Tickets
                        .SelectMany(t => t.Purchases)
                        .Where(p => p.Status == StatusEnums.PurchaseStatus.Completed)
                        .Sum(p => p.Quantity),
        Revenue = e.Tickets
                        .SelectMany(t => t.Purchases)
                        .Where(p => p.Status == StatusEnums.PurchaseStatus.Completed)
                        .Sum(p => p.TotalAmount),
        Registered = e.Participants.Count,
        CheckedIn = e.Participants.Count(p => p.Attendance)
    })
    .ToListAsync();

            var totals = new
            {
                TotalRevenue = rows.Sum(r => r.Revenue),
                TotalTickets = rows.Sum(r => r.TicketsSold),
                TotalRegistered = rows.Sum(r => r.Registered),
                TotalCheckedIn = rows.Sum(r => r.CheckedIn)
            };

            return new { Totals = totals, Events = rows };
        } 
        public AnalyticsController(IAnalyticsService analytics) => _analytics = analytics;

        [HttpGet("analytics")]
        public ActionResult<object> Analytics(int eventId)
            => Ok(_analytics.GetEventAnalytics(eventId));

        [HttpGet("attendance")]
        public ActionResult<object> Attendance(int eventId)
            => Ok(_analytics.GetAttendance(eventId));


    }
}
