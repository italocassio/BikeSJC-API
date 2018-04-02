using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BikeSJC.Models
{
    public class Estacao
    {
        public int est_id{ get; set; }
        public string est_nome { get; set; }
        public decimal est_longitude { get; set; }
        public decimal est_latitude { get; set; }
        public int est_capacidade { get; set; }
        public int est_num_bikes_atual { get; set; }
        public string est_observacoes { get; set; }
        public Boolean est_status { get; set; }
        public int est_travas_disponiveis { get; set; }
        public int est_travas_indisponiveis { get; set; }
        public string est_alugando_bike { get; set; }
        public Boolean est_aceitando_bike { get; set; }
        public DateTime est_ultimo_relatorio { get; set; }
    }

    public class EstacaoInfo
    {
        public int bicicletas { get; set; }
        public int vagas { get; set; }
    }
}