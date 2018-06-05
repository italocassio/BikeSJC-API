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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
namespace BikeSJC.Controllers
{
    
    public class EstacaoController : ApiController
    {
        Connect sql = new Connect();

        [Route("Estacao/")]
        [HttpGet]
        public IEnumerable<Estacao> Get(string loc)
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

                    if (loc != null)
                    {                       
                        Distancia dist = estacaoDistancia(loc, dr["est_latitude"].ToString(), dr["est_longitude"].ToString());
                        string dista = dist.rows[0].elements[0].distance.text;
                        if (dista != null)
                        {
                            est.est_distancia = dista;
                        }                        
                    }
                   
                    string info = estacaoInfo(est.est_id);
                    JToken token = JObject.Parse(info);
                    est.est_num_bikes_atual = (int)token.SelectToken("bicicletas");
                    est.est_travas_disponiveis = (int)token.SelectToken("vagas");
                    
                    //est.est_capacidade = Convert.ToInt32(dr["est_capacidade"]);
                    listaEstacoes.Add(est);
                }
            }
                       
            return listaEstacoes;
        }

        [Route("Estacao/info")]
        [HttpGet]
        public string estacaoInfo(int id)
        {
            StringBuilder query = new StringBuilder();
            EstacaoInfo estacao = new EstacaoInfo();

            try
            {
                query.AppendLine("select ");
                query.AppendLine("(select count(*) from travas where estacoes_est_id=@id and trava_status = 'D' and trava_funcionando='1') as 'vagas_disponiveis', ");
                query.AppendLine("	(select count(*) from travas where estacoes_est_id=@id and trava_status = 'I' and trava_funcionando='1') as 'bikes_disponiveis' ");
                query.AppendLine("from estacoes ");
                query.AppendLine("where est_id=@id");

                List<MySqlParameter> param = new List<MySqlParameter>();
                param.Add(new MySqlParameter("@id", id));

                using (MySqlDataReader dr = sql.RetornaQueryParam(query.ToString(), param))
                {
                    if (dr.Read())
                    {
                        estacao.bicicletas = Convert.ToInt32(dr["bikes_disponiveis"]);
                        estacao.vagas = Convert.ToInt32(dr["vagas_disponiveis"]);
                    }
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }

            return JsonConvert.SerializeObject(estacao);
        }

        
        public Distancia estacaoDistancia(string local, string lat, string lng ) 
        {
            lat = lat.Replace(",", ".");
            lng = lng.Replace(",", ".");

            Distancia dist = new Distancia();
            string URL = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={local}&destinations={lat},{lng}&mode=bicycling&language=pt-BR";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.ContentType = "application/json; charset=utf-8";
            request.PreAuthenticate = true;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);

                var dadosRetorno = reader.ReadToEnd();
                dist = JsonConvert.DeserializeObject<Distancia>(dadosRetorno);
            }


            return dist;
        }

        public class Distancia
        {
            public List<String> destination_addresses { get; set; }
            public List<String> origin_addresses { get; set; }
            public List<Rows> rows { get; set; }
            public string status { get; set; }
        }

        public class Rows
        {
            public List<Elements> elements { get; set; }
        }
        public class Elements
        {
            public Dist distance { get; set; }
            public Duracao duration { get; set; }
            public string status { get; set; }
        }

        public class Dist
        {
            public string text { get; set; }
            public string value { get; set; }
        }

        public class Duracao
        {
            public string text { get; set; }
            public string value { get; set; }
        }
        
        public class Local
        {
            public string lat { get; set; }
            public string lng { get; set; }
        }

               
    }
}
