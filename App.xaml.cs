using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Siscom;

namespace PesajeWPF {
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application {
        public const string Programa = "PesajeWPF";
        public const string Modulo = "LicenciaIP";
        public Seguridad seg = new Seguridad(Programa);
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            //MainWindow frm = new PesajeWPF.MainWindow();
            if (LicenciaValida()) {
                frmPesadas frm = new PesajeWPF.frmPesadas();
                bool startMinimized = false;
                for (int i = 0; i != e.Args.Length; ++i) {
                    if (e.Args[i] == "/StartMinimized") {
                        startMinimized = true;
                    }
                }
                if (startMinimized) {
                    frm.WindowState = WindowState.Minimized;
                }
                frm.Show();
            } else {
                // Pedir en un formulario la licencia nueva
                frmLicencia frm = new frmLicencia();
                frm.Show();
            }
        }

        private bool LicenciaValida() {
            bool bValida = false;
            Seguridad seg = new Seguridad(Programa);
            // El mismo puesto puede tener varias direcciones IP ¿?
            Datos.Puesto oPuesto = Datos.PuestoGestor.ObtenerPuesto(Configuracion.NombreEquipo);
            if (oPuesto != null) {
                LicenciaIP l = seg.CompruebaLicenciaIP(oPuesto.Token, Modulo);
                if (l.EsValida) {
                    if (l.Equipo.ToLower() == Configuracion.NombreEquipo.ToLower() && l.IP == Configuracion.IPEquipo) {
                        bValida = true;
                    }
                }
            }

            return bValida;
        }
    }
}
