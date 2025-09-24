using Final_back.Data;
using Final_back.Models;
using Final_back.Requests;
using Final_back.Services.Abstraction;

namespace Final_back.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly DataContext _db;
        public UserService(DataContext db) => _db = db;

        public User AddUser(AddUser req)
        {
            if (_db.Users.Any(u => u.Email == req.Email))
                throw new InvalidOperationException("Email already registered.");

            var user = new User
            {
                FullName = req.FullName,
                Email = req.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                HasConfirmed = false,
                ConfirmationCode = Guid.NewGuid().ToString("N")
            };

            _db.Users.Add(user);
            _db.SaveChanges();
            // TODO: send confirmation e-mail containing user.ConfirmationCode
            return user;
        }

        public List<User> GetUsers() => _db.Users.ToList();

        public bool VerifyUser(string email, string code)
        {
            var user = _db.Users.SingleOrDefault(u => u.Email == email);
            if (user is null) return false;
            if (user.HasConfirmed || user.ConfirmationCode != code) return false;

            user.HasConfirmed = true;
            user.ConfirmationCode = null;
            _db.SaveChanges();
            return true;
        }

        public User? GetProfile(int id)
            => _db.Users.Find(id);

        public User? Login(string email, string password)
        {
            var user = _db.Users.SingleOrDefault(u => u.Email == email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                return null;

            return user;          // caller can generate JWT if desired
        }
    }
}
