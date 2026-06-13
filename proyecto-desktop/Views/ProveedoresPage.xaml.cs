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
    public sealed partial class ProveedoresPage : Page
    {
        private ObservableCollection<Proveedor> proveedores = new();
        private List<Proveedor> _todosLosProveedores = new();
        private readonly SupplierService _supplierService = new();

        public ProveedoresPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            ProveedoresListView.ItemsSource = proveedores;
            this.Loaded += ProveedoresPage_Loaded;
        }

        private async void ProveedoresPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (proveedores.Count == 0) await CargarProveedoresDesdeApi();
        }

        private async Task CargarProveedoresDesdeApi()
        {
            try
            {
                var listaApi = await _supplierService.GetProveedoresAsync();
                DispatcherQueue.TryEnqueue(() =>
                {
                    proveedores.Clear();
                    _todosLosProveedores.Clear();
                    foreach (var p in listaApi) { proveedores.Add(p); _todosLosProveedores.Add(p); }
                });
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
                        !string.IsNullOrEmpty(p.Identificacion) && p.Identificacion.ToLower().Contains(filtro)).ToList();
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
                    if (proveedor.SupplierResourceId.HasValue)
                        await _supplierService.DeleteProveedorAsync(proveedor.SupplierResourceId.Value);

                    await CargarProveedoresDesdeApi();
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
                    await _supplierService.AddProveedorAsync(nuevoProveedor);
                }

                await CargarProveedoresDesdeApi();
            }
        }
    }
}