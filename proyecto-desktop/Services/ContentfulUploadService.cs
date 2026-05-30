using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;

namespace proyecto_desktop.Services
{
    public class ContentfulUploadService
    {
        private readonly ContentfulManagementClient _managementClient;

        // Importante mantenerlo en "en-US" que es el default de tu Contentful
        private readonly string _defaultLocale = "en-US";

        public ContentfulUploadService()
        {
            var httpClient = new HttpClient();

            // Tu Personal Access Token (Management)
            string cmaToken = "CFPAT-BfAk3mF-k9_vB4QgLyn0nrzKKc8kJ4XY7OanuDrlg6k";
            string spaceId = "8rnsgo3hklme";

            _managementClient = new ContentfulManagementClient(httpClient, cmaToken, spaceId);
        }

        public async Task<string> SubirImagenLocalAsync(string rutaLocalArchivo)
        {
            if (string.IsNullOrEmpty(rutaLocalArchivo) || !System.IO.File.Exists(rutaLocalArchivo))
                return string.Empty;

            try
            {
                var nombreArchivo = Path.GetFileName(rutaLocalArchivo);
                string extension = Path.GetExtension(rutaLocalArchivo).ToLower();
                string mimeType = extension == ".png" ? "image/png" : "image/jpeg";

                // 1. SOLUCIÓN AL ERROR: Generamos un ID único que empiece con letras
                string nuevoIdAsset = "img" + Guid.NewGuid().ToString("N");

                // 2. Preparamos el Asset asignándole explícitamente el ID en SystemProperties
                var asset = new ManagementAsset
                {
                    SystemProperties = new SystemProperties { Id = nuevoIdAsset }, // <-- AQUÍ CORREGIMOS EL ERROR
                    Title = new Dictionary<string, string> { { _defaultLocale, nombreArchivo } },
                    Files = new Dictionary<string, Contentful.Core.Models.File>
                    {
                        {
                            _defaultLocale, new Contentful.Core.Models.File
                            {
                                FileName = nombreArchivo,
                                ContentType = mimeType
                            }
                        }
                    }
                };

                // 3. Leemos los bytes físicos de la imagen local
                byte[] fileBytes = System.IO.File.ReadAllBytes(rutaLocalArchivo);

                // 4. Sube el binario y crea el Asset
                var createdAsset = await _managementClient.UploadFileAndCreateAsset(asset, fileBytes);

                // Le damos 2 segundos a los servidores de Contentful para procesar la imagen internamente
                await Task.Delay(2000);

                // 5. Obtenemos el asset refrescado para saber su versión exacta y publicarlo sin errores
                var assetA_Publicar = await _managementClient.GetAsset(nuevoIdAsset);
                var versionActual = assetA_Publicar.SystemProperties.Version ?? 1;

                var publishedAsset = await _managementClient.PublishAsset(nuevoIdAsset, versionActual);

                // 6. Extraer la URL pública resultante
                string urlFinal = publishedAsset.Files[_defaultLocale].Url;

                return urlFinal.StartsWith("http") ? urlFinal : $"https:{urlFinal}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error subiendo a Contentful: {ex.Message}");
                return string.Empty;
            }
        }
    }
}