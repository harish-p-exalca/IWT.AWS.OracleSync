using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using IWT.OracleSync.Data;
using IWT.OracleSync.Business;
using Newtonsoft.Json;

namespace IWT.OracleSync.Console
{
    internal class Program
    {
        static void Main()
        {
                OracleDBSync oracleSync = new OracleDBSync();
                oracleSync.getOracleData();
                oracleSync.InsertIntoRFIDAllocations();
        }

    }
}
