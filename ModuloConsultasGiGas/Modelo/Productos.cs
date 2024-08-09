using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuloConsultasGiGas.Modelo
{
    public class Productos
    {
        [JsonProperty("factura")]
        public string? Factura { get; set; }

        [JsonProperty("codigo")]
        public string? Codigo { get; set; }

        [JsonProperty("recibo")]
        public string? Recibo { get; set; }

        [JsonProperty("nit")]
        public string? Nit { get; set; }

        [JsonProperty("detalle")]
        public string? Detalle { get; set; }

        [JsonProperty("serial")]
        public string? Serial { get; set; }

        [JsonProperty("cantidad")]
        public decimal Cantidad { get; set; }

        [JsonProperty("valor")]
        public decimal Valor { get; set; }

        [JsonProperty("neto")]
        public decimal Neto { get; set; }

        [JsonProperty("dsct4")]
        public decimal Descuento { get; set; }

        [JsonProperty("iva")]
        public decimal Iva { get; set; }

        [JsonProperty("vriva")]
        public decimal IvaTotal { get; set; }

        [JsonProperty("vrventa")]
        public decimal Total { get; set; }

        [JsonProperty("consumo")]
        public decimal Consumo { get; set; }

        [JsonProperty("fdigitar")]
        public DateTime Fecha { get; set; }

        [JsonProperty("hdigita")]
        public string? Hora_Digitada { get; set; }

        [JsonProperty("artiexclu")]
        public int Excluido { get; set; }

        [JsonProperty("vrcmpant")]
        public decimal Valor2 { get; set; }
    }

}
