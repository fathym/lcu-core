using System.Linq;

namespace System.Security.Claims
{
    public static class LCUIdentityExtensions
    {
        public static string LoadClaim(this ClaimsPrincipal user, string claim)
        {
            return user?.Claims?.FirstOrDefault(c => c.Type == claim)?.Value;
        }

        public static string LoadUsername(this ClaimsPrincipal user)
        {
            var username = user.LoadClaim("emails");

            if (username.IsNullOrEmpty())
                username = user.LoadClaim("signInNames.emailAddress");

            if (username.IsNullOrEmpty())
                username = user.LoadClaim(ClaimTypes.Email);

            return username;
        }
    }
}
