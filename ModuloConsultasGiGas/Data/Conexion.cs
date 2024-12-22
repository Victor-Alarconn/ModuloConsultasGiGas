using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuloConsultasGiGas.Data
{
    public class Conexion
    {
        private string connectionString;

        public Conexion(string databaseName)
        {
            connectionString = $"Data Source=192.190.42.191; Database={databaseName}; User Id=root; Password=**Adict057**; ConvertZeroDateTime=True;";
        }

        public MySqlConnection ObtenerConexion()
        {
            MySqlConnection conexion = new MySqlConnection(connectionString);
            try
            {
                conexion.Open();
                Console.WriteLine("Conexión establecida exitosamente.");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error al conectar a la base de datos: " + ex.Message);
            }

            return conexion;
        }

        public void CerrarConexion(MySqlConnection conexion)
        {
            if (conexion != null && conexion.State == System.Data.ConnectionState.Open)
            {
                conexion.Close();
                Console.WriteLine("Conexión cerrada exitosamente.");
            }
        }
    }
}
