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
            // Mensaje de prueba para el botón PDF
            MessageBox.Show("Botón PDF presionado para la factura: " + factura.FacturaId);
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
