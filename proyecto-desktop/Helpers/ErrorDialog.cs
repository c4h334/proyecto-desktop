using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace proyecto_desktop.Helpers
{
    public static class ErrorDialog
    {
        /// <summary>
        /// Muestra un ContentDialog con un mensaje de error conciso según el tipo de excepción.
        /// </summary>
        public static async Task MostrarAsync(Exception ex, XamlRoot xamlRoot, string contexto = "")
        {
            string titulo;
            string mensaje;

            if (ex is HttpRequestException httpEx)
            {
                titulo = "Error de conexión";
                int? codigo = (int?)httpEx.StatusCode;

                mensaje = codigo switch
                {
                    400 => "La solicitud enviada no es válida. Verifique los datos ingresados.",
                    401 => "No autorizado. Su sesión puede haber expirado.",
                    403 => "Acceso denegado. No tiene permisos para realizar esta acción.",
                    404 => "El recurso solicitado no fue encontrado en el servidor.",
                    409 => "Conflicto: el registro ya existe o viola una restricción de unicidad.",
                    500 => "Error interno del servidor. Intente nuevamente en unos momentos.",
                    503 => "El servidor no está disponible. Intente más tarde.",
                    null => "No se pudo conectar con el servidor. Verifique su conexión a internet.",
                    _   => $"El servidor respondió con un error (código {codigo})."
                };
            }
            else if (ex is TaskCanceledException || ex is TimeoutException)
            {
                titulo = "Tiempo de espera agotado";
                mensaje = "La solicitud tardó demasiado. Verifique su conexión e intente de nuevo.";
            }
            else if (ex is InvalidOperationException)
            {
                titulo = "Operación inválida";
                mensaje = "Ocurrió un error inesperado al procesar la operación.";
            }
            else
            {
                titulo = "Error inesperado";
                mensaje = "Ocurrió un error que no pudo identificarse. Intente nuevamente.";
            }

            if (!string.IsNullOrWhiteSpace(contexto))
                mensaje = $"{mensaje}\n\nContexto: {contexto}";

            ContentDialog dialog = new()
            {
                Title = titulo,
                Content = mensaje,
                CloseButtonText = "Aceptar",
                XamlRoot = xamlRoot,
                RequestedTheme = ElementTheme.Light
            };

            await dialog.ShowAsync();
        }
    }
}
