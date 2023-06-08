using IWT.OracleSync.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;

namespace IWT.OracleSync.Business
{
    public class OracleDBSync
    {
        List<OracleModel> oracleData;
        SqlDB _sqlDb = new SqlDB();
        OracleDB _dbContext = new OracleDB();
        string firstCamera = ConfigurationManager.AppSettings["Camera1"];
        string secondCamera = ConfigurationManager.AppSettings["Camera2"];
        string thirdCamera = ConfigurationManager.AppSettings["Camera3"];
        string fourthCamera = ConfigurationManager.AppSettings["Camera4"];
        public void GetOracleData()
        {
            WriteLog.WriteToFile("Fetching data from Oracle database transaction table");
            string queryForFirst = "select * from TRANS where GINDT IS NOT NULL AND FIRSTWT IS NULL AND STATUS_FLG IN (NULL,'F','S') AND CFLAG = 'F'";
            DataTable filteredTable1 = _dbContext.GetData(queryForFirst);
            string JSONString1 = JsonConvert.SerializeObject(filteredTable1);
            oracleData = JsonConvert.DeserializeObject<List<OracleModel>>(JSONString1);
            WriteLog.WriteToFile($"Number of records found :- {oracleData.Count}");
            if (oracleData != null && oracleData.Count > 0)
            {
                InsertIntoRFIDAllocations();
            }
            UpdateFirstTransactionOracleData();
            UpdateSecondTransactionOracleData();
        }

        public void InsertIntoRFIDAllocations()
        {
            foreach (OracleModel model in oracleData)
            {
                
                DataTable table = _sqlDb.GetData($"select * from RFID_Allocations where TransId={model.TRANSNO}");
                string JSONString = JsonConvert.SerializeObject(table);
                var rfIdAllocation = JsonConvert.DeserializeObject<List<RFIDAllocation>>(JSONString);
                if (rfIdAllocation == null || rfIdAllocation.Count == 0)
                {
                    WriteLog.WriteToFile($"Inserting data into AWS Gate Entry for {model.TRANSNO}");
                    if (model.TRANSTYPE.Trim() == "R")
                    {
                        model.TRANSTYPE = "Inbound";
                    }
                    else if (model.TRANSTYPE.Trim() == "C")
                    {
                        model.TRANSTYPE = "Outbound";
                    }
                    string insertQuery = $@"INSERT INTO [RFID_Allocations] (TransType,
                                                                        IsSapBased,
                                                                        DocNumber,
                                                                        TransMode,
                                                                        IsLoaded,
                                                                        VehicleNumber,
                                                                        MaterialCode,
                                                                        MaterialName,
                                                                        SupplierCode,
                                                                        SupplierName,
                                                                        TareWeight,
                                                                        AllocationType,
                                                                        ExpiryDate,
                                                                        RFIDTag,
                                                                        [Status],
                                                                        CustomFieldValues,
                                                                        GatePassNumber,
                                                                        TokenNumber,
                                                                        NoOfMaterial,
                                                                        GatePassId,
                                                                        CreatedOn,
                                                                        OracleData,
                                                                        OracleStatus,
                                                                        TransId) 
                                    Values ('{model.TRANSTYPE}',
                                            0,
                                            '',
                                            'FT',
                                            '{model.TRANSTYPE == "Inbound"}',
                                            '{model.VEHINO}',
                                            '{model.PRODCODE}',
                                            '{model.PRODDESC}',
                                            '{model.SUPPCODE}',
                                            '{model.SUPPDESC}',
                                            '0',
                                            'Temporary',
                                            '{DateTime.Now.AddDays(2).ToString("yyyy-MM-dd HH:mm:ss")}',
                                            '{model.RFIDTAGUID}',
                                            'In-Transit',
                                            '[]',
                                            '',
                                            '',
                                            '0',
                                            '0',
                                            '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
                                            '{JsonConvert.SerializeObject(model)}',
                                            'Open',
                                            '{model.TRANSNO}');SELECT SCOPE_IDENTITY();";
                    _sqlDb.ExecuteQuery(insertQuery);
                }
                else
                {
                    string updateQuery = $@"update [RFID_Allocations] set OracleData='{JsonConvert.SerializeObject(model)}' where AllocationId={rfIdAllocation[0].AllocationId};";
                    _sqlDb.ExecuteQuery(updateQuery);
                }
            }
        }

        public void UpdateFirstTransactionOracleData()
        {
            WriteLog.WriteToFile("Fetching first transaction records");
            DataTable table = _sqlDb.GetData($@"select ge.[TransType], ge.[AllocationId], ge.[VehicleNumber], ge.[TransId], ge.[FTError], ge.[STError], ge.[FTErrorDate], ge.[STErrorDate], tr.[EmptyWeight], tr.[LoadWeight], tr.[EmptyWeightDate], tr.[LoadWeightDate], tr.[EmptyWeightTime], tr.[LoadWeightTime],
                                tr.[NetWeight], tr.[TicketNo], tr.[State], tr.[SystemID] from [RFID_Allocations] ge inner join [Transaction] tr on ge.AllocationId = tr.RFIDAllocation where ge.OracleStatus='First'");
            string JSONString = JsonConvert.SerializeObject(table);
            var oracleData = JsonConvert.DeserializeObject<List<RFIDAllocationWithTrans>>(JSONString);
            if (oracleData != null && oracleData.Count > 0)
            {
                string firstWeightDate;
                string firstWeightTime;
                string firstWeight;
                foreach (RFIDAllocationWithTrans data in oracleData)
                {
                    if (!string.IsNullOrEmpty(data.TransType))
                    {
                        if (data.TransType == "Inbound")
                        {
                            firstWeightDate = data.LoadWeightDate.Value.Date.ToString("dd-MMM-yyyy");
                            firstWeightTime = data.LoadWeightTime.ToString();
                            firstWeight = data.LoadWeight.ToString();
                        }
                        else
                        {
                            firstWeightDate = data.EmptyWeightDate.Value.Date.ToString("dd-MMM-yyyy");
                            firstWeightTime = data.EmptyWeightTime.ToString();
                            firstWeight = data.EmptyWeight.ToString();
                        }
                        WriteLog.WriteToFile("Updating first transaction records");
                        string camera1 = $"{firstCamera}\\{data.TicketNo}_{data.State}_cam{"1"}.jpeg";
                        string camera3 = $"{thirdCamera}\\{data.TicketNo}_{data.State}_cam{"2"}.jpeg";
                        string updateQuery = $@"UPDATE [TRANS] SET FIRSTWTDT='{firstWeightDate}', FIRSTWTTM='{firstWeightTime}', FIRSTWT='{firstWeight}', STATUS_FLG='S', WBNO_F='{data.SystemID}', IMAGENO1='{camera1}', IMAGENO3='{camera3}' WHERE TransId={data.TransId}";
                        var response = _dbContext.ExecuteQuery(updateQuery);
                        if (response)
                        {
                            string updateQueryRFIDAllocation = $@"UPDATE [RFID_Allocations] SET OracleStatus='First Updated' WHERE TransId={data.TransId}";
                            _sqlDb.ExecuteQuery(updateQueryRFIDAllocation);
                        }
                        else
                        {
                            throw new Exception("Something Went Wrong");
                        }
                    }
                    

                }
            }
            DataTable table1 = _sqlDb.GetData($@"select ge.[TransType], ge.[AllocationId], ge.[VehicleNumber], ge.[TransId], ge.[FTError], ge.[STError], ge.[FTErrorDate], ge.[STErrorDate], tr.[EmptyWeight], tr.[LoadWeight], tr.[EmptyWeightDate], tr.[LoadWeightDate], tr.[EmptyWeightTime], tr.[LoadWeightTime],
                                tr.[NetWeight], tr.[TicketNo], tr.[State], tr.[SystemID] from [RFID_Allocations] ge inner join [Transaction] tr on ge.AllocationId = tr.RFIDAllocation where ge.OracleStatus='Open' and ge.IsError=1");
            string JSONString1 = JsonConvert.SerializeObject(table1);
            var oracleData1 = JsonConvert.DeserializeObject<List<RFIDAllocationWithTrans>>(JSONString1);
            if (oracleData1 != null && oracleData1.Count > 0)
            {
                foreach (var data in oracleData1)
                {
                    var errLog = new ERRLOGS
                    {
                        TRANS_NO = data.TransId.Value,
                        VEH_NO = data.VehicleNumber,
                        ERRDES = data.FTError,
                        ERRDATE = data.FTErrorDate.Value,
                        ERRTIME = data.FTErrorDate.Value.ToShortTimeString(),
                        RFIDID = data.RFIDTag,
                        WB_NO = data.SystemID,
                    };
                    InsertErrorDetails(errLog,data.TransId);
                }
            }
        }

        public void UpdateSecondTransactionOracleData()
        {
            WriteLog.WriteToFile("Fetching second transaction records");
            DataTable table = _sqlDb.GetData($@"select ge.[TransType], ge.[AllocationId],ge.[VehicleNumber], ge.[TransId], ge.[FTError], ge.[STError],ge.[FTErrorDate], ge.[STErrorDate], tr.[EmptyWeight], tr.[LoadWeight], tr.[EmptyWeightDate], tr.[LoadWeightDate], tr.[EmptyWeightTime], 
                              tr.[LoadWeightTime], tr.[NetWeight], tr.[TicketNo], tr.[State], tr.[SystemID] from[RFID_Allocations] ge inner join [Transaction] tr on ge.AllocationId = tr.RFIDAllocation where ge.OracleStatus='Second'");
            string JSONString = JsonConvert.SerializeObject(table);
            var oracleData = JsonConvert.DeserializeObject<List<RFIDAllocationWithTrans>>(JSONString);
            if (oracleData != null && oracleData.Count > 0)
            {
                string secondWeightDate;
                string secondWeightTime;
                string secondWeight;
                foreach (RFIDAllocationWithTrans data in oracleData)
                {
                    if (!string.IsNullOrEmpty(data.TransType))
                    {
                        if (data.TransType == "Outbound")
                        {
                            secondWeightDate = data.LoadWeightDate.Value.Date.ToString("dd-MMM-yyyy");
                            secondWeightTime = data.LoadWeightTime.ToString();
                            secondWeight = data.LoadWeight.ToString();
                        }
                        else
                        {
                            secondWeightDate = data.EmptyWeightDate.Value.Date.ToString("dd-MMM-yyyy");
                            secondWeightTime = data.EmptyWeightTime.ToString();
                            secondWeight = data.EmptyWeight.ToString();
                        }
                        WriteLog.WriteToFile("Updating second transaction records");
                        string camera2 = $"{secondCamera}\\{data.TicketNo}_{data.State}_cam{"1"}.jpeg";
                        string camera4 = $"{fourthCamera}\\{data.TicketNo}_{data.State}_cam{"2"}.jpeg";
                        string updateQuery = $@"UPDATE [TRANS] SET SECONDWTDT='{secondWeightDate}', SECONDWTTM='{secondWeightTime}', SECONDWT='{secondWeight}', STATUS_FLG='C', NETWT='{data.NetWeight}', WBNO_S='{data.SystemID}', IMAGENO2='{camera2}', IMAGENO4='{camera4}' WHERE TransId={data.TransId}";
                        var response = _dbContext.ExecuteQuery(updateQuery);
                        if (response)
                        {
                            string updateQueryRFIDAllocation = $@"UPDATE [RFID_Allocations] SET OracleStatus='Second Updated' WHERE TransId={data.TransId}";
                            _sqlDb.ExecuteQuery(updateQueryRFIDAllocation);
                        }
                        else
                        {
                            throw new Exception("Something Went Wrong");
                        }
                    }                    
                }
            }

            DataTable table2 = _sqlDb.GetData($@"select ge.[TransType], ge.[AllocationId],ge.[VehicleNumber], ge.[TransId], ge.[FTError], ge.[STError],ge.[FTErrorDate], ge.[STErrorDate], tr.[EmptyWeight], tr.[LoadWeight], tr.[EmptyWeightDate], tr.[LoadWeightDate], tr.[EmptyWeightTime], 
                              tr.[LoadWeightTime], tr.[NetWeight], tr.[TicketNo], tr.[State], tr.[SystemID] from[RFID_Allocations] ge inner join [Transaction] tr on ge.AllocationId = tr.RFIDAllocation where ge.OracleStatus='First'");
            string JSONString2 = JsonConvert.SerializeObject(table2);
            var oracleData2 = JsonConvert.DeserializeObject<List<RFIDAllocationWithTrans>>(JSONString2);
            if (oracleData2 != null && oracleData2.Count > 0)
            {
                foreach (var data in oracleData2)
                {
                    if (!string.IsNullOrEmpty(data.STError))
                    {
                        var errLog = new ERRLOGS
                        {
                            TRANS_NO = data.TransId.Value,
                            VEH_NO = data.VehicleNumber,
                            ERRDES = data.STError,
                            ERRDATE = data.STErrorDate.Value,
                            ERRTIME = data.STErrorDate.Value.ToShortTimeString(),
                            RFIDID = data.RFIDTag,
                            WB_NO = data.SystemID,
                        };
                        InsertErrorDetails(errLog, data.TransId);
                    }
                }
            }
        }

        public void InsertErrorDetails(ERRLOGS errLogs,int? transId)
        {
            string insertQuery = $@"INSERT INTO [ERRLOGS] (
                                                            TRANS_NO,
                                                            VEH_NO,    
                                                            ERRDES,     
                                                            ERRDATE,    
                                                            RFIDID,    
                                                            ERRTIME,    
                                                            WB_NO                                                          
                                                          ) values 
                                                          (
                                                            '{errLogs.TRANS_NO}',
                                                            '{errLogs.VEH_NO}',
                                                            '{errLogs.ERRDES}',
                                                            '{errLogs.ERRDATE}',
                                                            '{errLogs.RFIDID}',
                                                            '{errLogs.ERRTIME}',
                                                            '{errLogs.WB_NO}') ";
            _dbContext.ExecuteQuery(insertQuery);
            string updateQueryRFIDAllocation = $@"UPDATE [RFID_Allocations] SET IsError='0' WHERE TransId={transId}";
            _sqlDb.ExecuteQuery(updateQueryRFIDAllocation);
        }
    }
}
