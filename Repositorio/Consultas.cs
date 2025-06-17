using ProcesadoSummary.Model;
using ProcesadoSummary.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProcesadoSummary.Repositorio
{
    public class Consultas
    {
        public OleDbConnection connection { get; set; }    // conexion al access.

        public Consultas(OleDbConnection connection)
        {
            this.connection = connection;
        }

        public Carretera GetCarreteraByName(string nombreCarretera)
        {
            Carretera c = null;

            string query = "SELECT * FROM CARRETERA WHERE NOMBRE = ?";

            try
            {
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.Add(new OleDbParameter("@nombre", nombreCarretera));

                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            c = new Carretera
                            {
                                Id = Convert.ToInt32(reader["Id"].ToString()),
                                Nombre = reader["Nombre"].ToString()
                            };
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error función GetDatos", "Consultas");
            }

            return c;
        }

        public Tramo GetTramoByCarreteraAndCarril(int idCarretera, string carril)
        {
            Tramo t = null;

            string query = "SELECT * FROM TRAMO WHERE IdCarretera = ? AND Carril = ?";

            try
            {
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.Add(new OleDbParameter("@IdCarretera", idCarretera));
                    command.Parameters.Add(new OleDbParameter("@Carril", carril));

                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            t = new Tramo
                            {
                                Id = Convert.ToInt32(reader["Id"].ToString()),
                                IdCarretera = Convert.ToInt32(reader["IdCarretera"].ToString()),
                                PKI = reader["PKI"].ToString(),
                                PKF = reader["PKF"].ToString(),
                                Carril = reader["Carril"].ToString(),
                                NumTramo = reader["NumTramo"].ToString(),
                                Observaciones = reader["Observaciones"].ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error función GetDatos", "Consultas");
            }

            return t;
        }

        public List<Dato> GetDatos()
        {
            List<Dato> listaDatos = new List<Dato>();

            string query = "SELECT * FROM DATO";

            try
            {
                using (OleDbCommand command = new OleDbCommand(query, connection))
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Dato d = new Dato
                        {
                            Dist_Origen = Convert.ToInt32(reader["distOrigen"]),
                            PKI = reader["PKI"].ToString(),
                            PKF = reader["PKF"].ToString(),
                            Archivo = 1,
                            Nombre = reader["Nombre"].ToString(),
                            Area_total = reader["Area_total"].ToString(),
                            Long_total = reader["Long_total"].ToString(),
                            IFTotal = reader["IFTotal"].ToString(),
                            Long_proyec = reader["Long_proyec"].ToString(),
                            IFP = reader["IFP"].ToString(),
                            Area_long = reader["Area_long"].ToString(),
                            Long_long = reader["Long_long"].ToString(),
                            IFL = reader["IFL"].ToString(),
                            Area_trans = reader["Area_trans"].ToString(),
                            Long_trans = reader["Long_trans"].ToString(),
                            IFT = reader["IFT"].ToString(),
                            Area_otras = reader["Area_otras"].ToString(),
                            Long_otras = reader["Long_otras"].ToString(),
                            IFO = reader["IFO"].ToString(),
                            Prof_r_izq = reader["Prof_r_izq"].ToString(),
                            Ancho_r_izq = reader["Ancho_r_izq"].ToString(),
                            Area_ri = reader["Area_ri"].ToString(),
                            Prof_r_der = reader["Prof_r_der"].ToString(),
                            Ancho_r_der = reader["Ancho_r_der"].ToString(),
                            Area_rd = reader["Area_rd"].ToString(),
                            Pos_linea_izq = reader["Pos_linea_izq"].ToString(),
                            Pos_linea_der = reader["Pos_linea_der"].ToString(),
                            Ancho_carril = reader["Ancho_carril"].ToString(),
                            UTM_X = reader["UTM_X"].ToString(),
                            UTM_Y = reader["UTM_Y"].ToString(),
                            UTM_Z = reader["UTM_Z"].ToString(),
                            Ancho_maximo = reader["Ancho_maximo"].ToString(),
                            Area_malla = "",
                            Long_malla = "",
                            Long_parches = "",
                            IFM = "",
                            Textura = "",
                            Textura_b1 = "",
                            Textura_b2 = "",
                            Textura_b3 = "",
                            Textura_b4 = "",
                            Textura_b5 = "",
                            Resul_ravelling = "",
                            N_baches = "",
                            Area_baches = "",
                            Area_parches = "",
                            Indice_parches = "",
                            Validar_carril = reader["Validar_carril"].ToString()
                        };

                        listaDatos.Add(d);
                    }
                }
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error función GetDatos", "Consultas");
            }



            return listaDatos;
        }

        public int AddCarretera(Carretera c)
        {
            try
            {
                string query = "INSERT INTO CARRETERA (Nombre) VALUES (@Nombre)";

                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", c.Nombre);
                    command.ExecuteNonQuery();

                    // Obtener el ID generado
                    command.CommandText = "SELECT @@IDENTITY";
                    command.Parameters.Clear(); // Muy importante limpiar los parámetros antes de reutilizar el comando
                    object result = command.ExecuteScalar();

                    if (result != null && int.TryParse(result.ToString(), out int newId))
                    {
                        return newId;
                    }
                }

            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error al InsertCarretera: {ex.Message}", "Consultas");
            }

            return 0;
        }

        public int AddTramo(Tramo t)
        {

            try
            {
                string query = "INSERT INTO TRAMO (IdCarretera, PKI, PKF, Carril, NumTramo, Observaciones) VALUES (@IdCarretera, @PKI, @PKF, @Carril, @NumTramo, @Observaciones)";

                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdCarretera", t.IdCarretera);
                    command.Parameters.AddWithValue("@PKI", t.PKI);
                    command.Parameters.AddWithValue("@PKF", t.PKI);
                    command.Parameters.AddWithValue("@Carril", t.Carril);
                    command.Parameters.AddWithValue("@NumTramo", t.NumTramo);
                    command.Parameters.AddWithValue("@Observaciones", t.Observaciones);

                    command.ExecuteNonQuery();

                    // Obtener el ID generado
                    command.CommandText = "SELECT @@IDENTITY";
                    command.Parameters.Clear(); // Muy importante limpiar los parámetros antes de reutilizar el comando
                    object result = command.ExecuteScalar();

                    if (result != null && int.TryParse(result.ToString(), out int newId))
                    {
                        return newId;
                    }
                }
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error AddTramo: {ex.Message}");
            }

            return 0;
        }

        public void AddDato(Dato d)
        {
            string query = "INSERT INTO DATO (IdTramo, Dist_Origen, PKI, PKF, Archivo, Nombre, Area_total, Long_total, IFTotal, Long_proyec, IFP, Area_long, Long_long, IFL, Area_trans, Long_trans, IFT, Area_otras, Long_otras, IFO, Area_malla, Long_malla, IFM, Prof_r_izq, Ancho_r_izq, Area_ri, Prof_r_der, Ancho_r_der, Area_rd, Textura_b1, Textura_b2, Textura_b3, Textura_b4, Textura_b5, Textura, Resul_ravelling, N_baches, Area_baches, Area_parches, Long_parches, Indice_parches, Pos_linea_izq, Pos_linea_der, Ancho_carril, Validar_carril, UTM_X, UTM_Y, UTM_Z, Ancho_maximo)" +
                " VALUES (@IdTramo, @Dist_Origen, @PKI, @PKF, @Archivo, @Nombre, @Area_total, @Long_total, @IFTotal, @Long_proyec, @IFP, @Area_long, @Long_long, @IFL, @Area_trans, @Long_trans, @IFT, @Area_otras, @Long_otras, @IFO, @Area_malla, @Long_malla, @IFM, @Prof_r_izq, @Ancho_r_izq, @Area_ri, @Prof_r_der, @Ancho_r_der, @Area_rd, @Textura_b1, @Textura_b2, @Textura_b3, @Textura_b4, @Textura_b5, @Textura, @Resul_ravelling, @N_baches, @Area_baches, @Area_parches, @Long_parches, @Indice_parches, @Pos_linea_izq, @Pos_linea_der, @Ancho_carril, @Validar_carril, @UTM_X, @UTM_Y, @UTM_Z, @Ancho_maximo)";

            try
            {
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdTramo", d.IdTramo);
                    command.Parameters.AddWithValue("@Dist_Origen", d.Dist_Origen);
                    command.Parameters.AddWithValue("@PKI", d.PKI);
                    command.Parameters.AddWithValue("@PKF", d.PKF);
                    command.Parameters.AddWithValue("@Archivo", d.Archivo);
                    command.Parameters.AddWithValue("@Nombre", d.Nombre);
                    command.Parameters.AddWithValue("@Area_total", d.Area_total);
                    command.Parameters.AddWithValue("@Long_total", d.Long_total);
                    command.Parameters.AddWithValue("@IFTotal", d.IFTotal);
                    command.Parameters.AddWithValue("@Long_proyec", d.Long_proyec);
                    command.Parameters.AddWithValue("@IFP", d.IFP);
                    command.Parameters.AddWithValue("@Area_long", d.Area_long);
                    command.Parameters.AddWithValue("@Long_long", d.Long_long);
                    command.Parameters.AddWithValue("@IFL", d.IFL);
                    command.Parameters.AddWithValue("@Area_trans", d.Area_trans);
                    command.Parameters.AddWithValue("@Long_trans", d.Long_trans);
                    command.Parameters.AddWithValue("@IFT", d.IFT);
                    command.Parameters.AddWithValue("@Area_otras", d.Area_otras);
                    command.Parameters.AddWithValue("@Long_otras", d.Long_otras);
                    command.Parameters.AddWithValue("@IFO", d.IFO);
                    command.Parameters.AddWithValue("@Area_malla", d.Area_malla);
                    command.Parameters.AddWithValue("@Long_malla", d.Long_malla);
                    command.Parameters.AddWithValue("@IFM", d.IFM);
                    command.Parameters.AddWithValue("@Prof_r_izq", d.Prof_r_izq);
                    command.Parameters.AddWithValue("@Ancho_r_izq", d.Ancho_r_izq);
                    command.Parameters.AddWithValue("@Area_ri", d.Area_ri);
                    command.Parameters.AddWithValue("@Prof_r_der", d.Prof_r_der);
                    command.Parameters.AddWithValue("@Ancho_r_der", d.Ancho_r_der);
                    command.Parameters.AddWithValue("@Area_rd", d.Area_rd);
                    command.Parameters.AddWithValue("@Textura_b1", d.Textura_b1);
                    command.Parameters.AddWithValue("@Textura_b2", d.Textura_b2);
                    command.Parameters.AddWithValue("@Textura_b3", d.Textura_b3);
                    command.Parameters.AddWithValue("@Textura_b4", d.Textura_b4);
                    command.Parameters.AddWithValue("@Textura_b5", d.Textura_b5);
                    command.Parameters.AddWithValue("@Textura", d.Textura);
                    command.Parameters.AddWithValue("@Resul_ravelling", d.Resul_ravelling);
                    command.Parameters.AddWithValue("@N_baches", d.N_baches);
                    command.Parameters.AddWithValue("@Area_baches", d.Area_baches);
                    command.Parameters.AddWithValue("@Area_parches", d.Area_parches);
                    command.Parameters.AddWithValue("@Long_parches", d.Long_parches);
                    command.Parameters.AddWithValue("@Indice_parches", d.Indice_parches);
                    command.Parameters.AddWithValue("@Pos_linea_izq", d.Pos_linea_izq);
                    command.Parameters.AddWithValue("@Pos_linea_der", d.Pos_linea_der);
                    command.Parameters.AddWithValue("@Ancho_carril", d.Ancho_carril);
                    command.Parameters.AddWithValue("@Validar_carril", d.Validar_carril);
                    command.Parameters.AddWithValue("@UTM_X", d.UTM_X);
                    command.Parameters.AddWithValue("@UTM_Y", d.UTM_Y);
                    command.Parameters.AddWithValue("@UTM_Z", d.UTM_Z);
                    command.Parameters.AddWithValue("@Ancho_maximo", d.Ancho_maximo);
                    //command.Parameters.AddWithValue("@Observaciones", d.Observaciones);
                    command.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                MessagesGlobal.MessageError($"Error al InsertarDato: {ex.Message}", "Consultas");
            }
        }

    }
}
