using Microsoft.Win32;
using ModuloConsultasGiGas.Modelo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ModuloConsultasGiGas.Model
{
    public class FacturaViewModel : INotifyPropertyChanged
    {
        private readonly string tempFilePath;
        private readonly List<Emisor> listaEmpresas;
        private string codigoEmpresaSeleccionada;
        private Dictionary<string, List<Dictionary<string, object>>> resultadosPorTabla;
        public ICommand ExportPdfCommand { get; private set; }
        public ICommand ExportXmlCommand { get; private set; }
        public ICommand SendEmailCommand { get; private set; }

        public FacturaViewModel( string tempFilePath)
        {
            this.tempFilePath = tempFilePath;
            
            ExportPdfCommand = new RelayCommand<Factura>(ExportPdf);
            ExportXmlCommand = new RelayCommand<Factura>(ExportXml);
            SendEmailCommand = new RelayCommand<Factura>(SendEmail);
        }

        public string CodigoEmpresaSeleccionada
        {
            get => codigoEmpresaSeleccionada;
            set
            {
                if (codigoEmpresaSeleccionada != value)
                {
                    codigoEmpresaSeleccionada = value;
                    OnPropertyChanged(nameof(CodigoEmpresaSeleccionada));
                }
            }
        }

        private void ExportPdf(Factura factura)
        {
            try
            {
                // Obtener todos los datos necesarios para la generación del PDF
                Emisor emisor = ObtenerEmisor(); // Método para obtener el emisor
                List<Productos> listaProductos = ObtenerProductos(factura.FacturaId, factura.Recibo); // Método para obtener la lista de productos
                Adquiriente adquiriente = ObtenerAdquiriente(factura.FacturaId); // Método para obtener el adquiriente
                Movimiento movimiento = ObtenerMovimiento(factura.FacturaId); // Método para obtener el movimiento
                Encabezado encabezado1 = ObtenerEncabezado(factura.FacturaId, factura.Terminal); // Método para obtener el encabezado
                List<FormaPago> listaFormaPago = ObtenerFormasPago(factura.FacturaId); // Método para obtener las formas de pago
                string cufe = ObtenerCufe(factura.FacturaId); // Método para obtener el CUFE

                // Verifica que todos los datos sean válidos antes de proceder
                if (emisor == null || listaProductos == null || cufe == null || adquiriente == null ||
                    movimiento == null || encabezado1 == null || listaFormaPago == null)
                {
                    MessageBox.Show("Error al obtener los datos necesarios para generar el PDF.");
                    return;
                }

                // Crear y configurar el SaveFileDialog antes de crear el PDF
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                saveFileDialog.Title = "Guardar archivo PDF";
                saveFileDialog.FileName = "Factura_" + factura.FacturaId + ".pdf";

                // Mostrar el diálogo al usuario
                if (saveFileDialog.ShowDialog() == true)
                {
                    string rutaArchivo = saveFileDialog.FileName;

                    // Llamar al método CrearPDF de la clase GenerarPDF
                    ModuloConsultasGiGas.VewModel.GenerarPDF.CrearPDF(rutaArchivo, emisor, factura, listaProductos, cufe, adquiriente, movimiento, encabezado1, listaFormaPago);

                    // Mensaje de confirmación al usuario
                    MessageBox.Show("El PDF ha sido guardado en: " + rutaArchivo);
                }
                else
                {
                    // El usuario canceló la operación
                    MessageBox.Show("Guardado cancelado.");
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que ocurra durante el proceso
                MessageBox.Show("Ocurrió un error al generar el PDF: " + ex.Message);
            }
        }





        private void ExportXml(Factura factura)
        {
            // Mensaje de prueba para el botón XML
            MessageBox.Show("Botón XML presionado para la factura: " + factura.FacturaId);
        }

        private void SendEmail(Factura factura)
        {
            // Mensaje de prueba para el botón ENVIO
            MessageBox.Show("Botón ENVIO presionado para la factura: " + factura.FacturaId);
        }

        // Implementa INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



        private Emisor ObtenerEmisor()
        {
            try
            {
                // Lee el archivo temporal
                string json = System.IO.File.ReadAllText(tempFilePath);
                var listaEmpresas = JsonConvert.DeserializeObject<List<Emisor>>(json);

                if (listaEmpresas != null)
                {
                    // Busca la empresa seleccionada usando el código de empresa
                    var selectedEmpresa = listaEmpresas.FirstOrDefault(empresa => empresa.Emprobra == codigoEmpresaSeleccionada);
                    if (selectedEmpresa != null)
                    {
                        return new Emisor
                        {
                            Emprobra = selectedEmpresa.Emprobra,
                            Nombre_emisor = selectedEmpresa.Nombre_emisor,
                            Codigo_FormaPago_emisor = selectedEmpresa.Codigo_FormaPago_emisor,
                            cude = selectedEmpresa.cude,
                            Nombre_municipio_emisor = selectedEmpresa.Nombre_municipio_emisor,
                            Codigo_departamento_emisor = selectedEmpresa.Codigo_departamento_emisor,
                            Nombre_departamento_emisor = selectedEmpresa.Nombre_departamento_emisor,
                            Direccion_emisor = selectedEmpresa.Direccion_emisor,
                            Codigo_postal_emisor = selectedEmpresa.Codigo_postal_emisor,
                            Nit_emisor = selectedEmpresa.Nit_emisor,
                            Responsable_emisor = selectedEmpresa.Responsable_emisor,
                            Correo_emisor = selectedEmpresa.Correo_emisor,
                            Tipo_emisor = selectedEmpresa.Tipo_emisor,
                            Telefono_emisor = selectedEmpresa.Telefono_emisor,
                            Retiene_emisor = selectedEmpresa.Retiene_emisor,
                            Url_emisor = selectedEmpresa.Url_emisor,
                            Ciudad_emisor = selectedEmpresa.Ciudad_emisor,
                            Representante = selectedEmpresa.Representante,
                            Ica = selectedEmpresa.Ica,
                            Logo_emisor = selectedEmpresa.Logo_emisor
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener el emisor: " + ex.Message);
            }
            return null;
        }



        private List<Productos> ObtenerProductos(string facturaId, string recibo)
        {
            var productos = new List<Productos>();

            try
            {
                // Lee el archivo temporal de facturas
                string tempFilePathFacturas = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "facturas_temp.json");
                if (File.Exists(tempFilePathFacturas))
                {
                    // Lee el contenido del archivo JSON
                    string json = File.ReadAllText(tempFilePathFacturas);
                    var facturasEnMemoria = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(json);

                    // Verifica si la tabla xxxxMvin existe en el archivo
                    if (facturasEnMemoria != null && facturasEnMemoria.ContainsKey("xxxxmvin"))
                    {
                        var productosTemp = facturasEnMemoria["xxxxmvin"];

                        // Filtra los productos según la facturaId y el recibo
                        foreach (var productoDict in productosTemp)
                        {
                            // Convierte el diccionario a un objeto Producto
                            var producto = JsonConvert.DeserializeObject<Productos>(JsonConvert.SerializeObject(productoDict));

                            // Verifica si la factura coincide y aplica el filtro de recibo si es necesario
                            if (producto.Factura == facturaId && (string.IsNullOrEmpty(recibo) || producto.Recibo == recibo))
                            {
                                productos.Add(producto);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No se encontraron productos en la tabla 'xxxxmvin'.");
                    }
                }
                else
                {
                    MessageBox.Show("El archivo temporal de facturas no existe.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener los productos: " + ex.Message);
            }

            return productos;
        }





        private string ObtenerCufe(string facturaId)
        {
            // Ruta al archivo temporal
            string tempFilePathFacturas = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "facturas_temp.json");

            // Leer el archivo JSON
            string jsonContent = File.ReadAllText(tempFilePathFacturas);

            // Deserializar el contenido del archivo en un objeto dinámico
            dynamic facturasTemp = JsonConvert.DeserializeObject(jsonContent);

            // Buscar el registro en la tabla 'xxxxccfc' donde la factura coincida con facturaId
            var registro = ((IEnumerable<dynamic>)facturasTemp["xxxxccfc"])
                           .FirstOrDefault(r => r.factura == facturaId);

            // Si se encuentra el registro, retornar el valor de 'dato_cufe'
            if (registro != null)
            {
                return registro.dato_cufe != null ? registro.dato_cufe.ToString() : null;
            }

            // Si no se encuentra el registro, retornar null
            return null;
        }


        private Adquiriente ObtenerAdquiriente(string facturaId)
        {
            var adquiriente = new Adquiriente();

            try
            {
                // Lee el archivo temporal de facturas
                string tempFilePathFacturas = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "facturas_temp.json");
                if (File.Exists(tempFilePathFacturas))
                {
                    // Lee el contenido del archivo JSON
                    string json = File.ReadAllText(tempFilePathFacturas);
                    var facturasEnMemoria = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(json);

                    // Verifica si la tabla 'xxxxmvin' y 'xxxx3ros' existen en el archivo
                    if (facturasEnMemoria != null && facturasEnMemoria.ContainsKey("xxxxmvin") && facturasEnMemoria.ContainsKey("xxxx3ros"))
                    {
                        var productosTemp = facturasEnMemoria["xxxxmvin"];
                        var adquirientesTemp = facturasEnMemoria["xxxx3ros"];

                        // Obtiene el nit del primer producto en la tabla 'xxxxmvin'
                        var primerProducto = productosTemp.FirstOrDefault(p => p.ContainsKey("factura") && p["factura"].ToString() == facturaId);
                        if (primerProducto != null && primerProducto.ContainsKey("nit"))
                        {
                            string nit = primerProducto["nit"].ToString();

                            // Busca los datos del adquiriente en la tabla 'xxxx3ros' usando el nit
                            var datosAdquiriente = adquirientesTemp.FirstOrDefault(a => a.ContainsKey("tronit") && a["tronit"].ToString() == nit);
                            if (datosAdquiriente != null)
                            {
                                // Concatena el nombre completo del adquiriente
                                string nombreCompleto = $"{datosAdquiriente["tronombre"]} {datosAdquiriente["tronomb_2"]} {datosAdquiriente["troapel_1"]} {datosAdquiriente["troapel_2"]}".Trim();

                                adquiriente = new Adquiriente
                                {
                                    Nombre_adqu = nombreCompleto,
                                    Nombre2 = datosAdquiriente.ContainsKey("tronombre2") ? datosAdquiriente["tronombre2"].ToString() : null,
                                    Apellido = datosAdquiriente.ContainsKey("troapel_1") ? datosAdquiriente["troapel_1"].ToString() : null,
                                    Apellido2 = datosAdquiriente.ContainsKey("troapel_2") ? datosAdquiriente["troapel_2"].ToString() : null,
                                    Codigo_municipio_adqui = datosAdquiriente.ContainsKey("trocciu") ? datosAdquiriente["trocciu"].ToString() : null,
                                    Nombre_municipio_adqui = datosAdquiriente.ContainsKey("trociudad") ? datosAdquiriente["trociudad"].ToString() : null,
                                    Codigo_departamento_adqui = datosAdquiriente.ContainsKey("trodato_cp") ? datosAdquiriente["trodato_cp"].ToString() : null,
                                    Nombre_departamento_adqui = datosAdquiriente.ContainsKey("trodato_cc") ? datosAdquiriente["trodato_cc"].ToString() : null,
                                    Direccion_adqui = datosAdquiriente.ContainsKey("trodirec") ? datosAdquiriente["trodirec"].ToString() : null,
                                    Codigo_postal_adqui = datosAdquiriente.ContainsKey("trocity") ? datosAdquiriente["trocity"].ToString() : null,
                                    Nit_adqui = datosAdquiriente.ContainsKey("tronit") ? datosAdquiriente["tronit"].ToString() : null,
                                    Dv_Adqui = datosAdquiriente.ContainsKey("trodigito") ? datosAdquiriente["trodigito"].ToString() : null,
                                    Responsable = datosAdquiriente.ContainsKey("troactivo") ? Convert.ToDecimal(datosAdquiriente["troactivo"]) : 0,
                                    Correo_adqui = datosAdquiriente.ContainsKey("troemail") ? datosAdquiriente["troemail"].ToString() : null,
                                    Tipo_p = datosAdquiriente.ContainsKey("trotp_3ro") ? Convert.ToDecimal(datosAdquiriente["trotp_3ro"]) : 0,
                                    Telefono_adqui = datosAdquiriente.ContainsKey("trocelular") ? datosAdquiriente["trocelular"].ToString() : null,
                                    Tipo_doc = datosAdquiriente.ContainsKey("trotipo") ? datosAdquiriente["trotipo"].ToString() : null,
                                    Correo2 = datosAdquiriente.ContainsKey("troemail") ? datosAdquiriente["troemail"].ToString() : null
                                };
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No se encontraron datos en las tablas 'xxxxmvin' o 'xxxx3ros'.");
                    }
                }
                else
                {
                    MessageBox.Show("El archivo temporal de facturas no existe.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener el adquiriente: " + ex.Message);
            }

            return adquiriente;
        }




        private Movimiento ObtenerMovimiento(string facturaId)
        {
            // Ruta al archivo temporal
            string tempFilePathFacturas = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "facturas_temp.json");

            // Leer el archivo JSON
            string jsonContent = File.ReadAllText(tempFilePathFacturas);

            // Deserializar el contenido del archivo en un objeto dinámico
            dynamic facturasTemp = JsonConvert.DeserializeObject(jsonContent);

            // Inicializar el objeto Movimiento
            Movimiento movimiento = new Movimiento();

            // Buscar en la tabla 'xxxxccfc'
            var movimientoData = ((IEnumerable<dynamic>)facturasTemp["xxxxccfc"])
                                 .FirstOrDefault(m => m.factura == facturaId);

            if (movimientoData != null)
            {
                // Asignar los valores al objeto Movimiento
                movimiento.Nit = movimientoData.nit;
                movimiento.Valor = movimientoData.valor != null ? Convert.ToDecimal(movimientoData.valor) : 0;
                movimiento.Valor_iva = movimientoData.vriva != null ? Convert.ToDecimal(movimientoData.vriva) : 0;
                movimiento.Valor_dsto = movimientoData.desctos != null ? Convert.ToDecimal(movimientoData.desctos) : 0;
                movimiento.Valor_neto = movimientoData.gravada != null ? Convert.ToDecimal(movimientoData.gravada) : 0;
                movimiento.Exentas = movimientoData.exentas != null ? Convert.ToDecimal(movimientoData.exentas) : 0;
                movimiento.Fecha_Factura = movimientoData.fcruce != null ? Convert.ToDateTime(movimientoData.fcruce) : DateTime.MinValue;
                movimiento.Hora_dig = movimientoData.hdigita;
                movimiento.Retiene = movimientoData.rfuente != null ? Convert.ToDecimal(movimientoData.rfuente) : 0;
                movimiento.Ipoconsumo = movimientoData.consumo != null ? Convert.ToDecimal(movimientoData.consumo) : 0;
                movimiento.Numero_bolsa = movimientoData.nbolsa != null ? Convert.ToDecimal(movimientoData.nbolsa) : 0;
                movimiento.Valor_bolsa = movimientoData.vbolsa != null ? Convert.ToDecimal(movimientoData.vbolsa) : 0;
                movimiento.Dato_Cufe = movimientoData.dato_cufe;
                movimiento.Dato_Qr = movimientoData.dato_qr;
                movimiento.Numero = movimientoData.numero;
                movimiento.Vendedor = movimientoData.electron;
                movimiento.Dias = movimientoData.dias != null ? Convert.ToDecimal(movimientoData.dias) : 0;
            }
            else
            {
                // Si no se encuentra en 'xxxxccfc', buscar en 'xxxxcmbt' si es una devolución
                var devolucionData = ((IEnumerable<dynamic>)facturasTemp["xxxxcmbt"])
                                     .FirstOrDefault(m => m.factura == facturaId);

                if (devolucionData != null)
                {
                    // Asignar los valores al objeto Movimiento para devoluciones
                    movimiento.Nit = devolucionData.nit;
                    movimiento.Valor = devolucionData.valor != null ? Convert.ToDecimal(devolucionData.valor) : 0;
                    movimiento.Dato_Qr = devolucionData.dato_qr;
                    movimiento.Nota_credito = devolucionData.debitos != null ? Convert.ToDecimal(devolucionData.debitos) : 0;
                }
            }

            return movimiento;
        }


        private Encabezado ObtenerEncabezado(string facturaId, string terminal)
        {
            // Ruta al archivo temporal
            string tempFilePathFacturas = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "facturas_temp.json");

            // Leer el archivo JSON
            string jsonContent = File.ReadAllText(tempFilePathFacturas);

            // Deserializar el contenido del archivo en un objeto dinámico
            dynamic facturasTemp = JsonConvert.DeserializeObject(jsonContent);

            // Inicializar el objeto Encabezado
            Encabezado encabezado = new Encabezado();

            // Buscar en la tabla 'xxxxterm' el registro que coincida con facturaId y terminal
            var encabezadoData = ((IEnumerable<dynamic>)facturasTemp["xxxxterm"])
                                 .FirstOrDefault(e => e.terminal == terminal);

            if (encabezadoData != null)
            {
                // Asignar los valores al objeto Encabezado
                encabezado.Autorizando = encabezadoData.resol_fe;
                encabezado.Fecha_inicio = encabezadoData.f_inicio != null ? Convert.ToDateTime(encabezadoData.f_inicio) : DateTime.MinValue;
                encabezado.Fecha_termina = encabezadoData.f_termina != null ? Convert.ToDateTime(encabezadoData.f_termina) : DateTime.MinValue;
                encabezado.R_inicio = encabezadoData.r_inicio != null ? Convert.ToInt32(encabezadoData.r_inicio) : 0;
                encabezado.R_termina = encabezadoData.r_termina != null ? Convert.ToInt32(encabezadoData.r_termina) : 0;
                encabezado.Prefijo = encabezadoData.prefijo0;
                encabezado.Resolucion = encabezadoData.resolucion;
                encabezado.Notas = encabezadoData.notas;
                encabezado.Nota_fin = encabezadoData.NOTA_FIN;
                encabezado.Llave_tecnica = encabezadoData.llave_tecn;
            }

            return encabezado;
        }
                                                                                                                                                                                                                            


        private List<FormaPago> ObtenerFormasPago(string facturaId)
{
    // Ruta al archivo temporal
    string tempFilePathFacturas = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "facturas_temp.json");

    // Leer el archivo JSON
    string jsonContent = File.ReadAllText(tempFilePathFacturas);

    // Deserializar el contenido del archivo en un objeto dinámico
    dynamic facturasTemp = JsonConvert.DeserializeObject(jsonContent);

    // Inicializar la lista de formas de pago
    var formasPago = new List<FormaPago>();

    // Buscar en la tabla 'xxxxccpg' los registros que coincidan con la facturaId
    var formasPagoData = ((IEnumerable<dynamic>)facturasTemp["xxxxccpg"])
                         .Where(fp => fp.factura == facturaId);

    // Mapear los datos a la clase FormaPago y agregarlos a la lista
    foreach (var data in formasPagoData)
    {
        FormaPago formaPago = new FormaPago
        {
            // Si 'bancop' es nulo, asignar "00"
            Id_forma = data.bancop != null ? data.bancop.ToString() : "00",

            // Si 'codigo' es nulo, asignar null (o puedes usar otro valor predeterminado)
            Codigo_forma = data.codigo?.ToString(),

            // Si 'vrpago' es nulo, asignar 0.00m
            Valor_pago = data.vrpago != null ? Convert.ToDecimal(data.vrpago) : 0.00m,

            // Si 'fecha' es nulo, asignar DateTime.MinValue
            Fecha_pago = data.fecha != null ? Convert.ToDateTime(data.fecha) : DateTime.MinValue,

            // Si 'factura' es nulo, asignar null
            Factura_pago = data.factura?.ToString(),

            // Si 'tpago' es nulo, asignar 0
            Terceros_pago = data.tpago != null ? Convert.ToInt32(data.tpago) : 0
        };

        formasPago.Add(formaPago);
    }

    // Retornar la lista de formas de pago
    return formasPago;
}




    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

}
