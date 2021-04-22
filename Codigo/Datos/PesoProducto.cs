using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PesajeWPF.Datos {
    public class PesosProductos : ObservableCollection<PesoProducto> { }
    public class PesoProducto {
        public int Id { get; set; }
        public int IdTrabajo { get; set; }
        public int IdCaja { get; set; }
        public int Numero  { get; set; }
        public string Lote { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Peso { get; set; }
    }
}
