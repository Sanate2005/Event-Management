namespace Final_back.Services.Abstraction
{
    public interface IAnalyticsService
    {
        object GetEventAnalytics(int eventId);
        object GetAttendance(int eventId);
    }
}
