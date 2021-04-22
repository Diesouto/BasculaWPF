using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace PesajeWPF.Datos {
    public class Productos : ObservableCollection<Producto> { }
    public class Producto {
        public int Id { get; set; }
        public int IdTrabajo { get; set; }
        public int IdCaja { get; set; }
        public int Numero { get; set; }
        public decimal Peso { get; set; }
        public string Lote { get; set; }
        public DateTime Fecha { get; set; }
    }
}
