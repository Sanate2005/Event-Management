namespace Final_back.Requests
{
    public record BuyTicketRequest(int TicketId, int Quantity = 1);
    public record BuyTicketResponse(
        int PurchaseId,
        int TicketId,
        int UserId,
        int Quantity,
        decimal TotalAmount,
        DateTime PurchaseDate);
}
