using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using proyecto_desktop.Models;
using System.Collections.ObjectModel;

namespace proyecto_desktop
{
    public sealed partial class MainWindow : Window
    {
        private ObservableCollection<Producto> productos = new();
        private ObservableCollection<Cliente> clientes = new();
        private ObservableCollection<Proveedor> proveedores = new();

        public MainWindow()
        {
            this.InitializeComponent();

            // Aplicar permanentemente el tema claro
            if (this.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ElementTheme.Light;
            }

            // Aplicar fondo Mica
            TrySetMicaBackdrop();

            // Cargar datos de prueba
            CargarDatosPrueba();

            // Enlazar las listas visuales con sus respectivas colecciones
            ProductosListView.ItemsSource = productos;
            ClientesListView.ItemsSource = clientes;
            ProveedoresListView.ItemsSource = proveedores;

            // Definir la vista inicial
            TopNavView.SelectedItem = TopNavView.MenuItems[0];

            VistaProductos.Visibility = Visibility.Visible;
            VistaClientes.Visibility = Visibility.Collapsed;
            VistaProveedores.Visibility = Visibility.Collapsed;
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

        private void CargarDatosPrueba()
        {
            productos.Add(new Producto
            {
                Nombre = "Silla",
                Descripcion = "Silla de madera",
                Cantidad = 10,
                Precio = 25000,
                Codigo = "P001",
                Disponible = true,
                Descuento = 5,
                CantDescuento = 2,
                Material = "Madera"
            });

            productos.Add(new Producto
            {
                Nombre = "Mesa",
                Descripcion = "Mesa para comedor",
                Cantidad = 4,
                Precio = 75000,
                Codigo = "P002",
                Disponible = true,
                Descuento = 0,
                CantDescuento = 0,
                Material = "Madera"
            });

            clientes.Add(new Cliente
            {
                Nombre = "Anderson Jesús",
                Apellidos = "Monge Alvarado",
                Identificacion = "123456789",
                Tel = "8888-8888",
                DireccionCasa = "Cartago",
                Correo = "correo@ejemplo.com"
            });

            clientes.Add(new Cliente
            {
                Nombre = "Jose Gabriel",
                Apellidos = "Chacón Calderón",
                Identificacion = "987654321",
                Tel = "8999-9999",
                DireccionCasa = "San José",
                Correo = "jose@ejemplo.com"
            });

            proveedores.Add(new Proveedor
            {
                Nombre = "Jacqueline María",
                Apellidos = "Oviedo Miranda",
                Identificacion = "111111111",
                Tel = "7777-7777",
                DireccionCasa = "San José",
                Correo = "proveedor@empresa.com"
            });

            proveedores.Add(new Proveedor
            {
                Nombre = "Distribuidora",
                Apellidos = "Central",
                Identificacion = "222222222",
                Tel = "7000-0000",
                DireccionCasa = "Alajuela",
                Correo = "contacto@distribuidora.com"
            });
        }

        private void TopNavView_SelectionChanged(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer == null)
            {
                return;
            }

            string tag = args.SelectedItemContainer.Tag?.ToString() ?? "";

            VistaProductos.Visibility = tag == "Productos"
                ? Visibility.Visible
                : Visibility.Collapsed;

            VistaClientes.Visibility = tag == "Clientes"
                ? Visibility.Visible
                : Visibility.Collapsed;

            VistaProveedores.Visibility = tag == "Proveedores"
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}