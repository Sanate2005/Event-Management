using Final_back.Models;

namespace Final_back.Services.Abstraction
{
    public interface IAuthService
    {
        User Register(User req);
        string? Login(string email, string password);  
    }
}
