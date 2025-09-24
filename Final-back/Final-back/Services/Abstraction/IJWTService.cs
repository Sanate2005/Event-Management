using Final_back.Models;

namespace Final_back.Services.Abstraction
{
    public interface IJWTService
    {
        UserToken GenerateToken(User user);
    }
}

