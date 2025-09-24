using Final_back.Models;

namespace Final_back.Requests
{
    public class CreateEventWithTicketsRequest
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Location Location { get; set; } = new();
        public int Capacity { get; set; }
        public int TicketQuantity { get; set; }

        public List<TicketDto> Tickets { get; set; } = new();
    }

    public class TicketDto
    {
        public string Type { get; set; } = default!; // "Regular", "VIP", etc.
        public decimal Price { get; set; }
        public int Quantity { get; set; }           // how many of this type
    }
}
