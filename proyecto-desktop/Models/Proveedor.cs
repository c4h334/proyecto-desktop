using System;
using System.Text.Json.Serialization;

namespace proyecto_desktop.Models
{
    public class Proveedor
    {
        [JsonPropertyName("supplierResourceId")]
        public Guid? SupplierResourceId { get; set; }

        [JsonPropertyName("companyName")]
        public string Nombre { get; set; } = "";

        [JsonIgnore]
        public string Apellidos { get; set; } = "";

        [JsonPropertyName("legalId")]
        public string Identificacion { get; set; } = "";

        [JsonPropertyName("location")]
        public string Direccion { get; set; } = "";

        [JsonPropertyName("phone")]
        public string Tel { get; set; } = "";

        [JsonPropertyName("email")]
        public string Correo { get; set; } = "";

        [JsonPropertyName("productList")]
        public string ProductList { get; set; } = "";
    }
}