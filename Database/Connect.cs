using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Data.Entity;
using BikeSJC.Models;
using System.Configuration;


namespace BikeSJC.Database
{
    public class Connect
    {
        // string connStr = "server=bikesjc.mysql.dbaas.com.br;user=bikesjc;database=bikesjc;port=3306;password=vaidebike;";
        string connStr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

        public MySqlDataReader RetornaQueryParam(string sql, List<MySqlParameter> listParam)
        {
            MySqlDataReader rdr;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();               
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                foreach(MySqlParameter p in listParam)
                {
                    cmd.Parameters.AddWithValue(p.ParameterName, p.Value);
                }
               
                rdr = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return rdr;
        }


        public Boolean ExecutaQueryTransactionParam(Array sql)
        {
            var ret = true;
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlTransaction tr = null;
            try
            {
                conn.Open();
                tr = conn.BeginTransaction();
                // Read array items using foreach loop  
                foreach (string str in sql)
                {
                    MySqlCommand cmd = new MySqlCommand(str, conn);
                    cmd.Transaction = tr;
                    cmd.ExecuteNonQuery();
                }

                tr.Commit();
            }
            catch
            {
                tr.Rollback();
                conn.Close();
                ret = false;
            }
            conn.Close();
            return ret;
        }




        public MySqlDataReader RetornaQuery(string sql)
        {
            MySqlDataReader rdr;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return rdr;
        }

        public Boolean ExecutaQuery(string sql)
        {
           var ret = true;
           MySqlConnection conn = new MySqlConnection(connStr);
           MySqlTransaction tr = null;
            try
                {
                    conn.Open();
                    tr = conn.BeginTransaction();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Transaction = tr;
                    cmd.ExecuteNonQuery();
                    tr.Commit();
            }
                catch 
            {
                tr.Rollback();
                conn.Close();
                ret = false;
            }
                conn.Close();
                return ret;            
        }

        public Boolean ExecutaQueryTransaction(Array sql)
        {
            var ret = true;
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlTransaction tr = null;
            try
            {
                conn.Open();
                tr = conn.BeginTransaction();
                // Read array items using foreach loop  
                foreach (string str in sql)
                {
                    MySqlCommand cmd = new MySqlCommand(str, conn);
                    cmd.Transaction = tr;
                    cmd.ExecuteNonQuery();
                }
                               
                tr.Commit();
            }
            catch 
            {
                tr.Rollback();
                conn.Close();
                ret = false;
            }
            conn.Close();
            return ret;
        }


    }
}