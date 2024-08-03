using Microsoft.Win32;
using ModuloConsultasGiGas.Modelo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ModuloConsultasGiGas.Model
{
    public class FacturaViewModel : INotifyPropertyChanged
    {
        public ICommand ExportPdfCommand { get; private set; }
        public ICommand ExportXmlCommand { get; private set; }
        public ICommand SendEmailCommand { get; private set; }

        public FacturaViewModel()
        {
            ExportPdfCommand = new RelayCommand<Factura>(ExportPdf);
            ExportXmlCommand = new RelayCommand<Factura>(ExportXml);
            SendEmailCommand = new RelayCommand<Factura>(SendEmail);
        }

        private void ExportPdf(Factura factura)
        {
            // Crear y configurar el SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
            saveFileDialog.Title = "Guardar archivo PDF";
            saveFileDialog.FileName = "Factura_" + factura.FacturaId + ".pdf";

            // Mostrar el diálogo al usuario
            if (saveFileDialog.ShowDialog() == true)
            {
                string rutaArchivo = saveFileDialog.FileName;

                // Aquí puedes definir los parámetros que necesitas para crear el PDF
                Emisor emisor = ObtenerEmisor(); // Método para obtener el emisor
                List<Productos> listaProductos = ObtenerProductos(factura.FacturaId); // Método para obtener la lista de productos
                string cufe = ObtenerCufe(factura.FacturaId); // Método para obtener el CUFE
                Adquiriente adquiriente = ObtenerAdquiriente(factura.FacturaId); // Método para obtener el adquiriente
                Movimiento movimiento = ObtenerMovimiento(factura.FacturaId); // Método para obtener el movimiento
                Encabezado encabezado1 = ObtenerEncabezado(factura.FacturaId); // Método para obtener el encabezado
                List<FormaPago> listaFormaPago = ObtenerFormasPago(factura.FacturaId); // Método para obtener las formas de pago

                // Llamar al método CrearPDF de la clase GenerarPDF
                ModuloConsultasGiGas.VewModel.GenerarPDF.CrearPDF(rutaArchivo, emisor, factura, listaProductos, cufe, adquiriente, movimiento, encabezado1, listaFormaPago);

                // Mensaje de prueba para el botón PDF
                MessageBox.Show("El PDF ha sido guardado en: " + rutaArchivo);
            }
            else
            {
                // El usuario canceló la operación
                MessageBox.Show("Guardado cancelado.");
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
