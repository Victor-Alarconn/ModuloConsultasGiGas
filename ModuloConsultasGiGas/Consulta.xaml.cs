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

namespace ModuloConsultasGiGas
{
    /// <summary>
    /// Lógica de interacción para Consulta.xaml
    /// </summary>
    public partial class Consulta : Window
    {
        private string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "empresas_temp.json");
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
                }
            }
        }


    }
}
