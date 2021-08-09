using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Script.Services;
using System.Web.Services;

namespace RuletaCasino.WebApi.Controllers
{
    public class PlaceBetController : ApiController
    {

        #region Variables de clase
        /// <summary>
        /// Inicializacion del objeto ConnectionBD
        /// </summary>
        private readonly ConnectionDB Command = new ConnectionDB();
        /// <summary>
        /// Objeto para respuesta del servicio
        /// </summary>
        private ResponseModel response;
        #endregion

        #region Servicios
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [ResponseType(typeof(PlaceBetParamsModel))]
        public String Post(PlaceBetParamsModel PlaceBetParams)
        {
            try
            {
                if (ModelIsNull(PlaceBetParams))
                    return JsonConvert.SerializeObject(this.response);

                if (ModelIsNotValid(PlaceBetParams))
                    return JsonConvert.SerializeObject(this.response);

                if (!ValidSingleNumber((int)PlaceBetParams.TypeBet, (int)PlaceBetParams.SingleNumber))
                    return JsonConvert.SerializeObject(this.response);

                if (!ValidRouletteWheel((int)PlaceBetParams.IdRouletteWheel))
                    return JsonConvert.SerializeObject(this.response);

                PlaceBetInDB(PlaceBetParams);

                return JsonConvert.SerializeObject(this.response);
            }
            catch (Exception ex)
            {
                this.response = new ResponseModel()
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

        #region Métodos privados
        private Boolean ModelIsNull(PlaceBetParamsModel PlaceBetParams)
        {
            if (HelperClass.ModelIsNull<PlaceBetParamsModel>(PlaceBetParams))
            {
                this.response = new ResponseModel()
                {
                    Status = "-2",
                    Message = "Los parámetros requeridos no fueron indicados."
                };
                return true;
            }
            return false;
        }

        private Boolean ModelIsNotValid(PlaceBetParamsModel PlaceBetParams)
        {
            if (!ModelState.IsValid)
            {
                List<String> ListErrors = ModelState.Values.SelectMany
                                                    (m => m.Errors.Select
                                                        (e => e.ErrorMessage)
                                                    ).ToList();
                response = new ResponseModel()
                {
                    Status = "-2",
                    Message = $"{ String.Join(", ", ListErrors) }."
                };

                return true;
            }
            return false;
        }

        private Boolean ValidSingleNumber(int TypeBet, int SingleNumber)
        {
            if (TypeBet.Equals(1) && (SingleNumber < 0 || SingleNumber > 36))
            {
                response = new ResponseModel()
                {
                    Status = "-2",
                    Message = "El número apostado no es válido. Debe estar entre 0 a 36."
                };
                return false;
            }

            if (TypeBet.Equals(2) && (!SingleNumber.Equals(0) && !SingleNumber.Equals(1)))
            {
                response = new ResponseModel()
                {
                    Status = "-2",
                    Message = "El color apostado no es válido. Debe ser 0 (Rojo) o 1 (Negro)."
                };
                return false;
            }
            return true;
        }

        private Boolean ValidRouletteWheel(int IdRouletteWheel)
        {
            #region Variables de método
            DataSet dsRoulette;
            int iOption;
            #endregion

            try
            {
                iOption = 120;

                this.Command.AddParameter("Option", iOption);
                this.Command.AddParameter("IdRouletteWheel", IdRouletteWheel);
                dsRoulette = this.Command.EjecutarProcedimiento(HelperClass.SP_ROULETTEWHEELEVENTS);

                if (!HelperClass.ResultSP(dsRoulette).Equals("OK."))
                {
                    response = new ResponseModel()
                    {
                        Status = "-3",
                        Message = dsRoulette.Tables[0].Rows[0]["Message"].ToString()
                    };
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void PlaceBetInDB(PlaceBetParamsModel PlaceBetParams)
        {
            #region Variables de método
            DataSet dsBet;
            int iOption;
            #endregion

            try
            {
                iOption = 130;

                this.Command.AddParameter("Option", iOption);
                this.Command.AddParameter("IdRouletteWheel", PlaceBetParams.IdRouletteWheel);
                this.Command.AddParameter("Username", PlaceBetParams.Username);
                this.Command.AddParameter("TypeBet", PlaceBetParams.TypeBet);
                this.Command.AddParameter("Amount", PlaceBetParams.Amount);

                if (PlaceBetParams.TypeBet.Equals(1))
                    this.Command.AddParameter("SingleNumber", PlaceBetParams.SingleNumber);

                if (PlaceBetParams.TypeBet.Equals(2))
                    this.Command.AddParameter("SingleColor",
                        PlaceBetParams.SingleNumber.Equals(0) ? "ROJO" : "NEGRO");

                dsBet = this.Command.EjecutarProcedimiento(HelperClass.SP_ROULETTEWHEELEVENTS);

                if (!HelperClass.ResultSP(dsBet).Equals("OK."))
                {
                    response = new ResponseModel()
                    {
                        Status = "-3",
                        Message = dsBet.Tables[0].Rows[0]["Message"].ToString()
                    };
                }

                this.response = new ResponseModel()
                {
                    Status = "0",
                    Message = dsBet.Tables[0].Rows[0]["Message"].ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}
