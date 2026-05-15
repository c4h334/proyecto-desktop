using proyecto_desktop.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace proyecto_desktop.Services
{
    public class CustomerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:5001/api/customers";

        public CustomerService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<List<Cliente>> GetClientesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_baseUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Cliente>>() ?? new List<Cliente>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR API GET CUSTOMERS: {ex.Message}");
                return new List<Cliente>();
            }
        }

        public async Task<Cliente?> AddClienteAsync(Cliente cliente)
        {
            var response = await _httpClient.PostAsJsonAsync(_baseUrl, cliente);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Cliente>();
        }

        public async Task UpdateClienteAsync(Guid id, Cliente cliente)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{id}", cliente);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteClienteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}