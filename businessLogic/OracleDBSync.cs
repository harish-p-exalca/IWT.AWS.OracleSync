using IWT.OracleSync.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
        public void GetOracleData()
        {
            string queryForFirst = "select * from Gate_Entry where GINDT IS NOT NULL AND FIRSTWT IS NULL AND STATUS_FLAG IS NULL AND CFLAG = 'F'";
            DataTable filteredTable1 = _dbContext.GetAllData(queryForFirst);
            string JSONString1 = JsonConvert.SerializeObject(filteredTable1);
            oracleData = JsonConvert.DeserializeObject<List<OracleModel>>(JSONString1);
            if (oracleData != null && oracleData.Count > 0)
            {
                InsertIntoRFIDAllocations();
            }
            string queryForSecond = "select * from Gate_Entry where FIRSTWT IS NOT NULL AND STATUS_FLAG = 'S' AND CFLAG = 'F'";
            DataTable filteredTable2 = _dbContext.GetAllData(queryForSecond);
            string JSONString2 = JsonConvert.SerializeObject(filteredTable2);
            oracleData = JsonConvert.DeserializeObject<List<OracleModel>>(JSONString2);
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
                DataTable table = _sqlDb.GetAllData($"select * from RFID_Allocations where TransId={model.TransId}");
                string JSONString = JsonConvert.SerializeObject(table);
                var rfIdAllocation = JsonConvert.DeserializeObject<List<RFIDAllocation>>(JSONString);
                if (rfIdAllocation == null || rfIdAllocation.Count == 0)
                {
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
                                            1,
                                            '{model.VEHICLE_NUMBER}',
                                            '{model.MATERIAL_CODE}',
                                            '{model.MATERIAL_DESCRIPTION}',
                                            '{model.SUPPLIER_CODE}',
                                            '{model.SUPPLIER_DESCRIPTION}',
                                            '0',
                                            'Temporary',
                                            '{DateTime.Now.AddDays(2).ToString("yyyy-MM-dd HH:mm:ss")}',
                                            '{model.RFIDTAGUID}',
                                            'In-Transit',
                                            '[]',
                                            '',
                                            '6100000050',
                                            '0',
                                            '0',
                                            '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',
                                            '{JsonConvert.SerializeObject(model)}',
                                            'Open',
                                            '{model.TransId}');SELECT SCOPE_IDENTITY();";
                    SqlCommand cmd = new SqlCommand(insertQuery);

                    _sqlDb.InsertData(cmd, CommandType.Text);
                }                
            }
        }

        public void UpdateFirstTransactionOracleData()
        {
            DataTable table = _sqlDb.GetAllData($@"select ge.[TransType], ge.[AllocationId], ge.[TransId], tr.[EmptyWeight], tr.[LoadWeight], tr.[EmptyWeightDate], tr.[LoadWeightDate], tr.[EmptyWeightTime], tr.[LoadWeightTime],
                                tr.[NetWeight], tr.[TicketNo], tr.[State], tr.[SystemID] from[RFID_Allocations] ge inner join [Transaction] tr on ge.AllocationId = tr.RFIDAllocation where ge.OracleStatus='First'");
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
                            firstWeightDate = data.LoadWeightDate.ToString();
                            firstWeightTime = data.LoadWeightTime.ToString();
                            firstWeight = data.LoadWeight.ToString();
                        }
                        else
                        {
                            firstWeightDate = data.EmptyWeightDate.ToString();
                            firstWeightTime = data.EmptyWeightTime.ToString();
                            firstWeight = data.EmptyWeight.ToString();
                        }
                        string camera1 = $"{firstCamera}\\{data.TicketNo}_{data.State}_cam{"1"}.jpeg";
                        string updateQuery = $@"UPDATE [Gate_Entry] SET FIRSTWTDT='{firstWeightDate}', FIRSTWTTM='{firstWeightTime}', FIRSTWT='{firstWeight}', STATUS_FLAG='S', WBNO_F='{data.SystemID}', IMAGENO1='{camera1}' WHERE TransId={data.TransId}";
                        SqlCommand cmd = new SqlCommand(updateQuery);
                        var response = _dbContext.ExecuteQuery(cmd);
                        if (response)
                        {
                            string updateQueryRFIDAllocation = $@"UPDATE [RFID_Allocations] SET OracleStatus='First Updated' WHERE TransId={data.TransId}";
                            SqlCommand cmdRFIDAllocation = new SqlCommand(updateQueryRFIDAllocation);
                            _sqlDb.ExecuteQuery(cmdRFIDAllocation);
                        }
                        else
                        {
                            throw new Exception("Something Went Wrong");
                        }
                    }                 
                    
                }
            }
        }

        public void UpdateSecondTransactionOracleData()
        {
            DataTable table = _sqlDb.GetAllData($@"select ge.[TransType], ge.[AllocationId], ge.[TransId], tr.[EmptyWeight], tr.[LoadWeight], tr.[EmptyWeightDate], tr.[LoadWeightDate], tr.[EmptyWeightTime], 
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
                        if (data.TransType == "Inbound")
                        {
                            secondWeightDate = data.LoadWeightDate.ToString();
                            secondWeightTime = data.LoadWeightTime.ToString();
                            secondWeight = data.LoadWeight.ToString();
                        }
                        else
                        {
                            secondWeightDate = data.EmptyWeightDate.ToString();
                            secondWeightTime = data.EmptyWeightTime.ToString();
                            secondWeight = data.EmptyWeight.ToString();
                        }
                        string camera2 = $"{secondCamera}\\{data.TicketNo}_{data.State}_cam{"1"}.jpeg";
                        string updateQuery = $@"UPDATE [Gate_Entry] SET SECONDWTDT='{secondWeightDate}', SECONDWTTM='{secondWeightTime}', SECONDWT='{secondWeight}', STATUS_FLAG='C', NETWT='{data.NetWeight}', WBNO_S='{data.SystemID}', IMAGENO2='{camera2}' WHERE TransId={data.TransId}";
                        SqlCommand cmd = new SqlCommand(updateQuery);
                        var response = _dbContext.ExecuteQuery(cmd);
                        if (response)
                        {
                            string updateQueryRFIDAllocation = $@"UPDATE [RFID_Allocations] SET OracleStatus='Second Updated' WHERE TransId={data.TransId}";
                            SqlCommand cmdRFIDAllocation = new SqlCommand(updateQueryRFIDAllocation);
                            _sqlDb.ExecuteQuery(cmdRFIDAllocation);
                        }
                        else
                        {
                            throw new Exception("Something Went Wrong");
                        }
                    }                    
                }
            }
        }
    }
}
