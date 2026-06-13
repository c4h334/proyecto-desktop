using proyecto_desktop.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;

namespace proyecto_desktop.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://raulvega-f7f8dfcvhbb4cmaz.mexicocentral-01.azurewebsites.net/api/users";

        public UserService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_baseUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Usuario>>() ?? new List<Usuario>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR API GET USERS: {ex.Message}");
                return GetMockUsuarios();
            }
        }

        public async Task<Usuario?> AddUsuarioAsync(Usuario usuario, string password)
        {
            var request = new
            {
                name = usuario.Name,
                username = usuario.Username,
                email = usuario.Email,
                password = password
            };

            var response = await _httpClient.PostAsJsonAsync(_baseUrl, request);
            response.EnsureSuccessStatusCode();
            var creado = await response.Content.ReadFromJsonAsync<Usuario>();

            // Si se creó el usuario y tiene roles específicos definidos, actualizarlos mediante PUT
            if (creado != null && creado.UserResourceId.HasValue && usuario.RolesList.Count > 0)
            {
                creado.RolesList = usuario.RolesList;
                await UpdateUsuarioAsync(creado.UserResourceId.Value, creado, null);
            }

            return creado;
        }

        public async Task UpdateUsuarioAsync(Guid id, Usuario usuario, string? password)
        {
            var request = new
            {
                name = usuario.Name,
                username = usuario.Username,
                email = usuario.Email,
                password = password,
                roles = usuario.RolesList
            };

            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{id}", request);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteUsuarioAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
            response.EnsureSuccessStatusCode();
        }

        private List<Usuario> GetMockUsuarios()
        {
            return new List<Usuario>
            {
                new Usuario
                {
                    UserResourceId = Guid.NewGuid(),
                    Name = "Raúl Vega",
                    Username = "raul.vega",
                    Email = "raul.vega@empresa.com",
                    RolesList = new List<string> { "Administrator", "Support" }
                },
                new Usuario
                {
                    UserResourceId = Guid.NewGuid(),
                    Name = "Ana Gómez",
                    Username = "ana.gomez",
                    Email = "ana.gomez@empresa.com",
                    RolesList = new List<string> { "Support" }
                },
                new Usuario
                {
                    UserResourceId = Guid.NewGuid(),
                    Name = "Luis Mora",
                    Username = "luis.mora",
                    Email = "luis.mora@cliente.com",
                    RolesList = new List<string> { "Customer" }
                },
                new Usuario
                {
                    UserResourceId = Guid.NewGuid(),
                    Name = "María Salas",
                    Username = "maria.salas",
                    Email = "maria.salas@cliente.com",
                    RolesList = new List<string> { "Customer" }
                }
            };
        }
    }
}
