namespace Final_back.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }

        // FOR AUTH
        public string Email { get; set; }
        public string Password { get; set; } // to be hashed

        //ROLE

        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }

        // For Verification
        public bool HasConfirmed { get; set; }
        public string? ConfirmationCode { get; set; }

        public List<Event> OrganizedEvents { get; set; } = new();
        public List<Purchase> Purchases { get; set; } = new();
        public List<Participant> ParticipantEntries { get; set; } = new();
    }
}
