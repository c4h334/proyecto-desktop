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
    public sealed partial class ClientesPage : Page
    {
        private ObservableCollection<Cliente> clientes = new();
        private List<Cliente> _todosLosClientes = new();
        private readonly CustomerService _customerService = new();

        public ClientesPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            ClientesListView.ItemsSource = clientes;
            this.Loaded += ClientesPage_Loaded;
        }

        private async void ClientesPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (clientes.Count == 0) await CargarClientesDesdeApi();
        }

        private async Task CargarClientesDesdeApi()
        {
            try
            {
                var listaApi = await _customerService.GetClientesAsync();
                DispatcherQueue.TryEnqueue(() =>
                {
                    clientes.Clear();
                    _todosLosClientes.Clear();
                    foreach (var c in listaApi) { clientes.Add(c); _todosLosClientes.Add(c); }
                });
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error Clientes: {ex.Message}"); }
        }

        private void BuscarClienteBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string filtro = sender.Text.Trim().ToLower();
                clientes.Clear();

                if (string.IsNullOrWhiteSpace(filtro))
                {
                    foreach (var c in _todosLosClientes) clientes.Add(c);
                }
                else
                {
                    var filtrados = _todosLosClientes.Where(c =>
                        !string.IsNullOrEmpty(c.Identificacion) && c.Identificacion.ToLower().Contains(filtro)).ToList();
                    foreach (var c in filtrados) clientes.Add(c);
                }
            }
        }

        private async void AgregarCliente_Click(object sender, RoutedEventArgs e) => await MostrarDialogoCliente(null);

        private async void ModificarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (ClientesListView.SelectedItem is Cliente cliente)
            {
                await MostrarDialogoCliente(cliente);
            }
        }

        private async void EliminarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (ClientesListView.SelectedItem is Cliente cliente)
            {
                ContentDialog dialog = new()
                {
                    Title = "Eliminar cliente",
                    Content = $"¿Desea eliminar a '{cliente.Nombre}'?",
                    PrimaryButtonText = "Eliminar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = ElementTheme.Light
                };

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    if (cliente.CustomerResourceId.HasValue)
                        await _customerService.DeleteClienteAsync(cliente.CustomerResourceId.Value);

                    await CargarClientesDesdeApi();
                }
            }
        }

        private async Task MostrarDialogoCliente(Cliente? clienteExistente)
        {
            bool esEdicion = clienteExistente != null;
            TextBox txtNombre = new() { Header = "Nombre Completo", Text = clienteExistente?.Nombre ?? "" };
            TextBox txtIdentificacion = new() { Header = "Identificación", Text = clienteExistente?.Identificacion ?? "" };
            TextBox txtTel = new() { Header = "Tel", Text = clienteExistente?.Tel ?? "" };
            TextBox txtDireccion = new() { Header = "Dirección", Text = clienteExistente?.DireccionCasa ?? "" };
            TextBox txtCorreo = new() { Header = "Correo", Text = clienteExistente?.Correo ?? "" };

            StackPanel panel = new() { Spacing = 10 };
            panel.Children.Add(txtNombre); panel.Children.Add(txtIdentificacion);
            panel.Children.Add(txtTel); panel.Children.Add(txtDireccion); panel.Children.Add(txtCorreo);

            ContentDialog dialog = new()
            {
                Title = esEdicion ? "Modificar cliente" : "Agregar cliente",
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Light
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if (esEdicion)
                {
                    clienteExistente!.Nombre = txtNombre.Text; clienteExistente.Identificacion = txtIdentificacion.Text;
                    clienteExistente.Tel = txtTel.Text; clienteExistente.DireccionCasa = txtDireccion.Text;
                    clienteExistente.Correo = txtCorreo.Text;

                    if (clienteExistente.CustomerResourceId.HasValue)
                        await _customerService.UpdateClienteAsync(clienteExistente.CustomerResourceId.Value, clienteExistente);
                }
                else
                {
                    var nuevoCliente = new Cliente
                    {
                        CustomerResourceId = Guid.NewGuid(),
                        Nombre = txtNombre.Text,
                        Identificacion = txtIdentificacion.Text,
                        Tel = txtTel.Text,
                        DireccionCasa = txtDireccion.Text,
                        Correo = txtCorreo.Text
                    };
                    await _customerService.AddClienteAsync(nuevoCliente);
                }

                await CargarClientesDesdeApi();
            }
        }
    }
}