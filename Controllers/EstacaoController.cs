using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BikeSJC.Models;
using BikeSJC.Database;
using MySql.Data.MySqlClient;
using System.Text;

namespace BikeSJC.Controllers
{
    
    public class EstacaoController : ApiController
    {
        Connect sql = new Connect();

        [Route("Estacao/")]
        [HttpGet]
        public IEnumerable<Estacao> Get()
        {
            List<Estacao> listaEstacoes = new List<Estacao>();

            string query = "Select * from estacoes";

            using (MySqlDataReader dr = sql.RetornaQuery(query))
            {
                while (dr.Read()){
                    Estacao est = new Estacao();
                    est.est_id = Convert.ToInt32(dr["est_id"]);
                    est.est_nome = dr["est_nome"].ToString();
                    est.est_latitude = Convert.ToDecimal(dr["est_latitude"]);
                    est.est_longitude = Convert.ToDecimal(dr["est_longitude"]);
                    //est.est_capacidade = Convert.ToInt32(dr["est_capacidade"]);
                    listaEstacoes.Add(est);
                }
            }
                       
            return listaEstacoes;
        }

        // GET: api/Estacao/5
        public string Get(int id)
        {
            //select est_id, est_nome, est_latitude, est_longitude,
            //(select count(*) from travas where estacoes_est_id = 1 and trava_status = 'D') as 'trava disponiveis',
            //(select count(*) from bicicletas where est_id = 1) as 'bikes disponiveis'
            //from estacoes e
            //join travas t on t.estacoes_est_id = e.est_id
            //where e.est_id = 1
            //group by 'est_id'



            return "value";
        }

        // POST: api/Estacao
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Estacao/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Estacao/5
        public void Delete(int id)
        {
        }
    }
}
