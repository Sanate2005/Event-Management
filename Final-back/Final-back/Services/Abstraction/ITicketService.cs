using System.Collections.Generic;
using Final_back.Models;

namespace Final_back.Services.Abstraction
{
    public interface ITicketService
    {
        Ticket? Get(int id);
        List<Ticket> GetByEvent(int eventId);
        Purchase? Buy(int ticketId, int quantity, int userId);   // keep legacy
        List<int> SellTickets(int eventId, int quantity, int userId); // NEW
        bool Validate(int ticketId, int userId);
        List<Ticket> AddTicketsForEvent(int eventId, List<Ticket> tickets);
    }
}