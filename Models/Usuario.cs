using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BikeSJC.Models
{
    public class Usuario
    {
        public int id { get; set; }
        public string usuario { get; set; }
        public string senha { get; set; }
        public string nome { get; set; }
        public string cpf { get; set; }
        public string email { get; set; }
        public string telefone { get; set; }
        public string status { get; set; }
        public int credito { get; set; }       
        
    }
       

}

