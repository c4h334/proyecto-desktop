using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace proyecto_desktop
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance;
        private AppWindow m_AppWindow;

        public MainWindow()
        {
            this.InitializeComponent();
            Instance = this;
            m_AppWindow = this.AppWindow;
            m_AppWindow.SetIcon("Assets/raul-vega-logo.ico");

            if (this.Content is FrameworkElement rootElement)
                rootElement.RequestedTheme = ElementTheme.Light;

            TrySetMicaBackdrop();

            TopNavView.Visibility = Visibility.Collapsed;
            LoginFrame.Navigate(typeof(Views.LoginPage));
        }

        public void MostrarMenuPrincipal(string nombreAdministrador)
        {
            AdminNameText.Text = nombreAdministrador;
            LoginFrame.Visibility = Visibility.Collapsed;
            TopNavView.Visibility = Visibility.Visible;
            TopNavView.SelectedItem = TopNavView.MenuItems[0];
            ContentFrame.Navigate(typeof(Views.ProductosPage));
        }

        private bool TrySetMicaBackdrop() { try { this.SystemBackdrop = new MicaBackdrop(); return true; } catch { return false; } }

        private void TopNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer == null) return;
            string tag = args.SelectedItemContainer.Tag?.ToString() ?? "";

            switch (tag)
            {
                case "Productos": ContentFrame.Navigate(typeof(Views.ProductosPage)); break;
                case "Usuarios": ContentFrame.Navigate(typeof(Views.UsuariosPage)); break;
                case "Proveedores": ContentFrame.Navigate(typeof(Views.ProveedoresPage)); break;
            }
        }
    }
}
