using System.ComponentModel.DataAnnotations;

namespace Final_back.Models
{
    public class Location
    {
        public string Address { get; set; } = default!;
        public string City { get; set; } = default!;
        public string Country { get; set; } = default!;
    }
}
