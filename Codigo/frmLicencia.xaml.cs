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

namespace PesajeWPF {
    /// <summary>
    /// Lógica de interacción para frmLicencia.xaml
    /// </summary>
    public partial class frmLicencia : Window {
        public frmLicencia() {
            InitializeComponent();
            pnlMensaje.Height = new GridLength(0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            lblEquipo.Content = Configuracion.NombreEquipo;
            lblIP.Content = Configuracion.IPEquipo;
            txtLicencia.Text = string.Empty;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(txtLicencia.Text)) {
                return;
            } else {
                // Guardar licencia del equipo
                Datos.Puesto oPuesto = new Datos.Puesto() {
                    Equipo = lblEquipo.Content.ToString(),
                    IP = lblIP.Content.ToString(),
                    Token = txtLicencia.Text
                };
                if (Datos.PuestoGestor.GuardarPuesto(oPuesto)) {
                    Siscom.Seguridad seg = new Siscom.Seguridad(App.Programa);
                    Siscom.LicenciaIP lic = seg.CompruebaLicenciaIP(txtLicencia.Text, App.Modulo);
                    if (lic != null && !string.IsNullOrEmpty(lic.Equipo)) {
                        // Actualizar los puestos licenciados
                        Datos.PuestoGestor.EliminaPuestosNoLicenciados(lic.NumeroPuestos);
                    }
                    Close();
                } else {
                    lblMensaje.Content = "Error al guardar";
                    pnlMensaje.Height = new GridLength(50);
                }
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
