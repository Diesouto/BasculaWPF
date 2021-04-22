using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace PesajeWPF.Datos {
    public class ProductoGestor {
        public static Productos ObtenerProductosTrabajo(int IdTrabajo) {
            Productos colEntidad = new Productos();
            try {
                using (SqlCommand cmd = new SqlCommand("Select * from Productos where idTrabajo = @IdTrabajo order by Numero", Configuracion.ConexionBBDD))
                {
                    cmd.Parameters.Add("IdTrabajo", SqlDbType.Int).Value = IdTrabajo;
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    foreach (DataRow dr in dt.Rows)
                    {
                        colEntidad.Add(new Producto()
                        {
                            Id = UtilidadesSQL.ObtenerEntero(dr, "Id"),
                            IdTrabajo = IdTrabajo,
                            IdCaja = UtilidadesSQL.ObtenerEntero(dr, "IdCaja"),
                            Numero = UtilidadesSQL.ObtenerEntero(dr, "Numero"),
                            Peso = UtilidadesSQL.ObtenerDecimal(dr, "Peso"),
                            Lote = UtilidadesSQL.ObtenerCadena(dr, "Lote"),
                            Fecha = UtilidadesSQL.ObtenerFecha(dr, "Fecha")
                        }); ;
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return colEntidad;
        }
        public static Productos ObtenerProductosCaja(int IdCaja) {
            Productos colEntidad = new Productos();
            try {
                using (SqlCommand cmd = new SqlCommand("Select * from Productos where idCaja = @IdCaja order by Numero", Configuracion.ConexionBBDD))
                {
                    cmd.Parameters.Add("IdCaja", SqlDbType.Int).Value = IdCaja;
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    foreach (DataRow dr in dt.Rows)
                    {
                        colEntidad.Add(new Producto()
                        {
                            Id = UtilidadesSQL.ObtenerEntero(dr, "Id"),
                            IdTrabajo = UtilidadesSQL.ObtenerEntero(dr, "IdTrabajo"),
                            IdCaja = IdCaja,
                            Numero = UtilidadesSQL.ObtenerEntero(dr, "Numero"),
                            Peso = UtilidadesSQL.ObtenerDecimal(dr, "Peso"),
                            Lote = UtilidadesSQL.ObtenerCadena(dr, "Lote"),
                            Fecha = UtilidadesSQL.ObtenerFecha(dr, "Fecha")
                        }); ;
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return colEntidad;
        }

        public static int GuardaProducto(Producto oProducto) {
            int id = 0;
            try
            {
                string qInsert = @"
                    Insert into Productos (IdTrabajo, IdCaja, Numero, Peso, Lote, Fecha)
                    values (@IdTrabajo, @IdCaja, Isnull((Select max(numero) from Productos where idtrabajo = @idTrabajo), 0)+1, @Peso, @Lote, @Fecha)

                    Select SCOPE_IDENTITY()
                ";
                using (SqlCommand cmd = new SqlCommand(qInsert, Configuracion.ConexionBBDD))
                {
                    cmd.Parameters.Add("@IdTrabajo", SqlDbType.Int).Value = oProducto.IdTrabajo;
                    cmd.Parameters.Add("@IdCaja", SqlDbType.Int).Value = oProducto.IdCaja;
                    cmd.Parameters.Add("@Peso", SqlDbType.Decimal).Value = oProducto.Peso;
                    cmd.Parameters.Add("@Lote", SqlDbType.VarChar).Value = oProducto.Lote;
                    cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = oProducto.Fecha;
                    var result = cmd.ExecuteScalar();
                    int.TryParse(result.ToString(), out id);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return id;
        }
        public static bool EliminaProducto(int IdProducto) {
            bool eliminado = false;
            try {
                string qDelete = @"Delete from Productos where Id = @IdProducto";
                using (SqlCommand cmd = new SqlCommand(qDelete, Configuracion.ConexionBBDD)) {
                    cmd.Parameters.Add("IdProducto", SqlDbType.Int).Value = IdProducto;
                    int result = cmd.ExecuteNonQuery();
                    eliminado = result > 0;
                }
            } catch (Exception ex) {
                Console.Write(ex.Message);
            }
            return eliminado;
        }
        public static bool AsignaCajaProducto(int IdProducto, int IdCaja)
        {
            bool actualizado = false;
            try
            {
                string qUpdate = @"UPDATE Productos SET IdCaja = @IdCaja WHERE Id = @IdProducto";
                using (SqlCommand cmd = new SqlCommand(qUpdate, Configuracion.ConexionBBDD))
                {
                    cmd.Parameters.Add("IdProducto", SqlDbType.Int).Value = IdProducto;
                    cmd.Parameters.Add("IdCaja", SqlDbType.Int).Value = IdCaja;
                    int result = cmd.ExecuteNonQuery();
                    actualizado = result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return actualizado;
        }
    }
}
