using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RuletaCasino.WebApi
{
    public class PlaceBetParamsModel
    {
        [Required(ErrorMessage = "El ID de la ruleta es obligatorio")]
        public int? IdRouletteWheel { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        public String Username { get; set; }

        [Required(ErrorMessage = "El tipo de apuesta es obligatorio")]
        [Range(1, 2, ErrorMessage = "El tipo de apuesta no es válido. " +
            "Sólo se permite 1 (Apuesta al número), 2 (Apuesta al color)")]
        public int? TypeBet { get; set; }

        [Required(ErrorMessage = "El número apostado es obligatorio")]
        public int? SingleNumber { get; set; }

        [Range(1, 10000, ErrorMessage = "El monto de la apuesta no es válido. " +
            "Debe ser estar entre 1 a 10000 USD")]
        public Double Amount { get; set; }
    }
}