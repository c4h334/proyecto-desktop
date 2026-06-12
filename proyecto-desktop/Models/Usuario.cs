using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace proyecto_desktop.Models
{
    public class Usuario
    {
        [JsonPropertyName("userResourceId")]
        public Guid? UserResourceId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("username")]
        public string Username { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("roles")]
        public List<string> RolesList { get; set; } = [];

        [JsonIgnore]
        public string Roles => string.Join(", ", RolesList);
    }
}
