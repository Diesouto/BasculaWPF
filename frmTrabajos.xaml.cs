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
using System.Windows.Shapes;
using PesajeWPF.Datos;

namespace PesajeWPF {
    /// <summary>
    /// Lógica de interacción para frmTrabajos.xaml
    /// </summary>
    public partial class frmTrabajos : Window {
        private  Trabajos oTrabajos = new Trabajos();
        public Trabajo TrabajoSeleccionado { get; set; }
        public frmTrabajos() {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            oTrabajos = TrabajoGestor.ObtenerTrabajos(Configuracion.NombreEquipo);
            lvTrabajos.ItemsSource = oTrabajos;
            TrabajoSeleccionado = null;
        }

        private void btnSeleccionar_Click(object sender, RoutedEventArgs e) {
            if (lvTrabajos.SelectedItem != null) {
                TrabajoSeleccionado = lvTrabajos.SelectedItem as Trabajo;
                DialogResult = true;
                Close();
            }
        }

        private void btnSalir_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}
