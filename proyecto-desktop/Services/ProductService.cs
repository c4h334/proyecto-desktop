using proyecto_desktop.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace proyecto_desktop.Services
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://raulvega-f7f8dfcvhbb4cmaz.mexicocentral-01.azurewebsites.net/api/products";
        private readonly string _adminUrl = "https://raulvega-f7f8dfcvhbb4cmaz.mexicocentral-01.azurewebsites.net/api/products/all";

        public ProductService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler);
        }

        public async Task<List<Producto>> GetProductosAsync()
        {
            var response = await _httpClient.GetAsync(_adminUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Producto>>() ?? new List<Producto>();
        }

        public async Task<Producto> AddProductoAsync(Producto producto)
        {
            var response = await _httpClient.PostAsJsonAsync(_baseUrl, producto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Producto>();
        }

        public async Task UpdateProductoAsync(Guid id, Producto producto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{id}", producto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteProductoAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}