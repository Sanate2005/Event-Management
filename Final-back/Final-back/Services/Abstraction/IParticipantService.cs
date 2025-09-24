using Final_back.Models;

namespace Final_back.Services.Abstraction
{
    public interface IParticipantService
    {
        Participant? Register(int eventId, int userId, int ticketId);
        List<Participant> GetByEvent(int eventId);
        Participant? Get(int id);
    }
}
