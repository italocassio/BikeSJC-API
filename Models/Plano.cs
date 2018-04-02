using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BikeSJC.Models
{
    public class Plano
    {
        public int pnaId { get; set; }
        public string  pnaDtExpira { get; set; }
        public DateTime pnaDtCompra { get; set; }
        public int pnaStatus { get; set; }
        public int planoId { get; set; }
        public int usuarioId { get; set; }
        public int pnaVoucher { get; set; }
    }
}