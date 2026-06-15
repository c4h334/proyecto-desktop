using System;
using System.Text.Json.Serialization;

namespace proyecto_desktop.Models
{
    public class Producto
    {
        [JsonPropertyName("productResourceId")]
        public Guid? ProductResourceId { get; set; }

        [JsonPropertyName("name")]
        public string Nombre { get; set; } = "";

        [JsonPropertyName("description")]
        public string Descripcion { get; set; } = "";

        [JsonPropertyName("quantity")]
        public int Cantidad { get; set; }

        [JsonPropertyName("price")]
        public decimal Precio { get; set; }

        [JsonPropertyName("code")]
        public string Codigo { get; set; } = "";

        [JsonPropertyName("image")]
        public string Image { get; set; } = "";

        [JsonPropertyName("available")]
        public bool Disponible { get; set; }

        [JsonPropertyName("discount")]
        public decimal Descuento { get; set; }

        [JsonPropertyName("discountQuantity")]
        public int CantDescuento { get; set; }

        [JsonPropertyName("material")]
        public string Material { get; set; } = "";
    }
}
