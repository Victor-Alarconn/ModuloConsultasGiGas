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
                string cufe = ObtenerCufe(factura.FacturaId); // Método para obtener el CUFE
                Adquiriente adquiriente = ObtenerAdquiriente(factura.FacturaId); // Método para obtener el adquiriente
                Movimiento movimiento = ObtenerMovimiento(factura.FacturaId); // Método para obtener el movimiento
                Encabezado encabezado1 = ObtenerEncabezado(factura.FacturaId); // Método para obtener el encabezado
                List<FormaPago> listaFormaPago = ObtenerFormasPago(factura.FacturaId); // Método para obtener las formas de pago

                // Verifica que todos los datos sean válidos antes de proceder
                if (emisor == null || listaProductos == null || cufe == null || adquiriente == null ||
                    movimiento == null || encabezado1 == null || listaFormaPago == null)
                {
                    MessageBox.Show("Error al obtener los datos necesarios para generar el PDF.");
                    return;
                }

                // Crear un archivo temporal para guardar el PDF
                string rutaArchivoTemporal = System.IO.Path.GetTempFileName();

                // Llamar al método CrearPDF de la clase GenerarPDF
                ModuloConsultasGiGas.VewModel.GenerarPDF.CrearPDF(rutaArchivoTemporal, emisor, factura, listaProductos, cufe, adquiriente, movimiento, encabezado1, listaFormaPago);

                // Crear y configurar el SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                saveFileDialog.Title = "Guardar archivo PDF";
                saveFileDialog.FileName = "Factura_" + factura.FacturaId + ".pdf";

                // Mostrar el diálogo al usuario
                if (saveFileDialog.ShowDialog() == true)
                {
                    string rutaArchivo = saveFileDialog.FileName;

                    // Copiar el archivo temporal a la ubicación seleccionada por el usuario
                    System.IO.File.Copy(rutaArchivoTemporal, rutaArchivo, true);

                    // Mensaje de confirmación al usuario
                    MessageBox.Show("El PDF ha sido guardado en: " + rutaArchivo);
                }
                else
                {
                    // El usuario canceló la operación
                    MessageBox.Show("Guardado cancelado.");
                }

                // Eliminar el archivo temporal
                System.IO.File.Delete(rutaArchivoTemporal);
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

                    // Verifica si la facturaId existe en el archivo
                    if (facturasEnMemoria != null && facturasEnMemoria.ContainsKey(facturaId))
                    {
                        var productosTemp = facturasEnMemoria[facturaId];

                        // Filtra los productos según el recibo
                        foreach (var productoDict in productosTemp)
                        {
                            // Convierte el diccionario a un objeto Producto
                            var producto = JsonConvert.DeserializeObject<Productos>(JsonConvert.SerializeObject(productoDict));

                            if (recibo == null || string.IsNullOrEmpty(recibo) || producto.Recibo == recibo)
                            {
                                productos.Add(producto);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No se encontraron productos para la factura especificada.");
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
            // Aquí va la lógica para obtener el CUFE.
            // Por ejemplo, si la información del CUFE está en la tabla "fac".
            var tabla = "fac";
            if (resultadosPorTabla.ContainsKey(tabla))
            {
                var datosFactura = resultadosPorTabla[tabla].FirstOrDefault(f => f.ContainsKey("factura") && f["factura"].ToString() == facturaId);
                if (datosFactura != null)
                {
                    return datosFactura["cufe"].ToString();
                }
            }
            return null;
        }

        private Adquiriente ObtenerAdquiriente(string facturaId)
        {
            // Aquí va la lógica para obtener el Adquiriente.
            // Por ejemplo, si la información del adquiriente está en la tabla "xxxxccfc".
            var tabla = "xxxxccfc";
            if (resultadosPorTabla.ContainsKey(tabla))
            {
                var datosAdquiriente = resultadosPorTabla[tabla].FirstOrDefault(f => f.ContainsKey("factura") && f["factura"].ToString() == facturaId);
                if (datosAdquiriente != null)
                {
                    return new Adquiriente
                    {
                        // Asigna las propiedades del Adquiriente usando datosAdquiriente
                        // Ejemplo: Nombre = datosAdquiriente["nombre"].ToString()
                    };
                }
            }
            return null;
        }

        private Movimiento ObtenerMovimiento(string facturaId)
        {
            var tabla = "xxxxccfc";
            if (resultadosPorTabla.ContainsKey(tabla))
            {
                var datosMovimiento = resultadosPorTabla[tabla].FirstOrDefault(f => f.ContainsKey("factura") && f["factura"].ToString() == facturaId);
                if (datosMovimiento != null)
                {
                    return new Movimiento
                    {
                        // Asigna las propiedades del Movimiento usando datosMovimiento
                        // Ejemplo: Tipo = datosMovimiento["tipo"].ToString()
                    };
                }
            }
            return null;
        }

        private Encabezado ObtenerEncabezado(string facturaId)
        {
            var tabla = "xxxxterm";
            if (resultadosPorTabla.ContainsKey(tabla))
            {
                var datosEncabezado = resultadosPorTabla[tabla].FirstOrDefault(f => f.ContainsKey("factura") && f["factura"].ToString() == facturaId);
                if (datosEncabezado != null)
                {
                    return new Encabezado
                    {
                        // Asigna las propiedades del Encabezado usando datosEncabezado
                        // Ejemplo: Titulo = datosEncabezado["titulo"].ToString()
                    };
                }
            }
            return null;
        }

        private List<FormaPago> ObtenerFormasPago(string facturaId)
        {
            var tabla = "xxxxccpg";
            var formasPago = new List<FormaPago>();
            if (resultadosPorTabla.ContainsKey(tabla))
            {
                var datosFormasPago = resultadosPorTabla[tabla].Where(fp => fp.ContainsKey("factura") && fp["factura"].ToString() == facturaId).ToList();
                foreach (var datosFormaPago in datosFormasPago)
                {
                    formasPago.Add(new FormaPago
                    {
                        // Asigna las propiedades de FormaPago usando datosFormaPago
                        // Ejemplo: Metodo = datosFormaPago["metodo"].ToString()
                    });
                }
            }
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
