using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace PesajeWPF.Datos {
    public class PuestoGestor {
        public static Puestos ObtenerPuestos() {
            Puestos colEntidad = new Puestos();
            try {
                using (SqlCommand cmd = new SqlCommand("Select * from Puestos order by Equipo", Configuracion.ConexionBBDD)) {
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    foreach (DataRow dr in dt.Rows) {
                        colEntidad.Add(ObtenerEntidadPuesto(dr));
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return colEntidad;
        }
        public static Puesto ObtenerPuesto(string Equipo) {
            Puesto oEntidad = null;
            try {
                using (SqlCommand cmd = new SqlCommand("Select * from Puestos where Equipo = @Equipo order by Equipo", Configuracion.ConexionBBDD)) {
                    cmd.Parameters.Add("Equipo", SqlDbType.VarChar).Value = Equipo;
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0) {
                        oEntidad=ObtenerEntidadPuesto(dt.Rows[0]);
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return oEntidad;
        }

        public static bool GuardarPuesto(Puesto Puesto) {
            bool guardado = false;
            try {
                string qUpdateInsert = @"
                    Declare @Id as int = (Select Id from Puestos where Equipo = @Equipo) -- and IP = @IP)
                    if @Id is not null
                        Update PuestoS set Token = @Licencia, IP = @IP where Id = @Id
                    else
                        Insert into Puestos (Equipo, IP, Token) values(@Equipo, @IP, @Licencia)
                ";
                using(SqlCommand cmd = new SqlCommand(qUpdateInsert, Configuracion.ConexionBBDD)) {
                    cmd.Parameters.Add("IP", SqlDbType.VarChar).Value = Puesto.IP;
                    cmd.Parameters.Add("Licencia", SqlDbType.Text).Value = Puesto.Token;
                    cmd.Parameters.Add("Equipo", SqlDbType.VarChar).Value = Puesto.Equipo;
                    int result = cmd.ExecuteNonQuery();
                    guardado = result > 0;
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return guardado;
        }
        /// <summary>
        /// Elimina los puestos no licenciados
        /// </summary>
        /// <param name="NumeroLicencias">Indica el número de puestos licenciados</param>
        public static void EliminaPuestosNoLicenciados(int NumeroLicencias) {
            string qDelete = $"delete from Puestos where id not in (select top {NumeroLicencias} id from Puestos order by Fecha desc)";
            try {
                using (SqlCommand cmd = new SqlCommand(qDelete, Configuracion.ConexionBBDD)) {
                    cmd.ExecuteNonQuery();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }
        private static Puesto ObtenerEntidadPuesto(DataRow dr) {
            return new Puesto() {
                Id = Convert.ToInt32(dr["Id"].ToString()),
                Equipo = dr["Equipo"].ToString(),
                IP = dr["IP"].ToString(),
                Token = dr["Token"].ToString()
            };
        }
    }
}
