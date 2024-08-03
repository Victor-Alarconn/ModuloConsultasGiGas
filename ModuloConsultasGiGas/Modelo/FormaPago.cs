﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuloConsultasGiGas.Modelo
{
    public class FormaPago
    {
        public string Id_forma { get; set; }
        public string? Codigo_forma { get; set; }
        public decimal Valor_pago { get; set; }
        public DateTime Fecha_pago { get; set; }
        public string? Factura_pago { get; set; }
        public int Terceros_pago { get; set; }
    }
}
