using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BikeSJC.Database;

namespace BikeSJC.Models
{
   
    public class Bicicleta
    {
        public int ID { get; set; }
        public string status { get; set; }
        public string obs { get; set; }
    }

     public class SolBike
    {
        
        public int estacao { get; set; }
        public int voucher { get; set; }
                
    }

    public class Solicitacao
    {
        public int id { get; set; }
        public int estacao { get; set; }
        public int trava { get; set; }
        public int bike { get; set; }
        public int dtretirada { get; set; }
        public int dtentrega { get; set; }
    }

    public class BicicletaProgresso
    {
        public string estacao { get; set; }
        public int estacaoId { get; set; }
        public string tempo { get; set; }
        public string agora { get; set; }
        public int bike { get; set; }
        public int trava { get; set; }
        public string msg { get; set; }
        public int execucao { get; set; }
        public int solicitacaoId { get; set; }

    }

    public class BicicletaHistorico
    {
        public string dataRetirada { get; set; }
        public string estacaoR { get; set; }
        public string estacaoE{ get; set; }
        public string tempo { get; set; }
    }
}