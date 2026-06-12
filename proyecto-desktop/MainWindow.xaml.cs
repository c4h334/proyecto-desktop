using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using proyecto_desktop.Models;
using proyecto_desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

namespace proyecto_desktop
{
    public sealed partial class MainWindow : Window
    {
        private ObservableCollection<Producto> productos = new();
        private List<Producto> _todosLosProductos = new();

        private ObservableCollection<Usuario> usuarios = new();
        private List<Usuario> _todosLosUsuarios = new();

        private ObservableCollection<Proveedor> proveedores = new();
        private List<Proveedor> _todosLosProveedores = new();

        // Servicios para cada entidad
        private readonly ProductService _productService = new();
        private readonly UserService _userService = new();
        private readonly SupplierService _supplierService = new();

        // NUEVO: Servicio para subir imágenes a Contentful
        private readonly ContentfulUploadService _contentfulUploadService = new();

        private AppWindow m_AppWindow;

        public MainWindow()
        {
            this.InitializeComponent();
            m_AppWindow = this.AppWindow;

            m_AppWindow.SetIcon("Assets/raul-vega-logo.ico");

            // Tema claro
            if (this.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ElementTheme.Light;
            }

            // Fondo Mica
            TrySetMicaBackdrop();

            // Asignar ItemsSource a los ListView
            ProductosListView.ItemsSource = productos;
            UsuariosListView.ItemsSource = usuarios;
            ProveedoresListView.ItemsSource = proveedores;

            // Cargar TODAS las listas desde la API
            _ = CargarDatosInicialesAsync();

            // Selección inicial
            TopNavView.SelectedItem = TopNavView.MenuItems[0];
        }

        private async Task CargarDatosInicialesAsync()
        {
            await CargarProductosDesdeApi();
            await CargarUsuariosDesdeDb();
            await CargarProveedoresDesdeApi();
        }

        private bool TrySetMicaBackdrop()
        {
            try
            {
                this.SystemBackdrop = new MicaBackdrop();
                return true;
            }
            catch { return false; }
        }

        private void TopNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer == null) return;
            string tag = args.SelectedItemContainer.Tag?.ToString() ?? "";

            VistaProductos.Visibility = tag == "Productos" ? Visibility.Visible : Visibility.Collapsed;
            VistaUsuarios.Visibility = tag == "Usuarios" ? Visibility.Visible : Visibility.Collapsed;
            VistaProveedores.Visibility = tag == "Proveedores" ? Visibility.Visible : Visibility.Collapsed;
        }

        // =========================================================
        // PRODUCTOS
        // =========================================================

        private async Task CargarProductosDesdeApi()
        {
            try
            {
                var listaApi = await _productService.GetProductosAsync();
                productos.Clear();
                foreach (var p in listaApi) productos.Add(p);

                // Respaldo para la búsqueda
                _todosLosProductos.Clear();
                _todosLosProductos.AddRange(productos);
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
                        !string.IsNullOrEmpty(p.Codigo) &&
                        p.Codigo.ToLower().Contains(filtro)).ToList();

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
                RefrescarLista(ProductosListView, productos);
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
                    try
                    {
                        if (producto.ProductResourceId.HasValue)
                            await _productService.DeleteProductoAsync(producto.ProductResourceId.Value);

                        productos.Remove(producto);
                        _todosLosProductos.Remove(producto);
                    }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error al eliminar: {ex.Message}"); }
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

            // NUEVO: Controles para subir imagen
            Button btnBuscarImagen = new() { Content = "Seleccionar Imagen Local", Margin = new Thickness(0, 10, 0, 0) };
            TextBlock txtRutaImagen = new()
            {
                Text = !string.IsNullOrEmpty(productoExistente?.Image) ? "Imagen actual: " + productoExistente.Image : "Ninguna imagen seleccionada",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12
            };
            string rutaImagenLocalSeleccionada = "";

            // Vista previa de imagen
            Image imgPreview = new()
            {
                Width = 200,
                Height = 150,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            if (!string.IsNullOrEmpty(productoExistente?.Image))
            {
                try
                {
                    imgPreview.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(productoExistente.Image));
                }
                catch { }
            }

            // NUEVO: Evento para abrir el explorador de archivos de Windows
            btnBuscarImagen.Click += async (s, e) =>
            {
                var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

                openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".png");

                var file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    rutaImagenLocalSeleccionada = file.Path;
                    txtRutaImagen.Text = "Seleccionada: " + file.Name;

                    try
                    {
                        var bitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                        using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                        {
                            await bitmap.SetSourceAsync(stream);
                        }
                        imgPreview.Source = bitmap;
                    }
                    catch { }
                }
            };

            StackPanel panel = new() { Spacing = 20 };
            panel.Children.Add(txtNombre); panel.Children.Add(txtDescripcion); panel.Children.Add(nbCantidad);
            panel.Children.Add(nbPrecio); panel.Children.Add(txtCodigo); panel.Children.Add(chkDisponible);
            panel.Children.Add(nbDescuento); panel.Children.Add(nbCantDescuento); panel.Children.Add(txtMaterial);

            // Agregamos los controles de la imagen al panel
            panel.Children.Add(btnBuscarImagen);
            panel.Children.Add(txtRutaImagen);
            panel.Children.Add(imgPreview);

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
                try
                {
                    // NUEVO: Mantener la imagen original por defecto
                    string urlImagenFinal = productoExistente?.Image ?? "";

                    // NUEVO: Si el usuario eligió una nueva imagen local, se sube a Contentful antes de llamar a la API
                    if (!string.IsNullOrEmpty(rutaImagenLocalSeleccionada))
                    {
                        string urlSubida = await _contentfulUploadService.SubirImagenLocalAsync(rutaImagenLocalSeleccionada);
                        if (!string.IsNullOrEmpty(urlSubida))
                        {
                            urlImagenFinal = urlSubida;
                        }
                    }

                    if (esEdicion)
                    {
                        productoExistente!.Nombre = txtNombre.Text;
                        productoExistente.Descripcion = txtDescripcion.Text;
                        productoExistente.Cantidad = (int)nbCantidad.Value;
                        productoExistente.Precio = (decimal)nbPrecio.Value;
                        productoExistente.Codigo = txtCodigo.Text;
                        productoExistente.Disponible = chkDisponible.IsChecked ?? false;
                        productoExistente.Descuento = (decimal)nbDescuento.Value;
                        productoExistente.CantDescuento = (int)nbCantDescuento.Value;
                        productoExistente.Material = txtMaterial.Text;
                        productoExistente.Image = urlImagenFinal; // NUEVO: Guardamos la URL

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
                            Image = urlImagenFinal // NUEVO: Guardamos la URL
                        };
                        var productoCreado = await _productService.AddProductoAsync(nuevoProducto);
                        var pFinal = productoCreado ?? nuevoProducto;

                        productos.Add(pFinal);
                        _todosLosProductos.Add(pFinal);
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error al guardar: {ex.Message}"); }
            }
        }

        // =========================================================
        // USUARIOS Y ROLES
        // =========================================================

        private async Task CargarUsuariosDesdeDb()
        {
            try
            {
                var listaDb = await _userService.GetUsuariosAsync();
                usuarios.Clear();
                foreach (var u in listaDb) usuarios.Add(u);

                // Respaldo para la búsqueda
                _todosLosUsuarios.Clear();
                _todosLosUsuarios.AddRange(usuarios);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error Usuarios: {ex.Message}"); }
        }

        private void BuscarUsuarioBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string filtro = sender.Text.Trim().ToLower();
                usuarios.Clear();

                if (string.IsNullOrWhiteSpace(filtro))
                {
                    foreach (var u in _todosLosUsuarios) usuarios.Add(u);
                }
                else
                {
                    var filtrados = _todosLosUsuarios.Where(u =>
                        (!string.IsNullOrEmpty(u.Username) && u.Username.ToLower().Contains(filtro)) ||
                        (!string.IsNullOrEmpty(u.Name) && u.Name.ToLower().Contains(filtro))).ToList();

                    foreach (var u in filtrados) usuarios.Add(u);
                }
            }
        }

        // =========================================================
        // PROVEEDORES
        // =========================================================

        private async Task CargarProveedoresDesdeApi()
        {
            try
            {
                var listaApi = await _supplierService.GetProveedoresAsync();
                proveedores.Clear();
                foreach (var p in listaApi) proveedores.Add(p);

                // Respaldo para la búsqueda
                _todosLosProveedores.Clear();
                _todosLosProveedores.AddRange(proveedores);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error Proveedores: {ex.Message}"); }
        }

        private void BuscarProveedorBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string filtro = sender.Text.Trim().ToLower();
                proveedores.Clear();

                if (string.IsNullOrWhiteSpace(filtro))
                {
                    foreach (var p in _todosLosProveedores) proveedores.Add(p);
                }
                else
                {
                    var filtrados = _todosLosProveedores.Where(p =>
                        !string.IsNullOrEmpty(p.Identificacion) &&
                        p.Identificacion.ToLower().Contains(filtro)).ToList();

                    foreach (var p in filtrados) proveedores.Add(p);
                }
            }
        }

        private async void AgregarProveedor_Click(object sender, RoutedEventArgs e) => await MostrarDialogoProveedor(null);

        private async void ModificarProveedor_Click(object sender, RoutedEventArgs e)
        {
            if (ProveedoresListView.SelectedItem is Proveedor proveedor)
            {
                await MostrarDialogoProveedor(proveedor);
                RefrescarLista(ProveedoresListView, proveedores);
            }
        }

        private async void EliminarProveedor_Click(object sender, RoutedEventArgs e)
        {
            if (ProveedoresListView.SelectedItem is Proveedor proveedor)
            {
                ContentDialog dialog = new()
                {
                    Title = "Eliminar proveedor",
                    Content = $"¿Desea eliminar a '{proveedor.Nombre}'?",
                    PrimaryButtonText = "Eliminar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = ElementTheme.Light
                };

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    try
                    {
                        if (proveedor.SupplierResourceId.HasValue)
                            await _supplierService.DeleteProveedorAsync(proveedor.SupplierResourceId.Value);

                        proveedores.Remove(proveedor);
                        _todosLosProveedores.Remove(proveedor);
                    }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error al eliminar: {ex.Message}"); }
                }
            }
        }

        private async Task MostrarDialogoProveedor(Proveedor? proveedorExistente)
        {
            bool esEdicion = proveedorExistente != null;
            TextBox txtNombre = new() { Header = "Nombre / Empresa", Text = proveedorExistente?.Nombre ?? "" };
            TextBox txtIdentificacion = new() { Header = "Cédula Jurídica", Text = proveedorExistente?.Identificacion ?? "" };
            TextBox txtTel = new() { Header = "Teléfono", Text = proveedorExistente?.Tel ?? "" };
            TextBox txtDireccion = new() { Header = "Ubicación", Text = proveedorExistente?.Direccion ?? "" };
            TextBox txtCorreo = new() { Header = "Correo", Text = proveedorExistente?.Correo ?? "" };
            TextBox txtListaProd = new() { Header = "Lista Productos", Text = proveedorExistente?.ProductList ?? "" };

            StackPanel panel = new() { Spacing = 10 };
            panel.Children.Add(txtNombre); panel.Children.Add(txtIdentificacion); panel.Children.Add(txtTel);
            panel.Children.Add(txtDireccion); panel.Children.Add(txtCorreo); panel.Children.Add(txtListaProd);

            ContentDialog dialog = new()
            {
                Title = esEdicion ? "Modificar proveedor" : "Agregar proveedor",
                Content = new ScrollViewer { Content = panel, Height = 500 },
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Light
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                try
                {
                    if (esEdicion)
                    {
                        proveedorExistente!.Nombre = txtNombre.Text; proveedorExistente.Identificacion = txtIdentificacion.Text;
                        proveedorExistente.Tel = txtTel.Text; proveedorExistente.Direccion = txtDireccion.Text;
                        proveedorExistente.Correo = txtCorreo.Text; proveedorExistente.ProductList = txtListaProd.Text;

                        if (proveedorExistente.SupplierResourceId.HasValue)
                            await _supplierService.UpdateProveedorAsync(proveedorExistente.SupplierResourceId.Value, proveedorExistente);
                    }
                    else
                    {
                        var nuevoProveedor = new Proveedor
                        {
                            SupplierResourceId = Guid.NewGuid(),
                            Nombre = txtNombre.Text,
                            Identificacion = txtIdentificacion.Text,
                            Tel = txtTel.Text,
                            Direccion = txtDireccion.Text,
                            Correo = txtCorreo.Text,
                            ProductList = txtListaProd.Text
                        };
                        var proveedorCreado = await _supplierService.AddProveedorAsync(nuevoProveedor);
                        var pFinal = proveedorCreado ?? nuevoProveedor;

                        proveedores.Add(pFinal);
                        _todosLosProveedores.Add(pFinal);
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error al guardar proveedor: {ex.Message}"); }
            }
        }

        // Refrescar genérico
        private void RefrescarLista(ListView list, object dataSource)
        {
            var seleccion = list.SelectedItem;
            list.ItemsSource = null;
            list.ItemsSource = dataSource;
            list.SelectedItem = seleccion;
        }
    }
}