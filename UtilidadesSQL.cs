using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace PesajeWPF {
    public class UtilidadesSQL {
        public static string ObtenerCadena(DataRow row, string campo) {
            string cadena = string.Empty;
            if (row.Table.Columns.Cast<DataColumn>().Select(n=>n.ColumnName.ToLower()).Contains(campo.ToLower())) {
                cadena = row[campo].ToString();
            }
            return cadena;
        }
        public static int ObtenerEntero(DataRow row, string campo) {
            int numero; int.TryParse(ObtenerCadena(row, campo), out numero);
            return numero;
        }
        public static decimal ObtenerDecimal(DataRow row, string campo) {
            decimal numero; decimal.TryParse(ObtenerCadena(row, campo), out numero);
            return numero;
        }
        public static DateTime ObtenerFecha(DataRow row, string campo) {
            DateTime fecha; DateTime.TryParse(ObtenerCadena(row, campo), out fecha);
            return fecha;
        }
        public static bool ObtenerBooleano(DataRow row, string campo) {
            bool valor; bool.TryParse(ObtenerCadena(row, campo), out valor);
            return valor;
        }
    }
}
