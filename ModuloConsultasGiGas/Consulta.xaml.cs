﻿using ModuloConsultasGiGas.Modelo;
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

namespace ModuloConsultasGiGas
{
    /// <summary>
    /// Lógica de interacción para Consulta.xaml
    /// </summary>
    public partial class Consulta : Window
    {
        private string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "empresas_temp.json");
        private string codigoEmpresaSeleccionada;
        private List<Empresa> listaEmpresas = new List<Empresa>();
        public Consulta()
        {
            InitializeComponent();
            ConsultarDatos("empresas");
        }

        private void ConsultarDatos(string databaseName)
        {
            try
            {
                ModuloConsultasGiGas.Data.Conexion conexionBD = new ModuloConsultasGiGas.Data.Conexion(databaseName);
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    string query = "SELECT emprobra, emprnombr, emprnit, emprdirec, emprciuda, emprtelef, empremail, empr_urlx FROM empresas";
                    MySqlCommand comando = new MySqlCommand(query, conexion);
                    MySqlDataReader reader = comando.ExecuteReader();

                    while (reader.Read())
                    {
                        Empresa empresa = new Empresa
                        {
                            Emprobra = reader["emprobra"]?.ToString() ?? string.Empty,
                            Emprnombr = reader["emprnombr"]?.ToString() ?? string.Empty,
                            Emprnit = reader["emprnit"]?.ToString() ?? string.Empty,
                            Emprdirec = reader["emprdirec"]?.ToString() ?? string.Empty,
                            Emprciuda = reader["emprciuda"]?.ToString() ?? string.Empty,
                            Emprtelef = reader["emprtelef"]?.ToString() ?? string.Empty,
                            Empremail = reader["empremail"]?.ToString() ?? string.Empty,
                            EmprUrlx = reader["empr_urlx"]?.ToString() ?? string.Empty
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

        private void GuardarDatosEnArchivoTemp(List<Empresa> listaEmpresas)
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
            var filteredEmpresas = listaEmpresas.Where(empresa =>
                (empresa.Emprobra?.ToLower().Contains(searchText) ?? false) ||
                (empresa.Emprnombr?.ToLower().Contains(searchText) ?? false)).ToList();

            // Actualiza la lista de resultados con el formato "Código - Nombre"
            if (filteredEmpresas.Any())
            {
                var formattedEmpresas = filteredEmpresas.Select(empresa => $"{empresa.Emprobra} - {empresa.Emprnombr}").ToList();
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
                    nombreTextBox.Text = selectedEmpresa.Emprnombr;
                    nitTextBox.Text = selectedEmpresa.Emprnit;
                    direccionTextBox.Text = selectedEmpresa.Emprdirec;
                    telefonoTextBox.Text = selectedEmpresa.Emprtelef;
                    ciudadTextBox.Text = selectedEmpresa.Emprciuda;
                    palabraTextBox.Text = selectedEmpresa.EmprUrlx;
                    // Actualiza el TextBox de búsqueda con el nombre de la empresa seleccionada
                    searchTextBox.Text = $"{selectedEmpresa.Emprobra} - {selectedEmpresa.Emprnombr}";

                    // Oculta el ListBox
                    resultsListBox.Visibility = Visibility.Collapsed;

                    var consultas = new ConsultasBase();
                    consultas.ConsultarDatosFacturas(selectedEmpresa.Emprobra);

                    codigoEmpresaSeleccionada = selectedEmpresa.Emprobra;

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

            // Mensaje de depuración para ver qué tablas están cargadas
            Console.WriteLine("Tablas cargadas:");
            foreach (var tabla in facturasEnMemoria.Keys)
            {
                Console.WriteLine(tabla);
            }

            // Buscar en la tabla 'fac' primero
            if (facturasEnMemoria.ContainsKey("fac"))
            {
                var resultadosFac = facturasEnMemoria["fac"].Where(f => f.ContainsKey("factura") && f["factura"].ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

                // Mensaje de depuración para ver el número de resultados encontrados
                Console.WriteLine("Resultados encontrados en 'fac': " + resultadosFac.Count);

                if (resultadosFac.Count > 0)
                {
                    var resultadosCompletos = new List<Dictionary<string, object>>();

                    foreach (var fac in resultadosFac)
                    {
                        var resultado = new Dictionary<string, object>(fac);

                        // Buscar en las demás tablas
                        resultado["Fecha"] = BuscarEnTabla("xxxxccfc", "fecha", fac["factura"]);
                        resultado["Encabezado"] = BuscarEnTabla("xxxxccfc", "factura", fac["factura"]);
                        resultado["Encabezado_NC"] = BuscarEnTabla("xxxxcmbt", "recibo", fac["factura"]);
                        resultado["Articulos"] = ContarArticulosEnTabla("xxxxmvin", fac["factura"], "");
                        resultado["Metodo"] = ContarEnTabla("xxxxccpg", "factura", fac["factura"]);

                        resultadosCompletos.Add(resultado);
                    }

                    facturasListView.ItemsSource = resultadosCompletos.Select(r => new
                    {
                        Factura = r.ContainsKey("factura") ? r["factura"] : "",
                        Recibo = r.ContainsKey("recibo") ? r["recibo"] : "",
                        Cliente = r.ContainsKey("nombre3") ? r["nombre3"] : "",
                        Estado = r.ContainsKey("estado") ? r["estado"] : "",
                        Error = r.ContainsKey("msm_error") ? r["msm_error"] : "",
                        Memo = r.ContainsKey("dato_qr") ? r["dato_qr"] : "",
                        Fecha = r.ContainsKey("Fecha") ? r["Fecha"] : "",
                        Encabezado = r.ContainsKey("Encabezado") ? r["Encabezado"] : "",
                        Encabezado_NC = r.ContainsKey("Encabezado_NC") ? r["Encabezado_NC"] : "",
                        Articulos = r.ContainsKey("Articulos") ? r["Articulos"] : "",
                        Metodo = r.ContainsKey("Metodo") ? r["Metodo"] : ""
                    }).ToList();
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





        private object BuscarEnTabla(string tabla, string columna, object valor)
        {
            if (facturasEnMemoria.ContainsKey(tabla))
            {
                var registro = facturasEnMemoria[tabla].FirstOrDefault(f => f.ContainsKey(columna) && f[columna].ToString() == valor.ToString());
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
                var count = facturasEnMemoria[tabla].Count(f => f.ContainsKey("factura") && f["factura"].ToString() == valorFactura.ToString());
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
                var count = facturasEnMemoria[tabla].Count(f => f.ContainsKey("factura") && f["factura"].ToString() == valorFactura.ToString() && (string.IsNullOrEmpty(recibo) ? string.IsNullOrEmpty(f["recibo"].ToString()) : f["recibo"].ToString() == recibo));
                Console.WriteLine($"Contar artículos en {tabla}: {count} registros para factura {valorFactura} con recibo '{recibo}'");
                return count;
            }
            Console.WriteLine($"Tabla {tabla} no encontrada o sin registros para factura {valorFactura} con recibo '{recibo}'");
            return 0;
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
    }
}
