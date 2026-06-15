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

        // Formato de localización por defecto para los campos en Contentful
        private readonly string _defaultLocale = "en-US";

        public ContentfulUploadService()
        {
            var httpClient = new HttpClient();

            // Personal Access Token
            string cmaToken = "CFPAT-BfAk3mF-k9_vB4QgLyn0nrzKKc8kJ4XY7OanuDrlg6k";
            // ID del espacio en Contentful
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
                string nuevoIdAsset = "img" + Guid.NewGuid().ToString("N");

                var asset = new ManagementAsset
                {
                    SystemProperties = new SystemProperties { Id = nuevoIdAsset },
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

                byte[] fileBytes = System.IO.File.ReadAllBytes(rutaLocalArchivo);
                var createdAsset = await _managementClient.UploadFileAndCreateAsset(asset, fileBytes);
                await Task.Delay(2000);


                var assetA_Publicar = await _managementClient.GetAsset(nuevoIdAsset);
                var versionActual = assetA_Publicar.SystemProperties.Version ?? 1;
                var publishedAsset = await _managementClient.PublishAsset(nuevoIdAsset, versionActual);
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