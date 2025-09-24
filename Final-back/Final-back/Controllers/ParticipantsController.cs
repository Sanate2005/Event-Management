using Final_back.Models;
using Final_back.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace Final_back.Controllers
{
    [ApiController]
    [Route("api/participants")]
    public class ParticipantsController : ControllerBase
    {
        private readonly IParticipantService _parts;
        public ParticipantsController(IParticipantService parts) => _parts = parts;

        // GET /api/participants/{id}
        [HttpGet("{id:int}")]
        public ActionResult<Participant> Get(int id)
            => _parts.Get(id) is { } p ? Ok(p) : NotFound();
    }
}
