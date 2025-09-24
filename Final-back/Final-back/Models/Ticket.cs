using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_back.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; } = default!;

        public int? UserId { get; set; }          // ← NEW
        public User? User { get; set; }

        public int Quantity { get; set; }

        [Required, MaxLength(100)]
        public string Type { get; set; } = default!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public StatusEnums.TicketStatus Status { get; set; } = StatusEnums.TicketStatus.Available;

        public List<Purchase> Purchases { get; set; } = new();
        public List<Participant> Participants { get; set; } = new();
    }
}