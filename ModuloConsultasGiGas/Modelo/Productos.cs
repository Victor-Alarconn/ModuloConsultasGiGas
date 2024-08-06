using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuloConsultasGiGas.Modelo
{
    public class Productos
    {
        public string? Factura { get; set; } // Solo Aqui
        public string? Codigo { get; set; }
        public string? Recibo { get; set; }
        public string? Nit { get; set; }
        public string? Detalle { get; set; }
        public string? Serial { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Valor { get; set; }
        public decimal Neto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Iva { get; set; }
        public decimal IvaTotal { get; set; }
        public decimal Total { get; set; }
        public decimal Consumo { get; set; }
        public string? Hora_Digitada { get; set; }
        public int Excluido { get; set; }
        public decimal Valor2 { get; set; }
        public DateTime Fecha { get; set; }
    }
}
