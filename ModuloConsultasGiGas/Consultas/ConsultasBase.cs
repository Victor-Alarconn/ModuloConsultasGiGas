using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModuloConsultasGiGas.Consultas
{
    public class ConsultasBase
    {
        public void ConsultarDatosFacturas(string codigoEmpresa)
        {
            try
            {
                string databaseName = "facturas";
                ModuloConsultasGiGas.Data.Conexion conexionBD = new ModuloConsultasGiGas.Data.Conexion(databaseName);
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    // Diccionario para almacenar los resultados agrupados por tabla
                    var resultadosPorTabla = new Dictionary<string, List<Dictionary<string, object>>>();

                    // Listado de tablas a consultar y las columnas correspondientes
                    var tablasYColumnas = new Dictionary<string, string>
                    {
                        { "fac", "empresa" },
                        { "xxxx3ros", "id_empresa" },
                        { "xxxxcmbt", "id_empresa" },
                        { "xxxxccfc", "id_empresa" },
                        { "xxxxterm", "empresa" },
                        { "xxxxccpg", "id_empresa" },
                        { "xxxxmvin", "id_empresa" }
                    };

                    foreach (var tablaYColumna in tablasYColumnas)
                    {
                        string tabla = tablaYColumna.Key;
                        string columna = tablaYColumna.Value;
                        string query = $"SELECT * FROM {tabla} WHERE {columna} = @CodigoEmpresa;";
                        MySqlCommand comando = new MySqlCommand(query, conexion);
                        comando.Parameters.AddWithValue("@CodigoEmpresa", codigoEmpresa);

                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var fila = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    fila[reader.GetName(i)] = reader.GetValue(i);
                                }

                                if (!resultadosPorTabla.ContainsKey(tabla))
                                {
                                    resultadosPorTabla[tabla] = new List<Dictionary<string, object>>();
                                }

                                resultadosPorTabla[tabla].Add(fila);
                            }
                        }
                    }

                    // Guarda los datos en un archivo temporal
                    GuardarDatosFacturasEnArchivoTemp(resultadosPorTabla);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar los datos de facturas: " + ex.Message);
            }
        }

        private void GuardarDatosFacturasEnArchivoTemp(Dictionary<string, List<Dictionary<string, object>>> resultadosPorTabla)
        {
            try
            {
                string tempFilePathFacturas = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "facturas_temp.json");
                string json = JsonConvert.SerializeObject(resultadosPorTabla, Formatting.Indented);
                File.WriteAllText(tempFilePathFacturas, json);
                Console.WriteLine("Datos de facturas guardados en archivo temporal.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar los datos de facturas en archivo temporal: " + ex.Message);
            }
        }

    }
}
