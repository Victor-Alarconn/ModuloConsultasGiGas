using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuloConsultasGiGas.Modelo
{
    public class Factura
    {
        // Definir las propiedades de la clase Factura aquí
        public string FacturaId { get; set; }
        public string Cliente { get; set; }
        public string Recibo { get; set; }

        public string Estado { get; set; }
        public string Error { get; set; }
        public string Memo { get; set; }
        public string Fecha { get; set; }
        public string Encabezado { get; set; }
        public string Encabezado_NC { get; set; }
        public string Articulos { get; set; }
        public string Metodo { get; set; }
       


    }
}
