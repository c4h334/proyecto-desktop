using proyecto_desktop.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace proyecto_desktop.Services
{
    public class SupplierService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://raulvega-f7f8dfcvhbb4cmaz.mexicocentral-01.azurewebsites.net/api/suppliers";

        public SupplierService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<List<Proveedor>> GetProveedoresAsync()
        {
            var response = await _httpClient.GetAsync(_baseUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Proveedor>>() ?? new List<Proveedor>();
        }

        public async Task<Proveedor?> AddProveedorAsync(Proveedor proveedor)
        {
            var response = await _httpClient.PostAsJsonAsync(_baseUrl, proveedor);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Proveedor>();
        }

        public async Task UpdateProveedorAsync(Guid id, Proveedor proveedor)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{id}", proveedor);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteProveedorAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}