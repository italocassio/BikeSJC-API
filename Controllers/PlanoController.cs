using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BikeSJC.Database;
using MySql.Data.MySqlClient;
using System.Text;
using BikeSJC.Models;
using BikeSJC.Controllers;

namespace BikeSJC.Controllers
{
    public class PlanoController : ApiController
    {
        Connect sql = new Connect();

        [Route("Plano/")]
        [HttpGet]
        public Plano retornaPlanoAtivo(int user)
        {
            Plano plano = new Plano();
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT * from  plano_ativo");
            query.AppendLine("WHERE");
            query.AppendLine($"usuarios_usu_id={user}");
            query.AppendLine("and pna_status=1 order by pna_id desc LIMIT 1");

            using (MySqlDataReader dr = sql.RetornaQuery(query.ToString())) {
                if(dr.Read()){
                    plano.pnaDtExpira = Convert.ToDateTime(dr["pna_dthora_expira"]).ToString("dd/MM/yyyy HH:mm:ss");
                    plano.pnaVoucher = Convert.ToInt32(dr["pna_voucher"]);
                    plano.pnaId = Convert.ToInt32(dr["pna_id"]);
                }
            }

            return plano;
        }

        [Route("Plano/comprar")]
        [HttpPost]
        public string comprarPlano(Plano plano)
        {

            //verifica se tem saldo
            UsuarioController usuario = new UsuarioController();
            int temSaldo = usuario.retornarCredito(plano.usuarioId);           
            if (temSaldo < 2 && plano.planoId == 1)
            {
                return "Saldo insuficiente!";
            }else if (temSaldo < 20 && plano.planoId == 2)
            {
                return "Saldo insuficiente!";
            }else if (temSaldo < 200 && plano.planoId == 3)
            {
                return "Saldo insuficiente!";
            }


            //verifica se veio preenchido
            if (plano.planoId == 0) return "Oops, plano não preenchido" ;
            if (plano.usuarioId == 0) return "Oops, usuario não preenchido";

            StringBuilder query = new StringBuilder();
            List<string> listQueries = new List<string>(); 
            string expiracao = retornaExpiracao(plano.planoId);
            StringBuilder voucher = new StringBuilder();
            Random rnd = new Random();

            try
            {
                //gera voucher
                for (int ctr = 1; ctr <= 4; ctr++)
                {
                    voucher.Append(rnd.Next(0, 9).ToString());
                }

                //cadastra plano
                query.AppendLine("INSERT INTO plano_ativo ");
                query.AppendLine("(pna_dthora_expira, pna_dthora_compra, pna_status, planos_pla_id, usuarios_usu_id, pna_voucher) ");
                query.AppendLine("VALUES ( ");
                query.AppendLine($"{expiracao}");
                query.AppendLine($",{DateTime.Now.ToString("yyyyMMddHHmmss")}");
                query.AppendLine(",1");
                query.AppendLine($",{plano.planoId}");
                query.AppendLine($",{plano.usuarioId}");
                query.AppendLine($",{voucher.ToString()}");
                query.AppendLine(") ");
                listQueries.Add(query.ToString());
                
                //atualiza créditos
                string queryCredito = atualizaCredito(plano.usuarioId, plano.planoId);
                listQueries.Add(queryCredito);

                sql.ExecutaQueryTransaction(listQueries.ToArray());
            }
            catch (Exception)
            {
                return "ocorreu um erro";
            }

            return "ok";
        }


        [Route("Plano/verificar")]
        [HttpPost]
        public string verificarPlano(int usuario)
        {
            string query = $"SELECT pna_id from plano_ativo where usuarios_usu_id={usuario} and pna_status=1 and pna_dthora_expira<{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            List<string> queryes = new List<string>();
            string ret = "sucesso";
            using(MySqlDataReader dr = sql.RetornaQuery(query)) {
                while (dr.Read())
                {
                    StringBuilder querie = new StringBuilder();
                    querie.AppendLine($"UPDATE plano_ativo set pna_status=0 where pna_id={dr["pna_id"]}");
                    queryes.Add(querie.ToString());
                }
            }

            try
            {
                Boolean result = sql.ExecutaQueryTransaction(queryes.ToArray());

            } catch(Exception ex)
            {
                ret = ex.ToString();
            }

            return ret;
        }

        //retorna a data de expiração do plano escolhido
        private string retornaExpiracao(int plano)
        {
            string data;

            switch (plano){
                case 1: //plano diario
                    data = DateTime.Now.AddDays(1).ToString("yyyyMMddHHmmss");
                    break;
                case 2: //plano mensal
                    data = DateTime.Now.AddMonths(1).ToString("yyyyMMddHHmmss");
                    break;
                case 3: //plano anual
                    data = DateTime.Now.AddYears(1).ToString("yyyyMMddHHmmss");
                    break;
                default: //plano inválido
                    data = DateTime.Now.ToString("yyyyMMddHHmmss");
                    break;
            }
            
            return data;
        }

        private string atualizaCredito(int usuario, int plano)
        {
            int desconto, creditoAtual = 0;
           
            switch (plano)
            {
                case 1: //plano diario
                    desconto = -2;
                    break;
                case 2: //plano mensal
                    desconto = -25;
                    break;
                case 3: //plano anual
                    desconto = -300;
                    break;
                default: //plano inválido
                    desconto = 0;
                    break;
            }

            //busca saldo atual
            using (MySqlDataReader dr = sql.RetornaQuery($"select cre_valor from creditos where usuarios_usu_id= {usuario}"))
            {
                if (dr.Read()) creditoAtual = Convert.ToInt32(dr["cre_valor"]);
            }
                       

            //monta query para atualizar
            string query = $"UPDATE  creditos set cre_valor={creditoAtual + desconto} where usuarios_usu_id= {usuario}";
            
            return query;
        }


        
    }
}
