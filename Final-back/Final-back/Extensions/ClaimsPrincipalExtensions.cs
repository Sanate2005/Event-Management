using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final_back.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
            => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
