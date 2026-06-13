using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using proyecto_desktop.Models;
using proyecto_desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_desktop.Views
{
    public sealed partial class ProductosPage : Page
    {
        private ObservableCollection<Producto> productos = new();
        private List<Producto> _todosLosProductos = new();
        private readonly ProductService _productService = new();
        private readonly ContentfulUploadService _contentfulUploadService = new();

        public ProductosPage()
        {
            this.InitializeComponent();

            // LA MAGIA AQUÍ: Evita que la página se destruya al cambiar de pestaña
            this.NavigationCacheMode = NavigationCacheMode.Required;

            ProductosListView.ItemsSource = productos;
            this.Loaded += ProductosPage_Loaded;
        }

        private async void ProductosPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Solo carga de la API si la lista está vacía (la primera vez)
            if (productos.Count == 0)
            {
                await CargarProductosDesdeApi();
            }
        }

        private async Task CargarProductosDesdeApi()
        {
            try
            {
                var listaApi = await _productService.GetProductosAsync();
                DispatcherQueue.TryEnqueue(() =>
                {
                    productos.Clear();
                    _todosLosProductos.Clear();
                    foreach (var p in listaApi) { productos.Add(p); _todosLosProductos.Add(p); }
                });
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error Productos: {ex.Message}"); }
        }

        private void BuscarProductoBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string filtro = sender.Text.Trim().ToLower();
                productos.Clear();

                if (string.IsNullOrWhiteSpace(filtro))
                {
                    foreach (var p in _todosLosProductos) productos.Add(p);
                }
                else
                {
                    var filtrados = _todosLosProductos.Where(p =>
                        !string.IsNullOrEmpty(p.Codigo) && p.Codigo.ToLower().Contains(filtro)).ToList();
                    foreach (var p in filtrados) productos.Add(p);
                }
            }
        }

        private async void AgregarProducto_Click(object sender, RoutedEventArgs e) => await MostrarDialogoProducto(null);

        private async void ModificarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (ProductosListView.SelectedItem is Producto producto)
            {
                await MostrarDialogoProducto(producto);
            }
        }

        private async void EliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (ProductosListView.SelectedItem is Producto producto)
            {
                ContentDialog dialog = new()
                {
                    Title = "Eliminar producto",
                    Content = $"¿Desea eliminar el producto '{producto.Nombre}'?",
                    PrimaryButtonText = "Eliminar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = ElementTheme.Light
                };

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    if (producto.ProductResourceId.HasValue)
                        await _productService.DeleteProductoAsync(producto.ProductResourceId.Value);

                    await CargarProductosDesdeApi();
                }
            }
        }

        private async Task MostrarDialogoProducto(Producto? productoExistente)
        {
            bool esEdicion = productoExistente != null;
            TextBox txtNombre = new() { Header = "Nombre", Text = productoExistente?.Nombre ?? "" };
            TextBox txtDescripcion = new() { Header = "Descripción", Text = productoExistente?.Descripcion ?? "" };
            NumberBox nbCantidad = new() { Header = "Cantidad", Value = productoExistente?.Cantidad ?? 0 };
            NumberBox nbPrecio = new() { Header = "Precio", Value = (double)(productoExistente?.Precio ?? 0) };
            TextBox txtCodigo = new() { Header = "Código", Text = productoExistente?.Codigo ?? "" };
            CheckBox chkDisponible = new() { Content = "Disponible", IsChecked = productoExistente?.Disponible ?? false };
            NumberBox nbDescuento = new() { Header = "Descuento", Value = (double)(productoExistente?.Descuento ?? 0) };
            NumberBox nbCantDescuento = new() { Header = "CantDescuento", Value = productoExistente?.CantDescuento ?? 0 };
            TextBox txtMaterial = new() { Header = "Material", Text = productoExistente?.Material ?? "" };

            Button btnBuscarImagen = new() { Content = "Seleccionar Imagen Local", Margin = new Thickness(0, 10, 0, 0) };
            TextBlock txtRutaImagen = new()
            {
                Text = !string.IsNullOrEmpty(productoExistente?.Image) ? "Imagen actual: " + productoExistente.Image : "Ninguna imagen seleccionada",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12
            };
            string rutaImagenLocalSeleccionada = "";

            btnBuscarImagen.Click += async (s, e) =>
            {
                var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Instance);
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
                openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                openPicker.FileTypeFilter.Add(".jpg"); openPicker.FileTypeFilter.Add(".jpeg"); openPicker.FileTypeFilter.Add(".png");

                var file = await openPicker.PickSingleFileAsync();
                if (file != null) { rutaImagenLocalSeleccionada = file.Path; txtRutaImagen.Text = "Seleccionada: " + file.Name; }
            };

            StackPanel panel = new() { Spacing = 20 };
            panel.Children.Add(txtNombre); panel.Children.Add(txtDescripcion); panel.Children.Add(nbCantidad);
            panel.Children.Add(nbPrecio); panel.Children.Add(txtCodigo); panel.Children.Add(chkDisponible);
            panel.Children.Add(nbDescuento); panel.Children.Add(nbCantDescuento); panel.Children.Add(txtMaterial);
            panel.Children.Add(btnBuscarImagen); panel.Children.Add(txtRutaImagen);

            ContentDialog dialog = new()
            {
                Title = esEdicion ? "Modificar producto" : "Agregar producto",
                Content = new ScrollViewer { Content = panel, Height = 500 },
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Light
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                string urlImagenFinal = productoExistente?.Image ?? "";
                if (!string.IsNullOrEmpty(rutaImagenLocalSeleccionada))
                {
                    string urlSubida = await _contentfulUploadService.SubirImagenLocalAsync(rutaImagenLocalSeleccionada);
                    if (!string.IsNullOrEmpty(urlSubida)) urlImagenFinal = urlSubida;
                }

                if (esEdicion)
                {
                    productoExistente!.Nombre = txtNombre.Text; productoExistente.Descripcion = txtDescripcion.Text;
                    productoExistente.Cantidad = (int)nbCantidad.Value; productoExistente.Precio = (decimal)nbPrecio.Value;
                    productoExistente.Codigo = txtCodigo.Text; productoExistente.Disponible = chkDisponible.IsChecked ?? false;
                    productoExistente.Descuento = (decimal)nbDescuento.Value; productoExistente.CantDescuento = (int)nbCantDescuento.Value;
                    productoExistente.Material = txtMaterial.Text; productoExistente.Image = urlImagenFinal;

                    if (productoExistente.ProductResourceId.HasValue)
                        await _productService.UpdateProductoAsync(productoExistente.ProductResourceId.Value, productoExistente);
                }
                else
                {
                    var nuevoProducto = new Producto
                    {
                        ProductResourceId = Guid.NewGuid(),
                        Nombre = txtNombre.Text,
                        Descripcion = txtDescripcion.Text,
                        Cantidad = (int)nbCantidad.Value,
                        Precio = (decimal)nbPrecio.Value,
                        Codigo = txtCodigo.Text,
                        Disponible = chkDisponible.IsChecked ?? false,
                        Descuento = (decimal)nbDescuento.Value,
                        CantDescuento = (int)nbCantDescuento.Value,
                        Material = txtMaterial.Text,
                        Image = urlImagenFinal
                    };
                    await _productService.AddProductoAsync(nuevoProducto);
                }

                // Recargar lista para mostrar los cambios automáticamente
                await CargarProductosDesdeApi();
            }
        }
    }
}