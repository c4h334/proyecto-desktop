using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using proyecto_desktop.Services;

namespace proyecto_desktop.Views
{
    public sealed partial class RegisterAdminPage : Page
    {
        private readonly AuthService _authService = new();

        public RegisterAdminPage() => this.InitializeComponent();

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            LoadingRing.IsActive = true;
            BtnRegister.IsEnabled = false;

            bool success = await _authService.RegisterAdminAsync(
                TxtName.Text, TxtUsername.Text, TxtEmail.Text, TxtPassword.Password);

            if (success)
            {
                AlertBar.Title = "Éxito";
                AlertBar.Message = "Admin registrado.";
                AlertBar.Severity = InfoBarSeverity.Success;
            }
            else
            {
                AlertBar.Title = "Error";
                AlertBar.Message = "Error al registrar.";
                AlertBar.Severity = InfoBarSeverity.Error;
            }
            AlertBar.IsOpen = true;
            LoadingRing.IsActive = false;
            BtnRegister.IsEnabled = true;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Frame.GoBack();
    }
}