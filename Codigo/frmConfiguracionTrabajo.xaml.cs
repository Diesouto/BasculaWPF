using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Drawing.Printing;
using Microsoft.Win32;
using System.IO;
using PesajeWPF.Datos;

namespace PesajeWPF {
    /// <summary>
    /// Lógica de interacción para frmConfiguracionTrabajo.xaml
    /// </summary>
    public partial class frmConfiguracionTrabajo : Window {
        public int CopiasProducto = 1;
        public int CopiasCaja = 1;
        public int CopiasTotal = 1;
        public int ProductosParaCierre = 0;
        private string RutaEtiquetas = string.Empty;
        private Trabajo TrabajoActivo;
        public frmConfiguracionTrabajo() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            dtFechaEnvasado.SelectedDate = DateTime.Today;
            txtCopiasProducto.Text = CopiasProducto.ToString();
            txtCopiasCaja.Text = CopiasCaja.ToString();
            txtCopiasTotal.Text = CopiasTotal.ToString();
            txtCierre.Text = ProductosParaCierre.ToString();

            PrinterSettings.StringCollection lstImpresoras = PrinterSettings.InstalledPrinters;
            cmbImpresoraProducto.ItemsSource = lstImpresoras;
            cmbImpresoraCaja.ItemsSource = lstImpresoras;
            cmbImpresoraTotal.ItemsSource = lstImpresoras;

            PrinterSettings ps = new PrinterSettings();
            cmbImpresoraProducto.SelectedItem = ps.PrinterName;
            cmbImpresoraCaja.SelectedItem = ps.PrinterName;
            cmbImpresoraTotal.SelectedItem = ps.PrinterName;

            TrabajoActivo = TrabajoGestor.ObtenerTrabajoActivo(Configuracion.NombreEquipo);
            if (TrabajoActivo != null) {
                txtLote.Text = TrabajoActivo.Lote;
                dtFechaEnvasado.SelectedDate = TrabajoActivo.Fecha;
                if (!string.IsNullOrEmpty(TrabajoActivo.EtiquetaProducto)) RutaEtiquetas = System.IO.Path.GetDirectoryName(TrabajoActivo.EtiquetaProducto);
                if (!string.IsNullOrEmpty(TrabajoActivo.EtiquetaCaja)) RutaEtiquetas = System.IO.Path.GetDirectoryName(TrabajoActivo.EtiquetaCaja);
                if (!string.IsNullOrEmpty(TrabajoActivo.EtiquetaTotal)) RutaEtiquetas = System.IO.Path.GetDirectoryName(TrabajoActivo.EtiquetaTotal);
                txtEtiquetaProducto.Text = System.IO.Path.GetFileName(TrabajoActivo.EtiquetaProducto);
                txtEtiquetaCaja.Text = System.IO.Path.GetFileName(TrabajoActivo.EtiquetaCaja);
                txtEtiquetaTotal.Text = System.IO.Path.GetFileName(TrabajoActivo.EtiquetaTotal);
                if (lstImpresoras.Cast<string>().ToList().Contains(TrabajoActivo.ImpresoraProducto)) cmbImpresoraProducto.SelectedItem = TrabajoActivo.ImpresoraProducto;
                if (lstImpresoras.Cast<string>().ToList().Contains(TrabajoActivo.ImpresoraCaja)) cmbImpresoraCaja.SelectedItem = TrabajoActivo.ImpresoraCaja;
                if (lstImpresoras.Cast<string>().ToList().Contains(TrabajoActivo.ImpresoraTotal)) cmbImpresoraTotal.SelectedItem = TrabajoActivo.ImpresoraTotal;
                txtCopiasProducto.Text = TrabajoActivo.CopiasProducto.ToString();
                CopiasProducto = TrabajoActivo.CopiasProducto;
                CopiasCaja = TrabajoActivo.CopiasCaja;
                CopiasTotal = TrabajoActivo.CopiasTotal;
                ProductosParaCierre = TrabajoActivo.NumeroProductosCierre;
                txtCopiasCaja.Text = TrabajoActivo.CopiasCaja.ToString();
                txtCopiasTotal.Text = TrabajoActivo.CopiasTotal.ToString();
                txtCierre.Text = TrabajoActivo.NumeroProductosCierre.ToString();
                // Sólo dejar cambiar las impresoras, las copias y el número de productos para el cierre de caja automático.
                txtLote.IsReadOnly = true;
                btnLimpiaLote.IsEnabled = false;
                btnLimpiaProducto.IsEnabled = false;
                btnLimpiaCaja.IsEnabled = false;
                btnLimpiaTotal.IsEnabled = false;
                btnTecladoLote.IsEnabled = false;
                btnBuscaEtiquetaProducto.IsEnabled = false;
                btnBuscaEtiquetaCaja.IsEnabled = false;
                btnBuscaEtiquetaTotal.IsEnabled = false;
                dtFechaEnvasado.IsEnabled = false;

            } else {
                TrabajoActivo = new Trabajo();
            }
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e) {
            // No dejar guardar si no hay datos
            if (string.IsNullOrEmpty(txtLote.Text))
            {
                MessageBox.Show("No se ha establecido el Lote.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            txtLote.Text = txtLote.Text.ToUpper();
            if (string.IsNullOrEmpty(txtEtiquetaProducto.Text) && string.IsNullOrEmpty(txtEtiquetaCaja.Text))
            {
                MessageBox.Show("No se ha establecido la etiqueta de Producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Comprobar si existen los archivos de etiquetas ¿?

            if (string.IsNullOrEmpty(txtEtiquetaTotal.Text)) {
                MessageBox.Show("No se ha establecido la etiqueta total. El programa continuará igualmente.", "Etiqueta Total", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            TrabajoActivo.Lote = txtLote.Text;
            TrabajoActivo.Fecha = dtFechaEnvasado.DisplayDate;
            TrabajoActivo.EtiquetaProducto = string.IsNullOrEmpty(txtEtiquetaProducto.Text) ? string.Empty : System.IO.Path.Combine(RutaEtiquetas, txtEtiquetaProducto.Text);
            TrabajoActivo.EtiquetaCaja = string.IsNullOrEmpty(txtEtiquetaCaja.Text) ? string.Empty : System.IO.Path.Combine(RutaEtiquetas, txtEtiquetaCaja.Text);
            TrabajoActivo.EtiquetaTotal = string.IsNullOrEmpty(txtEtiquetaTotal.Text) ? string.Empty : System.IO.Path.Combine(RutaEtiquetas, txtEtiquetaTotal.Text);
            TrabajoActivo.CopiasProducto = CopiasProducto;
            TrabajoActivo.CopiasCaja = CopiasCaja;
            TrabajoActivo.CopiasTotal = CopiasTotal;
            TrabajoActivo.ImpresoraProducto = cmbImpresoraProducto.SelectedItem.ToString();
            TrabajoActivo.ImpresoraCaja = cmbImpresoraCaja.SelectedItem.ToString();
            TrabajoActivo.ImpresoraTotal = cmbImpresoraTotal.SelectedItem.ToString();
            TrabajoActivo.NumeroProductosCierre = ProductosParaCierre;

            if (TrabajoGestor.GuardarTrabajo(TrabajoActivo) > 0) {
                DialogResult = true;
            } else {
                DialogResult = false;
            }
            Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void btnLimpiaLote_Click(object sender, RoutedEventArgs e) {
            txtLote.Text = string.Empty;
            txtLote.Focus();
        }

        private void btnTecladoLote_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("osk.exe");
            txtLote.SelectAll();
            txtLote.Focus();
        }

        private void btnBuscaEtiqueta_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openMOF = new OpenFileDialog();
            openMOF.DefaultExt = "*.MOF";
            openMOF.Filter = "Archivos de configuración de etiquetas (*.MOF)|*.MOF";
            openMOF.InitialDirectory = Configuracion.DirectorioEtiquetas;
            if (openMOF.ShowDialog() == true) {
                string filename = openMOF.FileName;
                RutaEtiquetas = System.IO.Path.GetDirectoryName(filename);
                string strmof = File.ReadAllText(filename);
                List<string> eti = strmof.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
                if (eti.Count >= 4) {
                    txtEtiquetaProducto.Text = eti[0];
                    txtEtiquetaCaja.Text = eti[1];
                    txtEtiquetaTotal.Text = eti[2];
                    int.TryParse(eti[3], out ProductosParaCierre);
                    txtCierre.Text = ProductosParaCierre.ToString();

                    // COMPROBAR SI EXISTEN LOS ARCHIVOS DE ETIQUETAS
                    txtEtiquetaProducto.BorderBrush = Brushes.Navy;
                    txtEtiquetaCaja.BorderBrush = Brushes.Navy;
                    txtEtiquetaTotal.BorderBrush = Brushes.Navy;
                    if (!File.Exists(Path.Combine(RutaEtiquetas,txtEtiquetaProducto.Text))) txtEtiquetaProducto.BorderBrush = Brushes.Red;
                    if (!File.Exists(Path.Combine(RutaEtiquetas,txtEtiquetaCaja.Text))) txtEtiquetaCaja.BorderBrush = Brushes.Red;
                    if (!File.Exists(Path.Combine(RutaEtiquetas,txtEtiquetaTotal.Text))) txtEtiquetaTotal.BorderBrush = Brushes.Red;
                }
            }
        }

        private void btnLimpiaProducto_Click(object sender, RoutedEventArgs e) {
            txtEtiquetaProducto.Text = string.Empty;
            txtEtiquetaProducto.BorderBrush = Brushes.Navy;
        }

        private void btnRestaCopiasProducto_Click(object sender, RoutedEventArgs e) {
            if (CopiasProducto > 1) --CopiasProducto;
            txtCopiasProducto.Text = CopiasProducto.ToString();
        }

        private void btnSumaCopiasProducto_Click(object sender, RoutedEventArgs e) {
            if (CopiasProducto < 100) ++CopiasProducto;
            txtCopiasProducto.Text = CopiasProducto.ToString();
        }

        private void btnLimpiaCaja_Click(object sender, RoutedEventArgs e) {
            txtEtiquetaCaja.Text = string.Empty;
            txtEtiquetaCaja.BorderBrush = Brushes.Navy;
        }

        private void btnRestaCopiasCaja_Click(object sender, RoutedEventArgs e) {
            if (CopiasCaja > 1) --CopiasCaja;
            txtCopiasCaja.Text = CopiasCaja.ToString();
        }

        private void btnSumaCopiasCaja_Click(object sender, RoutedEventArgs e) {
            if (CopiasCaja < 100) ++CopiasCaja;
            txtCopiasCaja.Text = CopiasCaja.ToString();
        }

        private void btnLimpiaTotal_Click(object sender, RoutedEventArgs e) {
            txtEtiquetaTotal.Text = string.Empty;
            txtEtiquetaTotal.BorderBrush = Brushes.Navy;
        }

        private void btnRestaCopiasTotal_Click(object sender, RoutedEventArgs e) {
            if (CopiasTotal > 1) --CopiasTotal;
            txtCopiasTotal.Text = CopiasTotal.ToString();
        }

        private void btnSumaCopiasTotal_Click(object sender, RoutedEventArgs e) {
            if (CopiasTotal < 100) ++CopiasTotal;
            txtCopiasTotal.Text = CopiasTotal.ToString();
        }

        private void btnRestaCierre_Click(object sender, RoutedEventArgs e) {
            if (ProductosParaCierre > 0) --ProductosParaCierre;
            txtCierre.Text = ProductosParaCierre.ToString();
        }

        private void btnSumaCierre_Click(object sender, RoutedEventArgs e) {
            if (ProductosParaCierre < 100) ++ProductosParaCierre;
            txtCierre.Text = ProductosParaCierre.ToString();
        }
    }
}