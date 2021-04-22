using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FontAwesome.WPF;
using System.IO;
using System.IO.Ports;

namespace PesajeWPF {
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }
        SerialPort COMBascula;
         private void Window_Loaded(object sender, RoutedEventArgs e) {
            lblFecha.Content = DateTime.Today.ToShortDateString();
            lblCOM.Visibility = Visibility.Collapsed;
            COMBascula = new SerialPort(Configuracion.PuertoCOM);
            try {
                COMBascula.Open();
            } catch {
                lblCOM.Text = $"El puerto {Configuracion.PuertoCOM} no existe.";
                lblCOM.Visibility = Visibility.Visible;
            }
            if (COMBascula.IsOpen) {
                imgCOM.Fill = Brushes.Green;
            } else {
                imgCOM.Fill = Brushes.Red;
            }
        }
       private void btnExpand_Click(object sender, RoutedEventArgs e) {
            if (vbTrabajo.Stretch == Stretch.Fill) {
                vbTrabajo.Stretch = Stretch.Uniform;
                faExpand.Icon = FontAwesomeIcon.Expand;
            } else {
                vbTrabajo.Stretch = Stretch.Fill;
                faExpand.Icon = FontAwesomeIcon.Compress;
            }
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e) {

        }

        private void btnSalir_Click(object sender, RoutedEventArgs e) {
            Close();
        }

    }
}
