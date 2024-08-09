using ModuloConsultasGiGas.Modelo;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ModuloConsultasGiGas.Consultas;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ModuloConsultasGiGas.Model;
using ModuloConsultasGiGas.View;

namespace ModuloConsultasGiGas
{
    /// <summary>
    /// Lógica de interacción para Consulta.xaml
    /// </summary>
    public partial class Consulta : Window
    {
        private string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "empresas_temp.json");
        private string codigoEmpresaSeleccionada;
        private List<Emisor> listaEmpresas = new List<Emisor>();
        private static Window resolucionesWindow;
        private FacturaViewModel facturaViewModel;
        public Consulta()
        {
            InitializeComponent();
            ConsultarDatos("empresas");
            ConfigurarColumnas();
            facturaViewModel = new FacturaViewModel(tempFilePath);
            this.DataContext = facturaViewModel;
        }

        private void ConsultarDatos(string databaseName)
        {
            try
            {
                ModuloConsultasGiGas.Data.Conexion conexionBD = new ModuloConsultasGiGas.Data.Conexion(databaseName);
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    string query = "SELECT emprobra, emprnombr, emprnit, emprtipo, emprdirec, emprciuda, empregim_x, emprperson, empremail, emprtelef, empretiene, empr_urlx, emprcity, emprepres, evtaica, logo, emprperson FROM empresas";
                    MySqlCommand comando = new MySqlCommand(query, conexion);
                    MySqlDataReader reader = comando.ExecuteReader();

                    while (reader.Read())
                    {
                        string Logo_emisor = string.Empty;
                        if (reader["logo"] != DBNull.Value)
                        {
                            byte[] logoBytes = (byte[])reader["logo"];
                            Logo_emisor = Convert.ToBase64String(logoBytes);
                        }
                        Emisor empresa = new Emisor
                        {
                            Emprobra = reader["emprobra"]?.ToString() ?? string.Empty,
                            Nombre_emisor = reader["emprnombr"]?.ToString() ?? string.Empty,
                            Nit_emisor = reader["emprnit"]?.ToString() ?? string.Empty,
                            Codigo_departamento_emisor = reader["emprtipo"]?.ToString() ?? string.Empty,
                            Direccion_emisor = reader["emprdirec"]?.ToString() ?? string.Empty,
                            Nombre_municipio_emisor = reader["emprciuda"]?.ToString() ?? string.Empty,
                            Responsable_emisor = reader["empregim_x"]?.ToString() ?? string.Empty,
                            Tipo_emisor = Convert.ToDecimal(reader["emprperson"]?.ToString() ?? "0"),
                            Correo_emisor = reader["empremail"]?.ToString() ?? string.Empty,
                            Telefono_emisor = reader["emprtelef"]?.ToString() ?? string.Empty,
                            Retiene_emisor = Convert.ToDecimal(reader["empretiene"]?.ToString() ?? "0"),
                            Url_emisor = reader["empr_urlx"]?.ToString() ?? string.Empty,
                            Ciudad_emisor = reader["emprcity"]?.ToString() ?? string.Empty,
                            Representante = reader["emprepres"]?.ToString() ?? string.Empty,
                            Ica = reader["evtaica"]?.ToString() ?? string.Empty,
                            Logo_emisor = Logo_emisor,
                            Regimen_emisor = Convert.ToDecimal(reader["emprperson"] ?? 0) == 1 ? "Natural" :
                                             Convert.ToDecimal(reader["emprperson"] ?? 0) == 2 ? "Jurídica" : ""

                        };
                        listaEmpresas.Add(empresa);
                    }

                    GuardarDatosEnArchivoTemp(listaEmpresas);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar los datos: " + ex.Message);
            }
        }

        private void GuardarDatosEnArchivoTemp(List<Emisor> listaEmpresas)
        {
            try
            {
                string json = JsonConvert.SerializeObject(listaEmpresas, Formatting.Indented);
                File.WriteAllText(tempFilePath, json);
                Console.WriteLine("Datos guardados en archivo temporal.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar los datos en archivo temporal: " + ex.Message);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            EliminarArchivoTemp();
        }

        private void EliminarArchivoTemp()
        {
            try
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                    Console.WriteLine("Archivo temporal eliminado.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el archivo temporal: " + ex.Message);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Muestra u oculta el texto del marcador de posición
            placeholderText.Visibility = string.IsNullOrWhiteSpace(searchTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;

            // Realiza la búsqueda de las empresas
            string searchText = searchTextBox.Text.ToLower();
            var filteredEmpresas = listaEmpresas.Where(Emisor =>
                (Emisor.Emprobra?.ToLower().Contains(searchText) ?? false) ||
                (Emisor.Nombre_emisor?.ToLower().Contains(searchText) ?? false)).ToList();

            // Actualiza la lista de resultados con el formato "Código - Nombre"
            if (filteredEmpresas.Any())
            {
                var formattedEmpresas = filteredEmpresas.Select(empresa => $"{empresa.Emprobra} - {empresa.Nombre_emisor}").ToList();
                resultsListBox.ItemsSource = formattedEmpresas;
                resultsListBox.Visibility = Visibility.Visible;
            }
            else
            {
                resultsListBox.ItemsSource = null;
                resultsListBox.Visibility = Visibility.Collapsed;
            }
        }

        private void ResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (resultsListBox.SelectedItem != null)
            {
                string selectedItem = resultsListBox.SelectedItem.ToString();
                string selectedCodigo = selectedItem.Split('-')[0].Trim();

                var selectedEmpresa = listaEmpresas.FirstOrDefault(empresa => empresa.Emprobra == selectedCodigo);
                if (selectedEmpresa != null)
                {
                    // Actualiza los TextBox con los datos de la empresa seleccionada
                    codigoTextBox.Text = selectedEmpresa.Emprobra;
                    nombreTextBox.Text = selectedEmpresa.Nombre_emisor;
                    nitTextBox.Text = selectedEmpresa.Nit_emisor;
                    direccionTextBox.Text = selectedEmpresa.Direccion_emisor;
                    telefonoTextBox.Text = selectedEmpresa.Telefono_emisor;
                    ciudadTextBox.Text = selectedEmpresa.Ciudad_emisor;
                    palabraTextBox.Text = selectedEmpresa.Url_emisor;
                    // Actualiza el TextBox de búsqueda con el nombre de la empresa seleccionada
                    searchTextBox.Text = $"{selectedEmpresa.Emprobra} - {selectedEmpresa.Nombre_emisor}";

                    // Oculta el ListBox
                    resultsListBox.Visibility = Visibility.Collapsed;

                    var consultas = new ConsultasBase();
                    consultas.ConsultarDatosFacturas(selectedEmpresa.Emprobra);

                    codigoEmpresaSeleccionada = selectedEmpresa.Emprobra;
                    facturaViewModel.CodigoEmpresaSeleccionada = selectedEmpresa.Emprobra;

                    // Carga las facturas en memoria
                    CargarFacturasEnMemoria();
                }
            }
        }



        private void BuscarFactura_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(codigoEmpresaSeleccionada))
            {
                string searchText = searchFactura.Text.Trim();
                BuscarYMostrarFacturas(searchText);
            }
            else
            {
                MessageBox.Show("Seleccione una empresa primero.");
            }
        }



        private void BuscarYMostrarFacturas(string searchText)
        {
            if (facturasEnMemoria == null)
            {
                CargarFacturasEnMemoria();
            }

            if (facturasEnMemoria.ContainsKey("fac"))
            {
                List<Dictionary<string, object>> resultadosFac;

                if (string.IsNullOrEmpty(searchText))
                {
                    resultadosFac = facturasEnMemoria["fac"];
                }
                else
                {
                    resultadosFac = facturasEnMemoria["fac"].Where(f => f.ContainsKey("factura") && f["factura"].ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (resultadosFac.Count > 0)
                {
                    var resultadosCompletos = new List<Factura>();

                    foreach (var fac in resultadosFac)
                    {
                        var factura = new Factura
                        {
                            FacturaId = fac.ContainsKey("factura") && fac["factura"] != null ? fac["factura"].ToString() : "",
                            Recibo = fac.ContainsKey("recibo") && fac["recibo"] != null ? fac["recibo"].ToString() : "",
                            Cliente = fac.ContainsKey("nombre3") && fac["nombre3"] != null ? fac["nombre3"].ToString() : "",
                            Estado = fac.ContainsKey("estado") && fac["estado"] != null ? fac["estado"].ToString() : "",
                            Error = fac.ContainsKey("msm_error") && fac["msm_error"] != null ? fac["msm_error"].ToString() : "",
                            Memo = fac.ContainsKey("dato_qr") && fac["dato_qr"] != null ? fac["dato_qr"].ToString() : "",
                            Terminal = fac.ContainsKey("terminal") && fac["terminal"] != null ? fac["terminal"].ToString() : "",
                            Tipo_movimiento = fac.ContainsKey("tipo_mvt") && fac["tipo_mvt"] != null ? fac["tipo_mvt"].ToString() : "",
                        };

                        // Determinar si es una devolución
                        bool esDevolucion = fac.ContainsKey("recibo") && !string.IsNullOrEmpty(fac["recibo"]?.ToString());

                        if (esDevolucion)
                        {
                            factura.Fecha = BuscarEnTabla("xxxxcmbt", "ffinal", fac["recibo"], "recibo")?.ToString() ?? "";
                            factura.Encabezado_NC = ContarEnTabla("xxxxcmbt", "recibo", fac["factura"]).ToString();
                        }
                        else
                        {
                            factura.Fecha = BuscarEnTabla("xxxxccfc", "fecha", fac["factura"], "factura")?.ToString() ?? "";
                            factura.Encabezado = ContarEnTabla("xxxxccfc", "factura", fac["factura"]).ToString(); 
                        }

                        factura.Articulos = ContarArticulosEnTabla("xxxxmvin", fac["factura"], "").ToString();
                        factura.Metodo = ContarEnTabla("xxxxccpg", "factura", fac["factura"]).ToString();

                        resultadosCompletos.Add(factura);
                    }

                    facturasListView.ItemsSource = resultadosCompletos;
                }
                else
                {
                    MessageBox.Show("No se encontraron facturas con el número especificado.");
                }
            }
            else
            {
                MessageBox.Show("La tabla 'fac' no está cargada.");
            }
        }



        private object BuscarEnTabla(string tabla, string columna, object valor, string nombre)
        {
            if (facturasEnMemoria.ContainsKey(tabla))
            {
                var registro = facturasEnMemoria[tabla].FirstOrDefault(f => f.ContainsKey(columna) && f[nombre]?.ToString() == valor?.ToString());
                if (registro != null && registro.ContainsKey(columna))
                {
                    Console.WriteLine($"Encontrado en {tabla}: {columna} = {registro[columna]}");
                    return registro[columna];
                }
            }
            Console.WriteLine($"No encontrado en {tabla}: {columna} para valor {valor}");
            return "";
        }

        private int ContarEnTabla(string tabla, string columna, object valorFactura)
        {
            if (facturasEnMemoria.ContainsKey(tabla))
            {
                var count = facturasEnMemoria[tabla].Count(f => f.ContainsKey("factura") && f["factura"]?.ToString() == valorFactura?.ToString());
                Console.WriteLine($"Contar en {tabla}: {count} registros para factura {valorFactura}");
                return count;
            }
            Console.WriteLine($"Tabla {tabla} no encontrada o sin registros para factura {valorFactura}");
            return 0;
        }

        private int ContarArticulosEnTabla(string tabla, object valorFactura, string recibo)
        {
            if (facturasEnMemoria.ContainsKey(tabla))
            {
                var count = facturasEnMemoria[tabla].Count(f => f.ContainsKey("factura") && f["factura"]?.ToString() == valorFactura?.ToString() && (string.IsNullOrEmpty(recibo) ? string.IsNullOrEmpty(f["recibo"]?.ToString()) : f["recibo"]?.ToString() == recibo));
                Console.WriteLine($"Contar artículos en {tabla}: {count} registros para factura {valorFactura} con recibo '{recibo}'");
                return count;
            }
            Console.WriteLine($"Tabla {tabla} no encontrada o sin registros para factura {valorFactura} con recibo '{recibo}'");
            return 0;
        }


        private void ConfigurarColumnas()
        {
            AgregarColumna("Factura", "FacturaId");
            AgregarColumna("Recibo", "Recibo");
            AgregarColumna("Cliente", "Cliente");
            AgregarColumna("Estado", "Estado");
            AgregarColumna("Msm Error", "Error");
            AgregarColumna("Memo", "Memo");
            AgregarColumna("Fecha", "Fecha");
            AgregarColumna("Encabezado", "Encabezado");
            AgregarColumna("Encabezado_NC", "Encabezado_NC");
            AgregarColumna("Articulos", "Articulos");
            AgregarColumna("Metodos Pago", "Metodo");
            AgregarColumnaAcciones(); 
        }

        private void AgregarColumna(string header, string bindingPath)
        {
            GridViewColumn column = new GridViewColumn
            {
                Header = header,
                DisplayMemberBinding = new Binding(bindingPath),
                Width = 100
            };

            // Asignar el evento de clic
            column.HeaderTemplate = CrearHeaderTemplate(header, bindingPath);

            facturasGridView.Columns.Add(column);
        }

        private void AgregarColumnaAcciones()
        {
            GridViewColumn accionesColumn = new GridViewColumn
            {
                Header = "Acciones",
                Width = 150,
                CellTemplate = CrearAccionesTemplate()
            };

            facturasGridView.Columns.Add(accionesColumn);
        }

        private DataTemplate CrearAccionesTemplate()
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            // Botón PDF
            FrameworkElementFactory pdfButtonFactory = new FrameworkElementFactory(typeof(Button));
            pdfButtonFactory.SetValue(Button.ContentProperty, "PDF");
            pdfButtonFactory.SetValue(Button.MarginProperty, new Thickness(5));
            pdfButtonFactory.SetBinding(Button.CommandProperty, new Binding("DataContext.ExportPdfCommand") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ListView), 1) });
            pdfButtonFactory.SetBinding(Button.CommandParameterProperty, new Binding("."));
            stackPanelFactory.AppendChild(pdfButtonFactory);

            // Botón XML
            FrameworkElementFactory xmlButtonFactory = new FrameworkElementFactory(typeof(Button));
            xmlButtonFactory.SetValue(Button.ContentProperty, "XML");
            xmlButtonFactory.SetValue(Button.MarginProperty, new Thickness(5));
            xmlButtonFactory.SetBinding(Button.CommandProperty, new Binding("DataContext.ExportXmlCommand") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ListView), 1) });
            xmlButtonFactory.SetBinding(Button.CommandParameterProperty, new Binding("."));
            stackPanelFactory.AppendChild(xmlButtonFactory);

            // Botón ENVIO
            FrameworkElementFactory envioButtonFactory = new FrameworkElementFactory(typeof(Button));
            envioButtonFactory.SetValue(Button.ContentProperty, "ENVIO");
            envioButtonFactory.SetValue(Button.MarginProperty, new Thickness(5));
            envioButtonFactory.SetBinding(Button.CommandProperty, new Binding("DataContext.SendEmailCommand") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ListView), 1) });
            envioButtonFactory.SetBinding(Button.CommandParameterProperty, new Binding("."));
            stackPanelFactory.AppendChild(envioButtonFactory);

            template.VisualTree = stackPanelFactory;
            return template;
        }

        private DataTemplate CrearHeaderTemplate(string header, string bindingPath)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetValue(TextBlock.TextProperty, header);
            factory.SetValue(TextBlock.TagProperty, bindingPath);
            factory.AddHandler(TextBlock.MouseLeftButtonUpEvent, new MouseButtonEventHandler(Header_Click));
            template.VisualTree = factory;
            return template;
        }

        private void Header_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                string sortBy = textBlock.Tag as string;
                if (!string.IsNullOrEmpty(sortBy))
                {
                    OrdenarPorColumna(sortBy);
                }
            }
        }

        private void OrdenarPorColumna(string sortBy)
        {
            if (facturasListView.ItemsSource is List<Factura> items)
            {
                var sortedItems = items.OrderBy(item =>
                {
                    var property = typeof(Factura).GetProperty(sortBy);
                    return property != null ? property.GetValue(item)?.ToString() : string.Empty;
                }).ToList();

                facturasListView.ItemsSource = sortedItems;
            }
        }




        private void facturasListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var columnHeader = VisualTreeHelper.GetParent(e.OriginalSource as DependencyObject) as GridViewColumnHeader;
            if (columnHeader != null)
            {
                string sortBy = columnHeader.Tag.ToString();
                SortListView(sortBy);
            }
        }

        private void SortListView(string sortBy)
        {
            if (facturasListView.ItemsSource is List<object> items)
            {
                // Ordenar los elementos de acuerdo al encabezado seleccionado
                var sortedItems = items.OrderBy(item =>
                {
                    var property = item.GetType().GetProperty(sortBy);
                    return property != null ? property.GetValue(item)?.ToString() : string.Empty;
                }).ToList();

                facturasListView.ItemsSource = sortedItems;
            }
        }



        private Dictionary<string, List<Dictionary<string, object>>> facturasEnMemoria;

        private void CargarFacturasEnMemoria()
        {
            try
            {
                string tempFilePathFacturas = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "facturas_temp.json");
                if (File.Exists(tempFilePathFacturas))
                {
                    string json = File.ReadAllText(tempFilePathFacturas);
                    facturasEnMemoria = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(json);
                }
                else
                {
                    MessageBox.Show("No se encontraron datos de facturas.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los datos de facturas: " + ex.Message);
            }
        }

        private void facturasListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (resolucionesWindow == null || !resolucionesWindow.IsVisible)
            {
                resolucionesWindow = new Resoluciones();
                resolucionesWindow.Show();
            }
            else
            {
                resolucionesWindow.Focus();
            }
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }
    }
}
