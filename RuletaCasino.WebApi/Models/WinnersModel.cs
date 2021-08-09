using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RuletaCasino.WebApi
{
    public class WinnersResponseModel
    {
        public String Username { get; set; }
        public int TypeBet { get; set; }
        public String SingleWinner { get; set; }
        public Double PayOut { get; set; }
    }
}