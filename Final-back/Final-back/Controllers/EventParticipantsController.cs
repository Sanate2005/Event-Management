using Final_back.Extensions;
using Final_back.Models;
using Final_back.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Final_back.Controllers
{
    [ApiController]
    [Route("api/events/{eventId:int}/participants")]
    public class EventParticipantsController : ControllerBase
    {
        private readonly IParticipantService _parts;
        public EventParticipantsController(IParticipantService parts) => _parts = parts;

        // GET
        [HttpGet]
        public ActionResult<IEnumerable<Participant>> Get(int eventId)
            => Ok(_parts.GetByEvent(eventId));

        // POST /api/events/{eventId}/participants/register?ticketId=3
        [Authorize]
        [HttpPost("register")]
        public ActionResult<Participant> Register(int eventId, [FromQuery] int ticketId)
        {
            var p = _parts.Register(eventId, User.GetUserId(), ticketId);
            return p is null ? BadRequest("Registration failed.") : Ok(p);
        }
    }
}
