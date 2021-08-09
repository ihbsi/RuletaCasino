using Newtonsoft.Json;
using System;
using System.Data;
using System.Web.Http;
using System.Web.Script.Services;
using System.Web.Services;

namespace RuletaCasino.WebApi.Controllers
{
    public class OpenRouletteWheelController : ApiController
    {
        #region Variables de clase
        /// <summary>
        /// Inicializacion del objeto ConnectionBD
        /// </summary>
        private readonly ConnectionDB Command = new ConnectionDB();
        #endregion

        #region Servicios
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String Post(int IdRoulettWheel)
        {
            #region Variables de método
            DataSet dsRoulette;
            int iOption;
            ResponseModel response;
            #endregion

            try
            {
                iOption = 110;
                this.Command.AddParameter("Option", iOption);
                this.Command.AddParameter("IdRouletteWheel", IdRoulettWheel);
                this.Command.AddParameter("EstadoRoulette", 1);
                
                dsRoulette = this.Command.EjecutarProcedimiento(HelperClass.SP_ROULETTEWHEELEVENTS);

                response = !HelperClass.ResultSP(dsRoulette).Equals("OK.")
                    ? new ResponseModel()
                    {
                        Status = "-2"
                    }
                    : new ResponseModel()
                    {
                        Status = "0"
                    };

                response.Message = dsRoulette.Tables[0].Rows[0]["Message"].ToString();

                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                response = new ResponseModel()
                {
                    Status = "-1",
                    Message = ex.Message
                };

                return JsonConvert.SerializeObject(response);
            }
            finally
            {
                this.Command.Desconectar();
            }
        }
        #endregion
    }
}
