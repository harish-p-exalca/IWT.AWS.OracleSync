﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OracleClient;

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

        //public DataTable GetAllData
        //   (string SQL)
        //{
        //    try
        //    {

        //        DataTable dt = new DataTable();
        //        SqlConnection con = new SqlConnection(ConnectionString);
        //        SqlCommand cmd = new SqlCommand(SQL);
        //        cmd.Connection = con;
        //        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        //        {
        //            da.Fill(dt);
        //        }
        //        cmd.Dispose();
        //        con.Close();
        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog.WriteToFile("GetAllData:" + ex.Message);
        //        return null;
        //    }
        //}

        //public bool ExecuteQuery(SqlCommand command)
        //{
        //    try
        //    {
        //        SqlConnection con = new SqlConnection(ConnectionString);
        //        con.Open();
        //        try
        //        {
        //            command.Connection = con;
        //            command.ExecuteNonQuery();
        //        }
        //        catch (Exception exp)
        //        {
        //            throw exp;
        //        }
        //        con.Close();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog.WriteToFile("ExecuteQuery : " + ex.Message);
        //        return false;
        //    }
        //}

        //public void InsertData(SqlCommand cmd, CommandType commandType = CommandType.StoredProcedure)
        //{
        //    try
        //    {
        //        SqlConnection con = new SqlConnection(GetDecryptedConnectionStringDB());
        //        cmd.Connection = con;
        //        cmd.CommandType = commandType;
        //        con.Open();
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        con.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog.WriteToFile("InsertData:" + ex.Message);
        //    }
        //}

        public DataTable GetAllData
           (string SQL)
        {
            try
            {

                DataTable dt = new DataTable();
                OracleConnection con = new OracleConnection(ConnectionString);
                OracleCommand cmd = new OracleCommand(SQL);
                cmd.Connection = con;
                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
                cmd.Dispose();
                con.Close();
                return dt;
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("OracleDB/GetAllData:" + ex.Message);
                return null;
            }
        }

        public bool ExecuteQuery(OracleCommand command)
        {
            try
            {
                OracleConnection con = new OracleConnection(ConnectionString);
                con.Open();
                try
                {
                    command.Connection = con;
                    command.ExecuteNonQuery();
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
                WriteLog.WriteToFile("OracleDB/ExecuteQuery : " + ex.Message);
                return false;
            }
        }

        public void InsertData(OracleCommand cmd, CommandType commandType = CommandType.StoredProcedure)
        {
            try
            {
                OracleConnection con = new OracleConnection(GetDecryptedConnectionStringDB());
                cmd.Connection = con;
                cmd.CommandType = commandType;
                con.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteToFile("OracleDB/InsertData:" + ex.Message);
            }
        }
    }
}
