using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace PesajeWPF.Datos {
    public class TrabajoGestor {
        public static Trabajos ObtenerTrabajos(string Equipo) {
            Trabajos colEntidad = new Trabajos();

            string qSelect = "Select * from Trabajos (nolock) where Equipo = @Equipo order by Id desc";
            try {
                using (SqlCommand cmd = new SqlCommand(qSelect, Configuracion.ConexionBBDD)) {
                    cmd.Parameters.Add("Equipo", SqlDbType.VarChar).Value = Equipo;
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    foreach (DataRow dr in dt.Rows) {
                        colEntidad.Add(ObtenerEntidadTrabajo(dr));
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            return colEntidad;
        }
        public static Trabajo ObtenerTrabajo(int Id) {
            Trabajo oEntidad = null;

            string qSelect = "Select * from Trabajos (nolock) where Id = @Id";
            try {
                using (SqlCommand cmd = new SqlCommand(qSelect, Configuracion.ConexionBBDD)) {
                    cmd.Parameters.Add("Id", SqlDbType.Int).Value = Id;
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if  (dt.Rows.Count > 0) {
                        oEntidad = ObtenerEntidadTrabajo(dt.Rows[0]);
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            return oEntidad;
        }
        public static Trabajo ObtenerTrabajoActivo(string Equipo) {
            Trabajo oEntidad = null;

            string qSelect = "Select * from Trabajos (nolock) where Equipo = @Equipo and Finalizado = 0";
            try {
                using (SqlCommand cmd = new SqlCommand(qSelect, Configuracion.ConexionBBDD)) {
                    cmd.Parameters.Add("Equipo", SqlDbType.VarChar).Value = Equipo;
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    if  (dt.Rows.Count > 0) {
                        oEntidad = ObtenerEntidadTrabajo(dt.Rows[0]);
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            return oEntidad;
        }
        public static int GuardarTrabajo(Trabajo oTrabajo) {
            int id = oTrabajo.Id;

            try {
                string qInsert = @"
                    if @Id = 0
                    BEGIN
	                    Insert into Trabajos (Equipo, Fecha, Lote,	
		                    EtiquetaProducto, EtiquetaCaja, EtiquetaTotal, 
		                    ImpresoraProducto, ImpresoraCaja, ImpresoraTotal, 
		                    CopiasProducto, CopiasCaja, CopiasTotal, NumeroProductosCierre)
	                    values (@Equipo, @Fecha, @Lote,	
		                    @EtiquetaProducto, @EtiquetaCaja, @EtiquetaTotal, 
		                    @ImpresoraProducto, @ImpresoraCaja, @ImpresoraTotal, 
		                    @CopiasProducto, @CopiasCaja, @CopiasTotal, @NumeroProductosCierre)
	                    Select SCOPE_IDENTITY()
                    END
                    else
                    BEGIN
	                    Update Trabajos
	                    set	Fecha=@Fecha, Lote=@Lote,	
		                    EtiquetaProducto=@EtiquetaProducto, EtiquetaCaja=@EtiquetaCaja, EtiquetaTotal=@EtiquetaTotal, 
		                    ImpresoraProducto=@ImpresoraProducto, ImpresoraCaja=@ImpresoraCaja, ImpresoraTotal=@ImpresoraTotal, 
		                    CopiasProducto=@CopiasProducto, CopiasCaja=@CopiasCaja, CopiasTotal=@CopiasTotal, NumeroProductosCierre=@NumeroProductosCierre
	                    where Id = @Id
	                    Select @Id
                    END
                    ";
                using (SqlCommand cmd = new SqlCommand(qInsert, Configuracion.ConexionBBDD)) {
                    cmd.Parameters.Add("Id", SqlDbType.Int).Value = oTrabajo.Id;
                    cmd.Parameters.Add("Equipo", SqlDbType.VarChar).Value = oTrabajo.Equipo;
                    cmd.Parameters.Add("Fecha", SqlDbType.VarChar).Value = oTrabajo.Fecha;
                    cmd.Parameters.Add("Lote", SqlDbType.VarChar).Value = oTrabajo.Lote;
                    cmd.Parameters.Add("EtiquetaProducto", SqlDbType.VarChar).Value = oTrabajo.EtiquetaProducto;
                    cmd.Parameters.Add("EtiquetaCaja", SqlDbType.VarChar).Value = oTrabajo.EtiquetaCaja;
                    cmd.Parameters.Add("EtiquetaTotal", SqlDbType.VarChar).Value = oTrabajo.EtiquetaTotal;
                    cmd.Parameters.Add("ImpresoraProducto", SqlDbType.VarChar).Value = oTrabajo.ImpresoraProducto;
                    cmd.Parameters.Add("ImpresoraCaja", SqlDbType.VarChar).Value = oTrabajo.ImpresoraCaja;
                    cmd.Parameters.Add("ImpresoraTotal", SqlDbType.VarChar).Value = oTrabajo.ImpresoraTotal;
                    cmd.Parameters.Add("CopiasProducto", SqlDbType.Int).Value = oTrabajo.CopiasProducto;
                    cmd.Parameters.Add("CopiasCaja", SqlDbType.Int).Value = oTrabajo.CopiasCaja;
                    cmd.Parameters.Add("CopiasTotal", SqlDbType.Int).Value = oTrabajo.CopiasTotal;
                    cmd.Parameters.Add("NumeroProductosCierre", SqlDbType.Int).Value = oTrabajo.NumeroProductosCierre;
                    var result = cmd.ExecuteScalar();
                    int.TryParse(result.ToString(), out id);
                }
            } catch (Exception ex) {
                id = -1;
                Console.WriteLine(ex.Message);
            }

            return id;
        }
        public static void FinalizarTrabajo(int Id) {
            try {
                using (SqlCommand cmd = new SqlCommand("Update Trabajos set Finalizado = 1 where Id = @Id", Configuracion.ConexionBBDD)) {
                    cmd.Parameters.Add("Id", SqlDbType.Int).Value = Id;
                    cmd.ExecuteNonQuery();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }
        private static Trabajo ObtenerEntidadTrabajo(DataRow dr) {
            int totalCajas = 0;
            decimal totalKilos = 0;

            //TotalCajas
            string qSelect = "Select Count(Id) from Cajas (nolock) where IdTrabajo = @Id";
            using (SqlCommand cmd = new SqlCommand(qSelect, Configuracion.ConexionBBDD))
            {
                cmd.Parameters.Add("Id", SqlDbType.VarChar).Value = UtilidadesSQL.ObtenerEntero(dr, "Id");
                var result = cmd.ExecuteScalar();
                int.TryParse(result.ToString(), out totalCajas);
            };

            //TotalKilos
            qSelect = "Select Sum(Peso) from Cajas (nolock) where IdTrabajo = @Id";
            using (SqlCommand cmd = new SqlCommand(qSelect, Configuracion.ConexionBBDD))
            {
                cmd.Parameters.Add("Id", SqlDbType.VarChar).Value = UtilidadesSQL.ObtenerEntero(dr, "Id");
                var result = cmd.ExecuteScalar();
                decimal.TryParse(result.ToString(), out totalKilos);
            };

            return new Trabajo()
            {
                Id = UtilidadesSQL.ObtenerEntero(dr, "Id"),
                Equipo = UtilidadesSQL.ObtenerCadena(dr, "equipo"),
                EtiquetaProducto = UtilidadesSQL.ObtenerCadena(dr, "EtiquetaProducto"),
                EtiquetaCaja = UtilidadesSQL.ObtenerCadena(dr, "EtiquetaCaja"),
                EtiquetaTotal = UtilidadesSQL.ObtenerCadena(dr, "EtiquetaTotal"),
                CopiasProducto = UtilidadesSQL.ObtenerEntero(dr, "CopiasProducto"),
                CopiasCaja = UtilidadesSQL.ObtenerEntero(dr, "CopiasCaja"),
                CopiasTotal = UtilidadesSQL.ObtenerEntero(dr, "CopiasTotal"),
                ImpresoraProducto = UtilidadesSQL.ObtenerCadena(dr, "ImpresoraProducto"),
                ImpresoraCaja = UtilidadesSQL.ObtenerCadena(dr, "ImpresoraCaja"),
                ImpresoraTotal = UtilidadesSQL.ObtenerCadena(dr, "ImpresoraTotal"),
                NumeroProductosCierre = UtilidadesSQL.ObtenerEntero(dr, "NumeroProductosCierre"),
                Lote = UtilidadesSQL.ObtenerCadena(dr, "Lote"),
                Fecha = UtilidadesSQL.ObtenerFecha(dr, "Fecha"),
                Finalizado = UtilidadesSQL.ObtenerBooleano(dr, "Finalizado"),
                TotalCajas = totalCajas,
                TotalKilos = totalKilos
            };
        }
    }
}
