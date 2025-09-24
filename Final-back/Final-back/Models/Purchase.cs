using System.ComponentModel.DataAnnotations.Schema;

namespace Final_back.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        public int? TicketId { get; set; }
        public Ticket? Ticket { get; set; } = default!;

        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        public StatusEnums.PurchaseStatus Status { get; set; } = StatusEnums.PurchaseStatus.Pending;
    }
}
