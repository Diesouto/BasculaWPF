using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PesajeWPF.Datos {
    public class Trabajos: ObservableCollection<Trabajo> { }
    public class Trabajo {
        public int Id { get; set; }
        public string Equipo { get; set; }
        public string Lote { get; set; }
        public DateTime Fecha { get; set; }
        public string EtiquetaProducto { get; set; }
        public string EtiquetaCaja { get; set; }
        public string EtiquetaTotal { get; set; }
        public string ImpresoraProducto { get; set; }
        public string ImpresoraCaja { get; set; }
        public string ImpresoraTotal { get; set; }
        public int CopiasProducto { get; set; }
        public int CopiasCaja { get; set; }
        public int CopiasTotal { get; set; }
        public int NumeroProductosCierre { get; set; }
        public bool Finalizado { get; set; }
        public int TotalCajas { get; set; }
        public decimal TotalKilos { get; set; }
        public Trabajo() {
            Id = 0;
            Equipo = Configuracion.NombreEquipo;
            Lote = string.Empty;
            Fecha = DateTime.Today;
            CopiasProducto = 1;CopiasCaja = 1;CopiasTotal = 1;
            NumeroProductosCierre = 0;
            Finalizado = false;
        }
    }
}
