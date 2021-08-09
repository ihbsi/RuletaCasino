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
    public class CloseRouletteWheelController : ApiController
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
        public String Post(int IdRoulettWheel)
        {
            #region Variables de método
            int IdRoundBet;
            int WinningNumber;
            List<WinnersResponseModel> ListWinners;
            #endregion

            try
            {
                IdRoundBet = CloseRouletteWheel(IdRoulettWheel);

                if (IdRoundBet.Equals(-1))
                    return JsonConvert.SerializeObject(this.response);

                WinningNumber = GetWinningNumber();

                WinningNumber = 15;
                WinningNumberInDB(IdRoundBet, WinningNumber);

                ListWinners = GetWinners(IdRoundBet);

                return ListWinners.Count.Equals(0)
                    ? JsonConvert.SerializeObject(this.response)
                    : JsonConvert.SerializeObject(new
                    {
                        Status = "0",
                        Message = ListWinners
                    });
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
        private int CloseRouletteWheel(int IdRoulettWheel)
        {
            #region Variables de método
            DataSet dsRoulette;
            int iOption;
            #endregion

            try
            {
                iOption = 110;
                this.Command.AddParameter("Option", iOption);
                this.Command.AddParameter("IdRouletteWheel", IdRoulettWheel);
                this.Command.AddParameter("EstadoRoulette", 0);

                dsRoulette = this.Command.EjecutarProcedimiento(HelperClass.SP_ROULETTEWHEELEVENTS);

                if (!HelperClass.ResultSP(dsRoulette).Equals("OK."))
                {
                    this.response = new ResponseModel()
                    {
                        Status = "-2",
                        Message = dsRoulette.Tables[0].Rows[0]["Message"].ToString()
                    };
                    return -1;
                }
                return Convert.ToInt32(dsRoulette.Tables[0].Rows[0]["IdRoundBet"]);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private int GetWinningNumber()
        {
            #region Variables de método
            Random Rnd;
            #endregion

            Rnd = new Random();

            return Rnd.Next(0, 37);
        }

        private Boolean WinningNumberInDB(int IdRoundBet, int WinningNumber)
        {
            #region Variables de método
            DataSet dsWinningNumber;
            int iOption;
            #endregion

            try
            {
                iOption = 160;

                this.Command.AddParameter("Option", iOption);
                this.Command.AddParameter("IdRoundBet", IdRoundBet);
                this.Command.AddParameter("WinningNumber", WinningNumber);

                dsWinningNumber = this.Command.EjecutarProcedimiento(HelperClass.SP_ROULETTEWHEELEVENTS);

                if (!HelperClass.ResultSP(dsWinningNumber).Equals("OK."))
                {
                    this.response = new ResponseModel()
                    {
                        Status = "-2",
                        Message = dsWinningNumber.Tables[0].Rows[0]["Message"].ToString()
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

        private List<WinnersResponseModel> GetWinners(int IdRoundBet)
        {
            #region Variables de método
            DataSet dsWinners;
            int iOption;
            List<WinnersResponseModel> ListWinners;
            #endregion

            try
            {
                iOption = 170;
                ListWinners = new List<WinnersResponseModel>();

                this.Command.AddParameter("Option", iOption);
                this.Command.AddParameter("IdRoundBet", IdRoundBet);

                dsWinners = this.Command.EjecutarProcedimiento(HelperClass.SP_ROULETTEWHEELEVENTS);

                if (!HelperClass.ResultSP(dsWinners).Equals("OK."))
                {
                    this.response = new ResponseModel()
                    {
                        Status = "-2",
                        Message = dsWinners.Tables[0].Rows[0]["Message"].ToString()
                    };
                    return ListWinners;
                }

                foreach (DataRow drWinner in dsWinners.Tables[0].Rows)
                {
                    WinnersResponseModel winner = new WinnersResponseModel()
                    {
                        Username = drWinner["Username"].ToString(),
                        TypeBet = Convert.ToInt16(drWinner["TypeBet"]),
                        SingleWinner = drWinner["SingleWinner"].ToString(),
                        PayOut = Convert.ToDouble(drWinner["PayOut"])
                    };

                    ListWinners.Add(winner);
                }
                return ListWinners;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }



        }
        #endregion
    }
}
