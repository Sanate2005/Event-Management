using Final_back.Models;

namespace Final_back.Requests
{
    public record EventDetails(
    int Id,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    LocationModel Location,
    int Capacity,
    StatusEnums.EventStatus Status);
}
