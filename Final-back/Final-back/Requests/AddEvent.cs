using System.ComponentModel.DataAnnotations;

namespace Final_back.Requests
{
    public record AddEvent(
        [Required, MaxLength(200)] string Title,
        string? Description,
        [Required] DateTime StartDate,
        [Required] DateTime EndDate,
        [Required] LocationModel Location,
        [Required] int Capacity,
        [Required] int TicketQuantity // New property
    );
}
