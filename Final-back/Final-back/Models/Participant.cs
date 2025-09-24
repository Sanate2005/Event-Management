namespace Final_back.Models
{
    public class Participant
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; } = default!;

        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = default!;

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public bool Attendance { get; set; } = false;
    }
}
