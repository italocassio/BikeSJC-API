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

namespace BikeSJC.Controllers
{
    //[RoutePrefix("Bicicleta")]
    public class BicicletaController : ApiController
    {
        Connect sql = new Connect();
        
       
        
        // liberar bike
        [Route("Bicicleta/solicitar")]
        [HttpPost]
        public string solicitarBike([FromBody]SolBike sol)
        {
            string retorno = "";
            
            try
            {
                int plano = getPlano(sol.voucher);

                if (plano != 0)
                {
                  retorno = liberarBike(sol, plano);
                }
                else
                {
                    retorno = "Contrate um plano para alugar a bike.";
                }
            }
            catch (Exception)
            {
                retorno = "Algo deu errado";
            }
            
            return retorno;
        }

        // liberar bike no arduino
        [Route("Bicicleta/ArduinoSolicitarBike")]
        [HttpGet]
        public string solicitarBikeArduino(int voucher, int estacao)
        {
            string retorno = "";
            SolBike sol = new SolBike();
            sol.voucher = voucher;
            sol.estacao = estacao;

            try
            {
                int plano = getPlano(sol.voucher);

                if (plano != 0)
                {
                    retorno = liberarBike(sol, plano);
                }
                else
                {
                    retorno = "Contrate um plano para alugar a bike.";
                }
            }
            catch (Exception)
            {
                retorno = "Algo deu errado";
            }

            return retorno;
        }


        // bicicleta em uso
        [Route("Bicicleta/utilizando")]
        [HttpGet]
        public string bikesEmProgresso(int pna)
        {
            string retorno = "";
            BicicletaProgresso bike = new BicicletaProgresso();
            
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT ");
            query.AppendLine("sol_data_retirada, est_nome, bicicletas_bic_id, sol_id, est_id  ");
            query.AppendLine("FROM solicitacoes so ");
            query.AppendLine("join estacoes est ");
            query.AppendLine("  on est.est_id = so.sol_estacao_retirada ");
            query.AppendLine($"where plano_ativo_pna_id={pna} ");
            query.AppendLine("and sol_data_entrega is null");
            
            try
            {
                using (MySqlDataReader dr = sql.RetornaQuery(query.ToString()))
                {
                    if (dr.Read())
                    {
                        bike.estacao = dr["est_nome"].ToString();
                        bike.estacaoId = Convert.ToInt32(dr["est_id"]);
                        TimeSpan calcTime = (DateTime.Now - Convert.ToDateTime(dr["sol_data_retirada"]));
                        //{0} days, {1} hours, {2} minutes, {3} seconds
                        string horas, minutos, segundos = "";

                        if (calcTime.Hours < 10) {
                            horas ="0" + calcTime.Hours;
                        } else {
                           horas = calcTime.Hours.ToString();
                        }

                        if (calcTime.Minutes < 10) {
                            minutos = "0" + calcTime.Minutes;
                        } else {
                            minutos = calcTime.Minutes.ToString();
                        }

                        if (calcTime.Seconds < 10) {  
                            segundos = "0" + calcTime.Seconds;
                        } else {
                            segundos = calcTime.Seconds.ToString();
                        }

                        bike.tempo = String.Format("{0}:{1}:{2}", horas, minutos , segundos );
                        bike.agora = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        bike.bike = Convert.ToInt32(dr["bicicletas_bic_id"]);
                        bike.solicitacaoId = Convert.ToInt32(dr["sol_id"]);
                        retorno = JsonConvert.SerializeObject(bike);
                    }
                    else
                    {
                        retorno = "Nenhuma";
                    }
                }    
            }
            catch (Exception)
            {
                retorno = "Algo deu errado";
            }

            return retorno;
        }


        //devolver bicicleta na estação
        [Route("Bicicleta/entregar")]
        [HttpPost]
        public BicicletaProgresso entregarBike(BicicletaProgresso p)
        {
            int travaId = 0;
            StringBuilder query = new StringBuilder();
            List<string> queryes = new List<string>();
            BicicletaProgresso bicileta = new BicicletaProgresso();
            //0- selecionar qual estação e qual trava deverá deixar a bike
            try
            {
                //busca trava livre para deixar a bike
                query.AppendLine("select trava_id from travas ");
                query.AppendLine($"where estacoes_est_id={p.estacao} ");
                query.AppendLine("and trava_status = 'D' and trava_funcionando='1' ");
                query.AppendLine("LIMIT 1 ");
                using (MySqlDataReader dr = sql.RetornaQuery(query.ToString()))
                {
                    if (dr.Read())
                    {
                        travaId = Convert.ToInt32(dr["trava_id"]);
                    }
                    else
                    {
                        bicileta.execucao = 0;
                        bicileta.msg = "Não tem travas disponíveis";                      
                    }
                }

                //1- finalizar solicitação
                query = new StringBuilder();
                query.AppendLine("UPDATE solicitacoes ");
                query.AppendLine("SET ");
                query.AppendLine($"sol_data_entrega='{DateTime.Now.ToString("yyyyMMddHHmmss")}', ");
                query.AppendLine($"sol_estacao_entrega={p.estacao}, ");
                query.AppendLine($"sol_trava_entrega={travaId} ");
                query.AppendLine($"WHERE ");
                query.AppendLine($"sol_id={p.solicitacaoId} ");
                queryes.Add(query.ToString());

                //2- inserir registro bikeXestacaoXtrava
                query = new StringBuilder();
                query.AppendLine("INSERT INTO bikeXestacaoXtrava ");
                query.AppendLine("( dthora_chegada, estacoes_est_id, bicicletas_bic_id, travas_trava_id ) ");
                query.AppendLine("VALUES ( ");
                query.AppendLine($"     '{DateTime.Now.ToString("yyyyMMddHHmmss")}' ");
                query.AppendLine($"     , {p.estacao}");
                query.AppendLine($"     ,{p.bike}");
                query.AppendLine($"     ,{travaId}");
                query.AppendLine(")");
                queryes.Add(query.ToString());

                //3- update travas
                query = new StringBuilder();
                query.AppendLine("UPDATE travas ");
                query.AppendLine(" SET ");
                query.AppendLine("   trava_status = 'I' ");
                query.AppendLine("WHERE ");
                query.AppendLine($"  trava_id={travaId}");
                queryes.Add(query.ToString());

                Boolean result = sql.ExecutaQueryTransaction(queryes.ToArray());

                if (result) {
                    bicileta.execucao = 1;
                    bicileta.msg = $"Guarde a bicicleta na trava de número: {travaId} ";
                }else {
                    bicileta.execucao = 0;
                    bicileta.msg = "Ocorreu um erro ao liberar"; 
                    
                }

            }
            catch (Exception ex)
            {
                bicileta.execucao = 0;
                bicileta.msg = ex.ToString();
            }

            return bicileta;
            
        }

               

        //****funções auxiliares*********

        private int getPlano(int voucher)
        {
            int plano = 0;

            using (MySqlDataReader dr = sql.RetornaQuery($"select pna_id  from  plano_ativo where pna_voucher={voucher} and pna_status=1 and pna_dthora_expira>='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' "))
            {
                if (dr.Read())
                {
                    plano = Convert.ToInt32(dr["pna_id"]);
                }
            }

            return plano;
        }

        private string liberarBike(SolBike sol, int plano)
        {
            int travaId = 0;
            int bikeId = 0;
            int bxe = 0;
            StringBuilder query = new StringBuilder();
            List<string> ListaQueryes = new List<string>();

            try
            {                            
            //busca trava com bicicleta para liberar
            query.AppendLine("select trava_id from travas ");
            query.AppendLine($"where estacoes_est_id={sol.estacao} ");
            query.AppendLine("and trava_status = 'I' and trava_funcionando='1'  ");
            query.AppendLine("LIMIT 1 ");

            using (MySqlDataReader dr = sql.RetornaQuery(query.ToString()))
            {
                if (dr.Read()) {
                    travaId = Convert.ToInt32(dr["trava_id"]);
                } else
                {
                    return "Não tem bikes disponíveis";
                }
            }

            //obtemos a bike id
            query = new StringBuilder();
            query.AppendLine("select * from bikeXestacaoXtrava ");
            query.AppendLine("where ");
            query.AppendLine($"travas_trava_id={travaId} ");
            query.AppendLine("and dthora_saida is null ");

            using (MySqlDataReader dr = sql.RetornaQuery(query.ToString()))
            {
                if (dr.Read())
                {
                    bikeId = Convert.ToInt32(dr["bicicletas_bic_id"]);
                    bxe = Convert.ToInt32(dr["id_bxe"]);
                    }
                else
                {
                    return "bike não encontrada";
                }
            }

            //cadastramos a solicitacao
            query = new StringBuilder();
            query.AppendLine("INSERT INTO ");
            query.AppendLine("  solicitacoes ");
            query.AppendLine("(sol_data_retirada, bicicletas_bic_id, sol_estacao_retirada, plano_ativo_pna_id, sol_trava_retirada ) ");
            query.AppendLine("VALUES ");
            query.AppendLine("( ");
            query.AppendLine($"'{DateTime.Now.ToString("yyyyMMddHHmmss")}'");
            query.AppendLine($",{bikeId}");
            query.AppendLine($",{sol.estacao}");
            query.AppendLine($",{plano} ");
            query.AppendLine($",{travaId} ");
            query.AppendLine(") ");
            ListaQueryes.Add(query.ToString());

            //atualiza trava
            query = new StringBuilder();
            query.AppendLine("UPDATE travas ");
            query.AppendLine(" SET ");
            query.AppendLine("   trava_status = 'D' ");
            query.AppendLine("WHERE ");
            query.AppendLine($"  trava_id={travaId}");
            ListaQueryes.Add(query.ToString());

            //atualiza histórico  de bikexestacaoxtrava
            query = new StringBuilder();
            query.AppendLine("UPDATE bikeXestacaoXtrava ");
            query.AppendLine($"SET dthora_saida={DateTime.Now.ToString("yyyyMMddHHmmss")} ");
            query.AppendLine("WHERE ");
            query.AppendLine($"id_bxe={bxe}");
            ListaQueryes.Add(query.ToString());


                Boolean result = sql.ExecutaQueryTransaction(ListaQueryes.ToArray());

            if (result){
                    return "liberada";
            } else {
                    return "ocorreu um erro ao liberar";
            }
                
            }
            catch (Exception)
            {
                return "Ocorreu um erro";
            }
                        
        }

    }
      

}
