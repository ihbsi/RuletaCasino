using System;
using System.Data;

namespace RuletaCasino.WebApi
{
    internal class HelperClass
    {
        #region Procedimientos almacenados
        internal const String SP_ROULETTEWHEELEVENTS = "[dbo].[SP_RouletteWheelEvents]";
        #endregion

        #region Métodos
        internal static String ResultSP(DataSet ds)
            => ds.Tables.Count > 0
                ? !Convert.ToInt32(ds.Tables[0].Rows[0]["Result"]).Equals(1)
                    ? ds.Tables[0].Rows[0]["Message"].ToString()
                    : "OK."
                : throw new Exception("Se generó un fallo en el sistema. Comuníquese con el Administrador.");

        internal static Boolean ModelIsNull<T>(T ObjModel)
            => ObjModel == null;
        #endregion
    }
}