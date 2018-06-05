using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;
using BikeSJC.Models;
using BikeSJC.Database;
using System.Configuration;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text;

namespace BikeSJC.Controllers
{
    public class UsuarioController : ApiController
    {
        Connect sql = new Connect();

        [Route("Usuario/login")]
        [HttpPost]
        public Usuario login(Usuario usuario)
        {
            var user = new Usuario();
            try
            {
                string query = "SELECT * from usuarios u join creditos c on c.usuarios_usu_id = u.usu_id where usu_usuario=@usuario and usu_senha=@senha";
                // 1. define os parêmetros usados no objeto command
                List<MySqlParameter> lisPar = new List<MySqlParameter>();
                // 2. inclui um novo parâmetro ao comando
                lisPar.Add(new MySqlParameter("@usuario", usuario.usuario.ToString()));
                lisPar.Add(new MySqlParameter("@senha", usuario.senha.ToString()));

                MySqlDataReader rdr = sql.RetornaQueryParam(query, lisPar);
                if (rdr.Read())
                    {
                        user.usuario = rdr["usu_usuario"].ToString();
                        user.nome = rdr["usu_nome"].ToString();
                        user.senha = rdr["usu_senha"].ToString();
                        user.status = rdr["usu_status"].ToString();
                        user.id = Convert.ToInt32(rdr["usu_id"]);
                        user.credito = Convert.ToInt32(rdr["cre_valor"]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return user;
        }



        [Route("Usuario/cadastro")]
        [HttpPost]
        public Boolean Post(Usuario user)
        {
            var ret = true;
            StringBuilder query = new StringBuilder();
            List<string> queryes = new List<string>();

            query.AppendLine("INSERT INTO usuarios ( ");
            query.AppendLine("usu_usuario, usu_senha, usu_nome, usu_email, usu_status, tipo, usu_cpf, usu_data_cadastro ");
            query.AppendLine(") VALUES ( ");
            query.AppendLine("'" + user.usuario + "', ");
            query.AppendLine("'" + user.senha + "', ");
            query.AppendLine("'" + user.nome + "', ");
            query.AppendLine("'" + user.email + "', ");
            query.AppendLine("'A', ");
            query.AppendLine("'U', ");
            query.AppendLine("'" + user.cpf + "', ");
            query.AppendLine("'" + DateTime.Now + "' ");
            query.AppendLine(" ) ");       
            queryes.Add(query.ToString());

            //insere créditos
            query = new StringBuilder();
            query.AppendLine("INSERT INTO creditos (usuarios_usu_id) (SELECT MAX(usu_id) AS prox from usuarios) ");
            queryes.Add(query.ToString());

            try
            {
                sql.ExecutaQueryTransaction(queryes.ToArray());
            } catch
            {
                ret = false;
            }
            
            return ret;
        }

        [Route("Usuario/credito")]
        [HttpPost]
        public Boolean comprarCredito([FromBody]int valor, int user)
        {
            var ret = true;
            StringBuilder query = new StringBuilder();
            int saldoAtual = retornarCredito(user);
            valor = valor + saldoAtual;
            query.AppendLine("UPDATE creditos SET ");
            query.AppendLine($"cre_valor={valor} ");
            query.AppendLine("WHERE ");
            query.AppendLine($"usuarios_usu_id={user}");
                      
            try
            {
                sql.ExecutaQuery(query.ToString());
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        [Route("Usuario/saldo")]
        [HttpPost]
        public int retornarCredito(int user)
        {
            var ret = 0;
            StringBuilder query = new StringBuilder();

            query.AppendLine("SELECT * FROM creditos  ");
            query.AppendLine("WHERE ");
            query.AppendLine($"usuarios_usu_id={user}");
           
            try
            {
                using (MySqlDataReader dr = sql.RetornaQuery(query.ToString()))
                {
                    if (dr.Read()){
                        ret = Convert.ToInt32(dr["cre_valor"]);
                    }
                }
            }
            catch
            {
                ret = -1;
            }

            return ret;
        }

        // histórico de uso das bicicletas
        [Route("Usuario/historico")]
        [HttpGet]
        public List<BicicletaHistorico> retornaHistoricoBike(int usuario)
        {
            List<BicicletaHistorico> listHitorico = new List<BicicletaHistorico>();

            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT    ");
            query.AppendLine("  sol_data_retirada ");
            query.AppendLine("  ,TIMEDIFF(sol_data_entrega, sol_data_retirada ) AS tempo");
            query.AppendLine("  ,e.est_nome as NomeEstRetirada");
            query.AppendLine("  ,e2.est_nome as NomeEstEntregue");
            query.AppendLine("FROM solicitacoes so  ");
            query.AppendLine("JOIN plano_ativo p on p.pna_id = so.plano_ativo_pna_id    ");
            query.AppendLine("JOIN estacoes e on e.est_id = so.sol_estacao_retirada ");
            query.AppendLine("JOIN estacoes e2 on e2.est_id = so.sol_estacao_entrega    ");
            query.AppendLine("WHERE ");
            query.AppendLine($"  usuarios_usu_id={usuario}");
            query.AppendLine("  ORDER BY sol_data_retirada DESC");

            using (MySqlDataReader dr = sql.RetornaQuery(query.ToString()))
            {

                while (dr.Read())
                {
                    BicicletaHistorico bh = new BicicletaHistorico();
                    bh.dataRetirada = Convert.ToDateTime(dr["sol_data_retirada"]).ToString("dd/MM/yyyy HH:mm:ss");
                    bh.estacaoE = dr["NomeEstRetirada"].ToString();
                    bh.estacaoR = dr["NomeEstEntregue"].ToString();
                    bh.tempo = dr["tempo"].ToString();
                    listHitorico.Add(bh);
                }

            }

            return listHitorico;
        }
    }
}
