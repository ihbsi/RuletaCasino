using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RuletaCasino.WebApi
{
    public class ConnectionDB
    {
        #region Variables privadas de clase
        /// <summary>
        /// Cadena de conexión a la base de datos.
        /// </summary>
        private readonly SqlConnection conexion;
        /// <summary>
        /// Lista de parámetros para el procedimiento almacenado.
        /// </summary> 
        private readonly List<SqlParameter> listParameters;
        /// <summary>
        /// Objeto que registra el log de eventos.
        /// </summary>
        private LogTrace logCorresp = null;
        /// <summary>
        /// Ruta del archivo log donde se almacenan los eventos.
        /// </summary>
        private String strFileLog = String.Empty;
        /// <summary>
        /// Activa la escritura de los eventos en el archivo log.
        /// </summary>
        private bool bLog = false;
        #endregion

        public ConnectionDB()
        {
            try
            {
                this.listParameters = new List<SqlParameter>();
                this.conexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectDB"].ConnectionString);
                this.conexion.Open();
            }
            catch (Exception ex)
            {
                RecordLog("ConnectionDB", "ConnectionDB", ex.Message);
            }
        }
        public void AddParameter(String Nombre, object Valor, ParameterDirection Direccion = ParameterDirection.Input, String NombreTipo = "")
        {
            SqlParameter Parameter = new SqlParameter
            {
                ParameterName = "@" + Nombre,
                Value = Valor,
                Direction = Direccion
            };

            if (Valor != null)
            {
                if (Valor.GetType().Equals(typeof(DataTable)))
                {
                    Parameter.SqlDbType = SqlDbType.Structured;
                }
            }

            if (!NombreTipo.Equals(String.Empty))
            {
                Parameter.TypeName = NombreTipo;
            }
            this.listParameters.Add(Parameter);
        }

        public DataTable Consultar(String sSQL)
        {
            #region Variables de método
            DataTable dt;
            SqlCommand cmd;
            SqlDataReader dr;
            #endregion

            dt = null;
            
            try
            {
                dt = new DataTable();
                cmd = new SqlCommand(sSQL, this.conexion);
                dr = cmd.ExecuteReader();
                dt.Load(dr);
            }
            catch (Exception ex)
            {
                RecordLog("InterDataBase.ConnectDB", "Consultar", ex.Message);
            }
            return dt;
        }

        public DataSet EjecutarProcedimiento(String NombreProcedimiento)
        {
            #region Variables de método
            DataSet ds;
            #endregion

            ds = new DataSet();

            try
            {
                SqlCommand comando = new SqlCommand
                {
                    Connection = this.conexion,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = NombreProcedimiento,
                    CommandTimeout = 900
                };
                foreach (SqlParameter parameter in this.listParameters)
                {
                    if (parameter.SqlDbType.Equals(SqlDbType.Structured))
                    {
                        comando.Parameters.AddWithValue(parameter.ParameterName, parameter.Value);
                    }
                    else
                    {
                        comando.Parameters.Add(parameter);
                    }
                }
                SqlDataAdapter da = new SqlDataAdapter
                {
                    SelectCommand = comando
                };
                da.Fill(ds);
            }
            catch (Exception ex)
            {
                RecordLog("ConnectionDB", "EjecutarProcedimiento", ex.Message);
            }
            finally
            {
                this.Desconectar();
                this.listParameters.Clear();
            }
            return ds;
        }

        public Boolean Desconectar()
        {
            try
            {
                if (this.conexion != null)
                    this.conexion.Close();
                return true;
            }
            catch (Exception ex)
            {
                RecordLog("ConnectionDB", "Desconectar", ex.Message);
                return false;
            }
        }
        public void RecordLog(String strMetodo, String strFuncion, String strMensaje)
        {
            if (this.logCorresp is null)
            {
                this.strFileLog = ConfigurationManager.AppSettings["FileLog"];
                this.bLog = Convert.ToBoolean(ConfigurationManager.AppSettings["Log"]);
                this.logCorresp = new LogTrace(this.strFileLog, this.bLog);
            }
            this.logCorresp.WriteLog(strMetodo, strFuncion, strMensaje);
        }
    }
}