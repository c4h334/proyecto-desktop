using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using proyecto_desktop.Helpers;
using proyecto_desktop.Models;
using proyecto_desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_desktop.Views
{
    public sealed partial class UsuariosPage : Page
    {
        private ObservableCollection<Usuario> usuarios = new();
        private List<Usuario> _todosLosUsuarios = new();
        private readonly UserService _userService = new();

        public UsuariosPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            UsuariosListView.ItemsSource = usuarios;
            this.Loaded += UsuariosPage_Loaded;
        }

        private async void UsuariosPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (usuarios.Count == 0) await CargarUsuariosDesdeApi();
        }

        private async Task CargarUsuariosDesdeApi()
        {
            try
            {
                var listaApi = await _userService.GetUsuariosAsync();
                DispatcherQueue.TryEnqueue(() =>
                {
                    usuarios.Clear();
                    _todosLosUsuarios.Clear();
                    foreach (var u in listaApi) { usuarios.Add(u); _todosLosUsuarios.Add(u); }
                });
            }
            catch (Exception ex)
            {
                await ErrorDialog.MostrarAsync(ex, this.Content.XamlRoot, "al cargar la lista de usuarios");
            }
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

        private async void AgregarUsuario_Click(object sender, RoutedEventArgs e) => await MostrarDialogoUsuario(null);

        private async void ModificarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (UsuariosListView.SelectedItem is Usuario usuario)
            {
                await MostrarDialogoUsuario(usuario);
            }
        }

        private async void EliminarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (UsuariosListView.SelectedItem is Usuario usuario)
            {
                ContentDialog dialog = new()
                {
                    Title = "Eliminar usuario",
                    Content = $"¿Desea eliminar al usuario '{usuario.Username}' ({usuario.Name})?",
                    PrimaryButtonText = "Eliminar",
                    CloseButtonText = "Cancelar",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = ElementTheme.Light
                };

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    if (usuario.UserResourceId.HasValue)
                        await _userService.DeleteUsuarioAsync(usuario.UserResourceId.Value);

                    await CargarUsuariosDesdeApi();
                }
            }
        }

        private async Task MostrarDialogoUsuario(Usuario? usuarioExistente)
        {
            bool esEdicion = usuarioExistente != null;
            TextBox txtNombre = new() { Header = "Nombre Completo", Text = usuarioExistente?.Name ?? "" };
            TextBox txtUsername = new() { Header = "Nombre de Usuario", Text = usuarioExistente?.Username ?? "" };
            TextBox txtEmail = new() { Header = "Correo Electrónico", Text = usuarioExistente?.Email ?? "" };

            // Campo de contraseña
            PasswordBox txtPassword = new() { Header = esEdicion ? "Nueva Contraseña (dejar en blanco para conservar)" : "Contraseña" };

            // Selección de roles
            TextBlock lblRoles = new() { Text = "Roles", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 10, 0, 5) };
            CheckBox chkAdmin = new() { Content = "Administrator", IsChecked = usuarioExistente?.RolesList.Contains("Administrator") ?? false };
            CheckBox chkCustomer = new() { Content = "Customer", IsChecked = usuarioExistente?.RolesList.Contains("Customer") ?? false };

            StackPanel panel = new() { Spacing = 10 };
            panel.Children.Add(txtNombre);
            panel.Children.Add(txtUsername);
            panel.Children.Add(txtEmail);
            panel.Children.Add(txtPassword);
            panel.Children.Add(lblRoles);
            panel.Children.Add(chkAdmin);
            panel.Children.Add(chkCustomer);

            ContentDialog dialog = new()
            {
                Title = esEdicion ? "Modificar usuario" : "Agregar usuario",
                Content = new ScrollViewer { Content = panel, MaxHeight = 400 },
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Light
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                try
                {
                    // Recopilar roles seleccionados
                    var rolesSeleccionados = new List<string>();
                    if (chkAdmin.IsChecked == true) rolesSeleccionados.Add("Administrator");
                    if (chkCustomer.IsChecked == true) rolesSeleccionados.Add("Customer");

                    if (esEdicion)
                    {
                        usuarioExistente!.Name = txtNombre.Text;
                        usuarioExistente.Username = txtUsername.Text;
                        usuarioExistente.Email = txtEmail.Text;
                        usuarioExistente.RolesList = rolesSeleccionados;

                        string? pass = string.IsNullOrEmpty(txtPassword.Password) ? null : txtPassword.Password;
                        if (usuarioExistente.UserResourceId.HasValue)
                            await _userService.UpdateUsuarioAsync(usuarioExistente.UserResourceId.Value, usuarioExistente, pass);
                    }
                    else
                    {
                        var nuevoUsuario = new Usuario
                        {
                            UserResourceId = Guid.NewGuid(),
                            Name = txtNombre.Text,
                            Username = txtUsername.Text,
                            Email = txtEmail.Text,
                            RolesList = rolesSeleccionados
                        };

                        await _userService.AddUsuarioAsync(nuevoUsuario, txtPassword.Password);
                    }

                    // Recargar lista para mostrar los cambios automáticamente
                    await CargarUsuariosDesdeApi();
                }
                catch (Exception ex)
                {
                    await ErrorDialog.MostrarAsync(ex, this.Content.XamlRoot, "al guardar el usuario");
                }
            }
        }
    }
}
