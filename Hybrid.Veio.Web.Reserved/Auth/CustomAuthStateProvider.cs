using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Hybrid.Veio.Web.Reserved.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private bool _initialized = false;
        private AuthenticationState _anonymous;

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            _tokenHandler = new JwtSecurityTokenHandler();
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(_anonymous);
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                var token = await _localStorage.GetItemAsync<string>("jwt_token");

                if (!string.IsNullOrEmpty(token))
                {
                    var tokenContent = _tokenHandler.ReadJwtToken(token);
                    var claims = tokenContent.Claims;
                    var identity = new ClaimsIdentity(claims, "jwt");
                    var user = new ClaimsPrincipal(identity);
                    _anonymous = new AuthenticationState(user);
                }

                _initialized = true;
                NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
            }
            catch
            {
                // In caso di errore, mantieni lo stato non autenticato
            }
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
