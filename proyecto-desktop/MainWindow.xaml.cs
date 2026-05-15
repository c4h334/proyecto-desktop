using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using proyecto_desktop.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using proyecto_desktop.Services;

namespace proyecto_desktop
{
    public sealed partial class MainWindow : Window
    {
        private ObservableCollection<Producto> productos = new();
        private readonly ProductService _productService = new();
        private ObservableCollection<Cliente> clientes = new();
        private ObservableCollection<Proveedor> proveedores = new();

        public MainWindow()
        {
            this.InitializeComponent();

            // Tema claro
            if (this.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ElementTheme.Light;
            }

            // Fondo Mica
            TrySetMicaBackdrop();

            ProductosListView.ItemsSource = productos;

            _ = CargarProductosDesdeApi();

            // Asignar ItemsSource a los ListView
            ProductosListView.ItemsSource = productos;
            ClientesListView.ItemsSource = clientes;
            ProveedoresListView.ItemsSource = proveedores;

            // Selección inicial
            TopNavView.SelectedItem = TopNavView.MenuItems[0];
        }

        private bool TrySetMicaBackdrop()
        {
            try
            {
                this.SystemBackdrop = new MicaBackdrop();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void TopNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer == null)
                return;

            string tag = args.SelectedItemContainer.Tag?.ToString() ?? "";

            VistaProductos.Visibility = tag == "Productos" ? Visibility.Visible : Visibility.Collapsed;
            VistaClientes.Visibility = tag == "Clientes" ? Visibility.Visible : Visibility.Collapsed;
            VistaProveedores.Visibility = tag == "Proveedores" ? Visibility.Visible : Visibility.Collapsed;
        }

        // =========================================================
        // PRODUCTOS
        // =========================================================

        private async void AgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            await MostrarDialogoProducto(null);
        }

        private async void ModificarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (ProductosListView.SelectedItem is Producto producto)
            {
                await MostrarDialogoProducto(producto);
                RefrescarProductos();
            }
        }

        private async void EliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (ProductosListView.SelectedItem is Producto producto)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Eliminar producto",
                    Content = $"¿Desea eliminar el producto '{producto.Nombre}'?",
                    PrimaryButtonText = "Eliminar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.Content.XamlRoot
                };

                ContentDialogResult result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        // 1. Eliminar en el Backend (API)
                        if (producto.ProductResourceId.HasValue)
                        {
                            await _productService.DeleteProductoAsync(producto.ProductResourceId.Value);
                        }

                        // 2. Eliminar de la lista local
                        productos.Remove(producto);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al eliminar: {ex.Message}");
                    }
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

            StackPanel panel = new()
            {
                Spacing = 20
            };

            panel.Children.Add(txtNombre);
            panel.Children.Add(txtDescripcion);
            panel.Children.Add(nbCantidad);
            panel.Children.Add(nbPrecio);
            panel.Children.Add(txtCodigo);
            panel.Children.Add(chkDisponible);
            panel.Children.Add(nbDescuento);
            panel.Children.Add(nbCantDescuento);
            panel.Children.Add(txtMaterial);

            ContentDialog dialog = new()
            {
                Title = esEdicion ? "Modificar producto" : "Agregar producto",
                Content = new ScrollViewer
                {
                    Content = panel,
                    Height = 500
                },
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    if (esEdicion)
                    {
                        // 1. Actualizar objeto localmente
                        productoExistente!.Nombre = txtNombre.Text;
                        productoExistente.Descripcion = txtDescripcion.Text;
                        productoExistente.Cantidad = (int)nbCantidad.Value;
                        productoExistente.Precio = (decimal)nbPrecio.Value;
                        productoExistente.Codigo = txtCodigo.Text;
                        productoExistente.Disponible = chkDisponible.IsChecked ?? false;
                        productoExistente.Descuento = (decimal)nbDescuento.Value;
                        productoExistente.CantDescuento = (int)nbCantDescuento.Value;
                        productoExistente.Material = txtMaterial.Text;

                        // 2. Enviar actualización al Backend (API)
                        if (productoExistente.ProductResourceId.HasValue)
                        {
                            await _productService.UpdateProductoAsync(productoExistente.ProductResourceId.Value, productoExistente);
                        }
                    }
                    else
                    {
                        // 1. Crear nuevo objeto
                        var nuevoProducto = new Producto
                        {
                            ProductResourceId = Guid.NewGuid(), // Se genera el ID para la base de datos
                            Nombre = txtNombre.Text,
                            Descripcion = txtDescripcion.Text,
                            Cantidad = (int)nbCantidad.Value,
                            Precio = (decimal)nbPrecio.Value,
                            Codigo = txtCodigo.Text,
                            Disponible = chkDisponible.IsChecked ?? false,
                            Descuento = (decimal)nbDescuento.Value,
                            CantDescuento = (int)nbCantDescuento.Value,
                            Material = txtMaterial.Text
                        };

                        // 2. Enviar al Backend (API)
                        var productoCreado = await _productService.AddProductoAsync(nuevoProducto);

                        // 3. Reflejar en la lista local
                        productos.Add(productoCreado ?? nuevoProducto);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al guardar: {ex.Message}");
                }
            }
        }

        private void RefrescarProductos()
        {
            var seleccion = ProductosListView.SelectedItem;
            ProductosListView.ItemsSource = null;
            ProductosListView.ItemsSource = productos;
            ProductosListView.SelectedItem = seleccion;
        }

        // =========================================================
        // CLIENTES
        // =========================================================

        private async void AgregarCliente_Click(object sender, RoutedEventArgs e)
        {
            TextBox txtNombre = new TextBox { Header = "Nombre", Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0) };
            TextBox txtDesc = new TextBox { Header = "Descripción", AcceptsReturn = true, Height = 100 };
            NumberBox nbPrecio = new NumberBox { Header = "Precio", Value = 0 };

            StackPanel panel = new StackPanel
            {
                Spacing = 10,
                Width = 500
            };
            panel.Children.Add(txtNombre);
            panel.Children.Add(txtDesc);
            panel.Children.Add(nbPrecio);

            ContentDialog dialogo = new ContentDialog
            {
                Title = "NUEVO CLIENTE",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot
            };

            dialogo.Resources["ContentDialogMaxWidth"] = 600.0;

            await dialogo.ShowAsync();
        }

        private async void ModificarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (ClientesListView.SelectedItem is Cliente cliente)
            {
                await MostrarDialogoCliente(cliente);
                RefrescarClientes();
            }
        }

        private async void EliminarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (ClientesListView.SelectedItem is Cliente cliente)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Eliminar cliente",
                    Content = $"¿Desea eliminar a '{cliente.Nombre} {cliente.Apellidos}'?",
                    PrimaryButtonText = "Eliminar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.Content.XamlRoot
                };

                ContentDialogResult result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    clientes.Remove(cliente);
                }
            }
        }

        private async Task MostrarDialogoCliente(Cliente? clienteExistente)
        {
            bool esEdicion = clienteExistente != null;

            TextBox txtNombre = new() { Header = "Nombre", Text = clienteExistente?.Nombre ?? "" };
            TextBox txtApellidos = new() { Header = "Apellidos", Text = clienteExistente?.Apellidos ?? "" };
            TextBox txtIdentificacion = new() { Header = "Identificación", Text = clienteExistente?.Identificacion ?? "" };
            TextBox txtTel = new() { Header = "Tel", Text = clienteExistente?.Tel ?? "" };
            TextBox txtDireccion = new() { Header = "Dirección casa", Text = clienteExistente?.DireccionCasa ?? "" };
            TextBox txtCorreo = new() { Header = "Correo", Text = clienteExistente?.Correo ?? "" };

            StackPanel panel = new()
            {
                Spacing = 10
            };

            panel.Children.Add(txtNombre);
            panel.Children.Add(txtApellidos);
            panel.Children.Add(txtIdentificacion);
            panel.Children.Add(txtTel);
            panel.Children.Add(txtDireccion);
            panel.Children.Add(txtCorreo);

            ContentDialog dialog = new()
            {
                Title = esEdicion ? "Modificar cliente" : "Agregar cliente",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (esEdicion)
                {
                    clienteExistente!.Nombre = txtNombre.Text;
                    clienteExistente.Apellidos = txtApellidos.Text;
                    clienteExistente.Identificacion = txtIdentificacion.Text;
                    clienteExistente.Tel = txtTel.Text;
                    clienteExistente.DireccionCasa = txtDireccion.Text;
                    clienteExistente.Correo = txtCorreo.Text;
                }
                else
                {
                    clientes.Add(new Cliente
                    {
                        Nombre = txtNombre.Text,
                        Apellidos = txtApellidos.Text,
                        Identificacion = txtIdentificacion.Text,
                        Tel = txtTel.Text,
                        DireccionCasa = txtDireccion.Text,
                        Correo = txtCorreo.Text
                    });
                }
            }
        }

        private void RefrescarClientes()
        {
            var seleccion = ClientesListView.SelectedItem;
            ClientesListView.ItemsSource = null;
            ClientesListView.ItemsSource = clientes;
            ClientesListView.SelectedItem = seleccion;
        }

        // =========================================================
        // PROVEEDORES
        // =========================================================

        private async void AgregarProveedor_Click(object sender, RoutedEventArgs e)
        {
            await MostrarDialogoProveedor(null);
        }

        private async void ModificarProveedor_Click(object sender, RoutedEventArgs e)
        {
            if (ProveedoresListView.SelectedItem is Proveedor proveedor)
            {
                await MostrarDialogoProveedor(proveedor);
                RefrescarProveedores();
            }
        }

        private async void EliminarProveedor_Click(object sender, RoutedEventArgs e)
        {
            if (ProveedoresListView.SelectedItem is Proveedor proveedor)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Eliminar proveedor",
                    Content = $"¿Desea eliminar a '{proveedor.Nombre} {proveedor.Apellidos}'?",
                    PrimaryButtonText = "Eliminar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.Content.XamlRoot
                };

                ContentDialogResult result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    proveedores.Remove(proveedor);
                }
            }
        }

        private async Task MostrarDialogoProveedor(Proveedor? proveedorExistente)
        {
            bool esEdicion = proveedorExistente != null;

            TextBox txtNombre = new() { Header = "Nombre", Text = proveedorExistente?.Nombre ?? "" };
            TextBox txtApellidos = new() { Header = "Apellidos", Text = proveedorExistente?.Apellidos ?? "" };
            TextBox txtIdentificacion = new() { Header = "Identificación", Text = proveedorExistente?.Identificacion ?? "" };
            TextBox txtTel = new() { Header = "Tel", Text = proveedorExistente?.Tel ?? "" };
            TextBox txtDireccion = new() { Header = "Dirección casa", Text = proveedorExistente?.DireccionCasa ?? "" };
            TextBox txtCorreo = new() { Header = "Correo", Text = proveedorExistente?.Correo ?? "" };

            StackPanel panel = new()
            {
                Spacing = 10
            };

            panel.Children.Add(txtNombre);
            panel.Children.Add(txtApellidos);
            panel.Children.Add(txtIdentificacion);
            panel.Children.Add(txtTel);
            panel.Children.Add(txtDireccion);
            panel.Children.Add(txtCorreo);

            ContentDialog dialog = new()
            {
                Title = esEdicion ? "Modificar proveedor" : "Agregar proveedor",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (esEdicion)
                {
                    proveedorExistente!.Nombre = txtNombre.Text;
                    proveedorExistente.Apellidos = txtApellidos.Text;
                    proveedorExistente.Identificacion = txtIdentificacion.Text;
                    proveedorExistente.Tel = txtTel.Text;
                    proveedorExistente.DireccionCasa = txtDireccion.Text;
                    proveedorExistente.Correo = txtCorreo.Text;
                }
                else
                {
                    proveedores.Add(new Proveedor
                    {
                        Nombre = txtNombre.Text,
                        Apellidos = txtApellidos.Text,
                        Identificacion = txtIdentificacion.Text,
                        Tel = txtTel.Text,
                        DireccionCasa = txtDireccion.Text,
                        Correo = txtCorreo.Text
                    });
                }
            }
        }

        private void RefrescarProveedores()
        {
            var seleccion = ProveedoresListView.SelectedItem;
            ProveedoresListView.ItemsSource = null;
            ProveedoresListView.ItemsSource = proveedores;
            ProveedoresListView.SelectedItem = seleccion;
        }

        private async Task CargarProductosDesdeApi()
        {
            try
            {
                var listaApi = await _productService.GetProductosAsync();
                productos.Clear();
                foreach (var p in listaApi)
                {
                    productos.Add(p);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar: {ex.Message}");
            }
        }
    }
}