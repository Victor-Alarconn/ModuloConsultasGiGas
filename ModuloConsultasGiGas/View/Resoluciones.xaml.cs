using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

namespace ModuloConsultasGiGas.View
{
    public partial class Resoluciones : Window
    {
        private static readonly HttpClient client = new HttpClient();

        public Resoluciones()
        {
            InitializeComponent();
        }

        private void SearchResolucion_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Mostrar u ocultar el placeholder según el contenido del TextBox
            if (searchResolucion.Text.Length > 0)
            {
                placeholderResolucion.Visibility = Visibility.Collapsed;
            }
            else
            {
                placeholderResolucion.Visibility = Visibility.Visible;
            }
        }

        private async void SearchResolucion_KeyDown(object sender, KeyEventArgs e)
        {
            // Realizar la búsqueda cuando se presione la tecla Enter
            if (e.Key == Key.Enter)
            {
                e.Handled = true;  // Evita el sonido de 'ding' al presionar Enter

                if (searchResolucion.Text.Length > 0)
                {
                    await BuscarResolucion(searchResolucion.Text);
                }
                else
                {
                    responseTextBox.Text = string.Empty;
                }
            }
        }

        private async Task BuscarResolucion(string numeroCedula)
        {
            string softwareCode = "49fab599-4556-4828-a30b-852a910c5bb1";
            string accountCodeVendor = "890930534";
            string accountCode = numeroCedula;
            string url = $"https://apivp.efacturacadena.com/v1/vp/consulta/rango-numeracion?softwareCode={softwareCode}&accountCodeVendor={accountCodeVendor}&accountCode={accountCode}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                // Formatear el JSON
                var formattedJson = JsonConvert.SerializeObject(
                    JsonConvert.DeserializeObject(responseBody),
                    Formatting.Indented);

                responseTextBox.Text = formattedJson;
            }
            catch (HttpRequestException ex)
            {
                responseTextBox.Text = $"Error en la solicitud: {ex.Message}";
            }
        }
    }
}
