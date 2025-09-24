namespace Final_back.Requests
{
    public record RegisterUser(
        string FullName, 
        string Email, 
        string Password);
}
