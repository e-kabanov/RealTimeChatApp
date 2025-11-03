using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace RealTimeChatApp.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId (this ClaimsPrincipal user)
        {
           var userIdClaim = user.FindFirst (ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token");
            }

            return userId;
        }
    }
}
