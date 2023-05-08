using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.OracleSync.Data
{
    public class OracleDB
    {
        private string ConnectionString { get; set; }
        public OracleDB()
        {

            ConnectionString = GetDecryptedConnectionStringDB();

        }
        public string GetDecryptedConnectionStringDB()
        {
            try
            {

                byte[] b = Convert.FromBase64String(ConfigurationManager.ConnectionStrings["oracleDbConnection"].ConnectionString);
                string decryptedConnectionString = System.Text.ASCIIEncoding.ASCII.GetString(b);
                return decryptedConnectionString;
            }
            catch (Exception ex)
            {

                Byte[] b1 = System.Text.ASCIIEncoding.ASCII.GetBytes(ConfigurationManager.ConnectionStrings["oracleDbConnection"].ConnectionString);
                string encryptedConnectionString = Convert.ToBase64String(b1);
                var decrypted = System.Text.ASCIIEncoding.ASCII.GetString(b1);
                return decrypted;
            }
        }

        public DataTable GetAllData
           (string SQL)
        {
            try
            {
                
                DataTable dt = new DataTable();
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlCommand cmd = new SqlCommand(SQL);
                cmd.Connection = con;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
                cmd.Dispose();
                con.Close();
                return dt;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("GetAllData:" + ex.Message);
                return null;
            }
        }

        public bool ExecuteQuery(string query)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                con.Open();
                try
                {
                    new SqlCommand(query, con).ExecuteNonQuery();
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("ExecuteQuery : " + ex.Message);
                return false;
            }
        }
    }
}
