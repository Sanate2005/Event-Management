using System.Collections.Generic;
using System.Security.Claims;
using Final_back.Data;
using Final_back.Extensions;
using Final_back.Models;
using Final_back.Requests;
using Final_back.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Final_back.Models.StatusEnums;

namespace Final_back.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _tickets;
        private readonly DataContext _context;   // ← add

        public TicketsController(ITicketService tickets, DataContext context) {
            _tickets = tickets;

            _context = context;
        }

        // POST /api/tickets/purchase?ticketId=5&quantity=2
        [Authorize]
        [HttpPost("buy")]
        public ActionResult<List<int>> Buy([FromBody] BuyDto dto)
        {
            var ids = _tickets.SellTickets(dto.EventId, dto.Quantity, User.GetUserId());
            return ids.Count == 0 ? BadRequest("Sold out or invalid quantity.") : Ok(ids);
        }

        public record BuyDto(int EventId, int Quantity);

        public record BuyMultipleDto(int EventId, int Quantity);


        [HttpGet("remaining/{eventId:int}")]
        public ActionResult<int> GetRemaining(int eventId)
        {
            var ev = _context.Events.Find(eventId);
            if (ev == null) return NotFound();

            int sold = _context.Purchases
                               .Where(p => p.Ticket.EventId == eventId &&
                                           p.Status == StatusEnums.PurchaseStatus.Completed)
                               .Sum(p => p.Quantity);

            return ev.TicketQuantity - sold;
        }



        // GET /api/tickets/{id}
        [HttpGet("{id:int}")]
        public ActionResult<Ticket> Get(int id)
            => _tickets.Get(id) is { } t ? Ok(t) : NotFound();

        // POST /api/tickets/validate?ticketId=5&userId=GUID
        [HttpPost("validate")]
        public IActionResult Validate([FromQuery] int ticketId, [FromQuery] Guid userId)
            => _tickets.Validate(ticketId, User.GetUserId()) ? Ok("Valid") : BadRequest("Invalid ticket");

        // POST /api/tickets/add-for-event/{eventId}
        [HttpPost("add-for-event/{eventId}")]
        public ActionResult<List<Ticket>> AddTicketsForEvent([FromRoute] int eventId, [FromBody] List<Ticket> tickets)
        {
            try
            {
                var createdTickets = _tickets.AddTicketsForEvent(eventId, tickets);
                return Ok(createdTickets);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while adding tickets: " + ex.Message);
            }
        }


        public class SoldTicketDto
        {
            public int PurchaseId { get; set; }   // PK we’ll use for delete
            public string BuyerName { get; set; }
            public string EventTitle { get; set; }
            public string TicketType { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public DateTime BoughtAt { get; set; }
        }


        // GET  /api/tickets/sold
        [Authorize]
        [HttpGet("sold")]
        public async Task<ActionResult<List<SoldTicketDto>>> GetAllSoldTickets()
        {
            var raw = await _context.Purchases
    .Include(p => p.User)
    .Include(p => p.Ticket)
        .ThenInclude(t => t.Event)
    .Where(p => p.Status == PurchaseStatus.Completed)
    .ToListAsync();          // <-- materialise here

            var data = raw.Select(p => new SoldTicketDto
            {
                PurchaseId = p.Id,
                BuyerName = p.User?.FullName ?? "N/A",
                EventTitle = p.Ticket?.Event?.Title ?? "N/A",
                TicketType = p.Ticket?.Type ?? "N/A",
                Price = p.TotalAmount,
                Quantity = p.Quantity,
                BoughtAt = p.PurchaseDate
            }).ToList();

            return data;
        }

        // DELETE  /api/tickets/sold/{purchaseId}
        [Authorize]
        [HttpDelete("sold/{purchaseId:int}")]
        public async Task<IActionResult> DeleteSoldTicket(int purchaseId)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Ticket)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null) return NotFound();

            if (purchase.Ticket != null)               // <-- NEW
                purchase.Ticket.Status = StatusEnums.TicketStatus.Available;

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<ActionResult<List<SoldTicketDto>>> GetMyTickets()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var data = await _context.Purchases
                .Include(p => p.User)
                .Include(p => p.Ticket)
                    .ThenInclude(t => t.Event)
                .Where(p => p.UserId == userId &&
                            p.Status == PurchaseStatus.Completed)
                .Select(p => new SoldTicketDto
                {
                    PurchaseId = p.Id,
                    BuyerName = p.User.FullName,
                    EventTitle = p.Ticket.Event.Title,
                    TicketType = p.Ticket.Type,
                    Price = p.TotalAmount,
                    Quantity = p.Quantity,
                    BoughtAt = p.PurchaseDate
                })
                .ToListAsync();

            return data;
        }

        [HttpGet("types/{eventId:int}")]
        public async Task<ActionResult<List<object>>> GetTicketTypes(int eventId)
        {
            var types = await _context.Tickets
                .Where(t => t.EventId == eventId && t.Status == TicketStatus.Available)
                .Select(t => new
                {
                    TicketId = t.Id,
                    Type = t.Type,
                    Price = t.Price
                })
                .ToListAsync();

            return Ok(types);   // <-- was missing
        }


        [Authorize]
        [HttpPost("buy-by-id")]
        public async Task<ActionResult<List<int>>> BuyById([FromBody] BuyByIdDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // 1. stock check
            var ticket = await _context.Tickets
                                  .FirstOrDefaultAsync(t => t.Id == dto.TicketId);
            if (ticket == null || ticket.Quantity < dto.Quantity)
                return BadRequest("Not enough tickets left.");

            // 2. decrement stock
            ticket.Quantity -= dto.Quantity;

            // 3. ONE purchase row for the whole quantity
            var purchase = new Purchase
            {
                TicketId = dto.TicketId,
                UserId = userId,
                Quantity = dto.Quantity,          // whole block
                TotalAmount = dto.Price * dto.Quantity,
                Status = PurchaseStatus.Completed,
                PurchaseDate = DateTime.UtcNow
            };
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            // return single ID (or list with one item to keep Angular happy)
            return Ok(new List<int> { purchase.Id });
        }


        [HttpGet("remaining/{ticketId:int}")]
        public ActionResult<int> RemainingForType(int ticketId)
=> _context.Tickets
           .Where(t => t.Id == ticketId)
           .Select(t => t.Quantity)
           .FirstOrDefault();



        [HttpGet("remaining-per-event/{eventId:int}")]
        public async Task<ActionResult<List<RemainingPerTypeDto>>> GetRemainingPerEvent(int eventId)
        {
            var list = await _context.Tickets
                .Where(t => t.EventId == eventId && t.Status == TicketStatus.Available)
                .Select(t => new RemainingPerTypeDto(
                    t.Id,
                    t.Type,
                    t.Quantity))
                .ToListAsync();

            return Ok(list);
        }

        public record RemainingPerTypeDto(int TicketId, string Type, int Remaining);

        public record BuyByIdDto(int TicketId, int Quantity, decimal Price);


    }
}