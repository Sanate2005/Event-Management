using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Final_back.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Location Location { get; set; } = new();

        public int Capacity { get; set; }

        public int OrganizerId { get; set; }
        public User Organizer { get; set; } = default!;

        public StatusEnums.EventStatus Status { get; set; } = StatusEnums.EventStatus.Draft;

        public List<Ticket> Tickets { get; set; } = new();
        public List<Participant> Participants { get; set; } = new();

        // New property to store the total number of tickets available
        public int TicketQuantity { get; set; }
    }
}