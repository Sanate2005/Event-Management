namespace Final_back.Models
{
    public class StatusEnums
    {
        public enum EventStatus
        {
            Draft = 0,
            Published = 1,
            Completed = 2
        }


        public enum TicketStatus
        {
            Available = 0,
            SoldOut = 1,
            Sold = 2      
        }


        public enum PurchaseStatus
        {
            Pending = 0,
            Completed = 1
        }
    }
}
