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
             

        [Route("Usuario/login")]
        [HttpPost]
        public Usuario login(Usuario usuario)
        {
            var user = new Usuario();
            var sql = new Connect(); 
            try
            {
                string query = "SELECT * from usuarios where usu_usuario=@usuario and usu_senha=@senha";
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
            var sql = new Connect();
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

            try
            {
                sql.ExecutaQueryTransaction(queryes.ToArray());
            } catch
            {
                ret = false;
            }
            
            return ret;
        }
    }
}
