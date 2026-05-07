using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proyecto_desktop.Models
{
    public class Producto
    {


        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string Codigo { get; set; } = "";
        public bool Disponible { get; set; }
        public decimal Descuento { get; set; }
        public int CantDescuento { get; set; }
        public string Material { get; set; } = "";
    }
}
