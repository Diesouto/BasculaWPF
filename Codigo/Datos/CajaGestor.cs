using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace PesajeWPF.Datos {
    public class CajaGestor {
        public static Cajas ObtenerCajas(int IdTrabajo) {
            Cajas colEntidad = new Cajas();
            try {
                using(SqlCommand cmd = new SqlCommand("Select * from Cajas where Idtrabajo = @IdTrabajo order by Numero", Configuracion.ConexionBBDD)) {
                    cmd.Parameters.Add("IdTrabajo", SqlDbType.Int).Value = IdTrabajo;
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    foreach(DataRow dr in dt.Rows) {
                        colEntidad.Add(new Caja() {
                            Id = UtilidadesSQL.ObtenerEntero(dr, "Id"),
                            IdTrabajo = UtilidadesSQL.ObtenerEntero(dr, "IdTrabajo"),
                            Numero = UtilidadesSQL.ObtenerEntero(dr, "Numero"),
                            Peso = UtilidadesSQL.ObtenerDecimal(dr, "Peso"),
                            Lote = UtilidadesSQL.ObtenerCadena(dr, "Lote"),
                            Fecha = UtilidadesSQL.ObtenerFecha(dr, "Fecha")
                        });
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return colEntidad;
        }
        public static int GuardarCaja(Caja caja) {
            int id = 0;
            try {
                string qInsert = @"
                    Insert into Cajas (IdTrabajo, Numero, Peso, Lote, Fecha)
                    values (@IdTrabajo, Isnull((Select max(numero) from Cajas where idtrabajo = @idTrabajo), 0)+1, @Peso, @Lote, @Fecha)

                    Select SCOPE_IDENTITY()
                ";
                using(SqlCommand cmd = new SqlCommand(qInsert, Configuracion.ConexionBBDD)) {
                    cmd.Parameters.Add("IdTrabajo", SqlDbType.Int).Value = caja.IdTrabajo;
                    cmd.Parameters.Add("@Peso", SqlDbType.Decimal).Value = caja.Peso;
                    cmd.Parameters.Add("@Lote", SqlDbType.VarChar).Value = caja.Lote;
                    cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = caja.Fecha;
                    var result = cmd.ExecuteScalar();
                    int.TryParse(result.ToString(), out id);
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return id;
        }
        public static bool EliminaCaja(int IdCaja) {
            bool eliminada = false;
            try {
                string qDelete = @"
                Delete from Productos where Idcaja = @IdCaja;
                Delete from Cajas where Id = @IdCaja
                ";
                using(SqlCommand cmd = new SqlCommand(qDelete, Configuracion.ConexionBBDD)) {
                    cmd.Parameters.Add("IdCaja", SqlDbType.Int).Value = IdCaja;
                    int result = cmd.ExecuteNonQuery();
                    eliminada = result > 0;
                }
            } catch (Exception ex) {
                Console.Write(ex.Message);
            }
            return eliminada;
        }
    }
}
