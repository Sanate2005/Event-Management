using Final_back.Models;
using Final_back.Requests;

namespace Final_back.Services.Abstraction
{
    public interface IUserService
    {
        User AddUser(AddUser req);                  
        List<User> GetUsers();                        
        bool VerifyUser(string email, string code);  
        User? GetProfile(int id);                    
        User? Login(string email, string password);  
    }
}
