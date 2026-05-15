using System;
using System.Text.Json.Serialization;

namespace proyecto_desktop.Models
{
    public class Cliente
    {
        [JsonPropertyName("customerResourceId")]
        public Guid? CustomerResourceId { get; set; }

        [JsonPropertyName("fullName")]
        public string Nombre { get; set; } = ""; // Guarda el nombre completo

        [JsonIgnore]
        public string Apellidos { get; set; } = ""; // Ignorado para la API, para no romper XAML

        [JsonPropertyName("identification")]
        public string Identificacion { get; set; } = "";

        [JsonPropertyName("phone")]
        public string Tel { get; set; } = "";

        [JsonPropertyName("homeAddress")]
        public string DireccionCasa { get; set; } = "";

        [JsonPropertyName("email")]
        public string Correo { get; set; } = "";
    }
}