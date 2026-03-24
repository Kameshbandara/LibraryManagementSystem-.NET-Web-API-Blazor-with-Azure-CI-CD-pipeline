using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LibraryBlazorApp.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Simulate a loading delay as requested to show the loading UI
            await Task.Delay(2000);

            try
            {
                var token = await _localStorage.GetItemAsStringAsync("accessToken");

                if (string.IsNullOrWhiteSpace(token))
                    return new AuthenticationState(_anonymous);

                var claims = ParseClaimsFromJwt(token);
                if (claims == null)
                    return new AuthenticationState(_anonymous);

                // Check if token is expired
                var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
                if (expClaim != null)
                {
                    var expDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value));
                    if (expDate <= DateTimeOffset.UtcNow)
                    {
                        // Token expired - return anonymous (the auto-refresh handler will take care of refresh)
                        return new AuthenticationState(_anonymous);
                    }
                }

                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            var authState = Task.FromResult(new AuthenticationState(user));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(new AuthenticationState(_anonymous));
            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                return token.Claims;
            }
            catch
            {
                return null;
            }
        }
    }
}
