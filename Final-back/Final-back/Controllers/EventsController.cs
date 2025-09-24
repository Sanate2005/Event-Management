using Final_back.Models;
using Final_back.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Final_back.Extensions;
using Final_back.Requests;
using Microsoft.EntityFrameworkCore;
using Final_back.Data;

namespace Final_back.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _events;
        private readonly DataContext _context;   // or YourDbContext

        public EventsController(IEventService events, DataContext context)
        {
            _events = events;
            _context = context;
        }

        // GET /api/events
        [HttpGet("AllEvents")]
        public ActionResult<IEnumerable<EventDto>> GetAll()
        {
            var list = _events.GetAll()
                .Select(e => new EventDto(
                    e.Id,
                    e.Title,
                    e.Description,
                    e.StartDate,
                    e.EndDate,
                    e.Capacity,
                    e.TicketQuantity,
                    new LocationDto(
                        e.Location.Address,
                        e.Location.City,
                        e.Location.Country)))
                .ToList();

            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public ActionResult<EventDto> Get(int id)
        {
            var ev = _events.Get(id);
            if (ev is null) return NotFound();

            var dto = new EventDto(
                ev.Id,
                ev.Title,
                ev.Description,
                ev.StartDate,
                ev.EndDate,
                ev.Capacity,
                ev.TicketQuantity,
                new LocationDto(
                    ev.Location.Address,
                    ev.Location.City,
                    ev.Location.Country));

            return Ok(dto);
        }

        // POST /api/events/add
        [HttpPost("addevent")]
        public ActionResult<Event> Add([FromBody] AddEvent request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body is null.");
                }

                var eventEntity = new Event
                {
                    Title = request.Title,
                    Description = request.Description,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Location = new Location
                    {
                        Address = request.Location.Address,
                        City = request.Location.City,
                        Country = request.Location.Country
                    },
                    Capacity = request.Capacity,
                    TicketQuantity = request.TicketQuantity, // Set TicketQuantity
                    Status = StatusEnums.EventStatus.Draft
                };

                var created = _events.Add(eventEntity);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException);
                }
                Console.WriteLine(ex.StackTrace);
                return BadRequest("Failed to create event: " + ex.Message);
            }
        }



        [Authorize]
        [HttpPost("create-with-tickets")]
        public async Task<ActionResult<EventWithTicketsDto>> CreateWithTickets([FromBody] CreateEventWithTicketsRequest dto)
        {
            var orgId = User.GetUserId();
            var created = await _events.CreateWithTicketsAsync(dto, orgId);

            var resp = new EventWithTicketsDto(
                created.Id,
                created.Title,
                created.Description,
                created.StartDate,
                created.EndDate,
                created.Capacity,
                created.TicketQuantity,
                new LocationDto(created.Location.Address, created.Location.City, created.Location.Country),
                created.Tickets.Select(t => new TicketSlimDto(t.Id, t.Type, t.Price)).ToList()
            );

            return CreatedAtAction(nameof(Get), new { id = resp.Id }, resp);
        }

        // PUT /api/events/{id}
        [Authorize]
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, Event model)
        {
            var updated = _events.Update(id, model, User.GetUserId());
            return updated is null ? Forbid() : Ok(updated);
        }

        // DELETE /api/events/{id}
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
    => _events.Delete(id) ? NoContent() : NotFound();



        [HttpGet("locations")]
        public ActionResult<IEnumerable<LocationDto>> GetLocations()
        {
            var list = _context.Events
                .Select(e => new LocationDto(
                    e.Location.Address,
                    e.Location.City,
                    e.Location.Country))
                .Distinct()
                .ToList();

            return Ok(list);
        }




        public record EventDto(
    int Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    int TicketQuantity,
    LocationDto Location
);

        public record LocationDto(
            string Address,
            string City,
            string Country
        );

        public record EventWithTicketsDto(
        int Id,
        string Title,
        string Description,
        DateTime StartDate,
        DateTime EndDate,
        int Capacity,
        int TicketQuantity,
        LocationDto Location,
        List<TicketSlimDto> Tickets
    );

        public record TicketSlimDto(int Id, string Type, decimal Price);

    }
}