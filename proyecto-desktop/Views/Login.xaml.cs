using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using proyecto_desktop.Services;

namespace proyecto_desktop.Views
{
    public sealed partial class LoginPage : Page
    {
        private readonly AuthService _authService = new();

        public LoginPage() => this.InitializeComponent();

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoadingRing.IsActive = true;
            BtnLogin.IsEnabled = false;

            var response = await _authService.LoginAsync(TxtUsername.Text, TxtPassword.Password);

            if (response != null)
            {
                if (_authService.IsAdministrator(response.BearerToken))
                {
                    MainWindow.Instance?.MostrarMenuPrincipal(response.Name);
                }
                else
                {
                    ErrorBar.Message = "Acceso denegado. Esta aplicación está reservada únicamente para administradores.";
                    ErrorBar.IsOpen = true;
                }
            }
            else
            {
                ErrorBar.Message = "Usuario o contraseña incorrectos.";
                ErrorBar.IsOpen = true;
            }
            LoadingRing.IsActive = false;
            BtnLogin.IsEnabled = true;
        }

        private void BtnRegisterLink_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(RegisterAdminPage));
    }
}