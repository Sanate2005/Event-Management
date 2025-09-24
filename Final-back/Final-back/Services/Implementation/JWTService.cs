using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Final_back;
using Final_back.Data;
using Final_back.Models;
using Microsoft.EntityFrameworkCore;
using Final_back.Services.Abstraction;

namespace Final_back.Services.Implementation

{
    public class JWTService : IJWTService
    {
        private readonly DataContext _context;

        public JWTService(DataContext context)
        {
            _context = context;
        }

        public UserToken GenerateToken(User user)
        {
            var jwtKey = "cc14dc33397c2b073b1ee85c270fd301a83b9494c472119bebe1bd37a187fad1c21edd872dc84b904079015bd29cc2b26972b97a19709aa31ad2c7fa161569fbae1b8ff71440251aeccd72106ba5118035201e8d6f6ba0dfa8647cac7b3f525cca0ef27f85789527cfe265b1936944dcf008e1dd616c526bf310e90a0ba1df7a5da4610193dc3ab34e9e10f2c7c2b510e299c27f7b517c05986908f5aeead9a8b48623b0ebb43ebaf1bb5db7029a5ef11f11ea7e148292714198428e629ef0b6fdec1d6085243ee42a608e854b9e90b3196506439d7962c3d5fd69309f20287a8cdb4eed2afbee1339d8a9d3a84d447fd1940c85bbe6ab7988f1f16c9aa967fe";
            var jwtIssuer = "chven";
            var jwtAudience = "isini";
            var durationInMinutes = 300;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var signature = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.NameId, $"{user.Id}"),
            new Claim(JwtRegisteredClaimNames.Name, $"{user.FullName}"),
            new Claim(JwtRegisteredClaimNames.Email, $"{user.Email}"),
        };


            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                expires: DateTime.Now.AddMinutes(durationInMinutes),
                signingCredentials: signature,
                claims: claims
                );


            return new UserToken
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }
    }
}
