using Microsoft.Data.SqlClient;
using proyecto_desktop.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace proyecto_desktop.Services
{
    public class UserService
    {
        // Conexión al contenedor Docker de SQL Server publicado en el puerto 1434 local
        private readonly string _connectionString = "Server=127.0.0.1,1434;Database=RaulVega;User Id=sa;Password=MiClaveSegura123*;TrustServerCertificate=True;Connection Timeout=5;";

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            var list = new List<Usuario>();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    
                    // STRING_AGG agrupa los nombres de roles en una cadena separada por comas
                    string query = @"
                        SELECT u.UserResourceId, u.Name, u.Username, u.Email, 
                               COALESCE(STRING_AGG(r.Name, ', '), '') AS Roles
                        FROM Users u
                        LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
                        LEFT JOIN Roles r ON ur.RoleId = r.RoleId
                        GROUP BY u.UserId, u.UserResourceId, u.Name, u.Username, u.Email";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var user = new Usuario
                            {
                                UserResourceId = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0),
                                Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                Username = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                Roles = reader.IsDBNull(4) ? "" : reader.GetString(4)
                            };
                            list.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR CONECTANDO A BASE DE DATOS DOCKER: {ex.Message}");
                // Fallback automático si no se puede conectar
                list = GetMockUsuarios();
            }

            return list;
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
                    Roles = "Administrator, Support"
                },
                new Usuario
                {
                    UserResourceId = Guid.NewGuid(),
                    Name = "Ana Gómez",
                    Username = "ana.gomez",
                    Email = "ana.gomez@empresa.com",
                    Roles = "Support"
                },
                new Usuario
                {
                    UserResourceId = Guid.NewGuid(),
                    Name = "Luis Mora",
                    Username = "luis.mora",
                    Email = "luis.mora@cliente.com",
                    Roles = "Customer"
                },
                new Usuario
                {
                    UserResourceId = Guid.NewGuid(),
                    Name = "María Salas",
                    Username = "maria.salas",
                    Email = "maria.salas@cliente.com",
                    Roles = "Customer"
                }
            };
        }
    }
}
