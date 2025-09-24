using Final_back.Models;
using Final_back.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace Final_back.Controllers
{
    [ApiController]
    [Route("api/events/{eventId:int}/tickets")]
    public class EventTicketsController : ControllerBase
    {
        private readonly ITicketService _tickets;
        public EventTicketsController(ITicketService tickets) => _tickets = tickets;

        [HttpGet]
        public ActionResult<IEnumerable<Ticket>> Get(int eventId)
            => Ok(_tickets.GetByEvent(eventId));
    }
}
