using System;
using System.Collections.Generic;

namespace PesajeWPF.Datos {
    public class Puestos : List<Puesto> { }
    public class Puesto {
        public int Id { get; set; }
        public string Equipo { get; set; }
        public string IP { get; set; }
        public string Token { get; set; }

    }
}
