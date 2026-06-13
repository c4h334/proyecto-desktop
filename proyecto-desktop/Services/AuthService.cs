using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace proyecto_desktop.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient = new(new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true });

        public async Task<AuthResponse?> LoginAsync(string user, string pass)
        {
            var res = await _httpClient.PostAsJsonAsync("https://raulvega-f7f8dfcvhbb4cmaz.mexicocentral-01.azurewebsites.net/api/authorization/authorize", new { Username = user, Password = pass });
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<AuthResponse>() : null;
        }

        public async Task<bool> RegisterAdminAsync(string n, string u, string e, string p)
        {
            var res = await _httpClient.PostAsJsonAsync("https://raulvega-f7f8dfcvhbb4cmaz.mexicocentral-01.azurewebsites.net/api/users/admin", new { Name = n, Username = u, Email = e, Password = p });
            return res.IsSuccessStatusCode;
        }

        public bool IsAdministrator(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                // Leemos el token sin validarlo (solo queremos extraer los claims)
                var jwtToken = handler.ReadJwtToken(token);

                // Buscamos cualquier claim de tipo Role que tenga el valor "Administrator"
                return jwtToken.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Administrator");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al validar rol: {ex.Message}");
                return false;
            }
        }
    }

    public class AuthResponse
    {
        public string BearerToken { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}