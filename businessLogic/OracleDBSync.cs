using IWT.OracleSync.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace IWT.OracleSync.Business
{
    public class OracleDBSync
    {
        List<OracleModel> oracleData;
        public List<OracleModel> getOracleData()
        {
            OracleDB _dbContext = new OracleDB();
            string query = "select * from Gate_Entry where GINDT IS NOT NULL AND FIRSTWT IS NULL AND STATUS_FLAG IS NULL AND CFLAG = 'F'";
            DataTable filteredTable = _dbContext.GetAllData(query);
            string JSONString = JsonConvert.SerializeObject(filteredTable);
            oracleData = JsonConvert.DeserializeObject<List<OracleModel>>(JSONString);
            return oracleData;
        }

        public void InsertIntoRFIDAllocations()
        {
            SqlDB _sqlDb = new SqlDB();
            foreach (OracleModel model in oracleData)
            {
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
                                                                        CreatedOn) 
                                    Values ('Inbound',
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
                                            '{DateTime.Now.AddDays(2)}',
                                            '225 40 21',
                                            'In-Transit',
                                            '[]',
                                            '',
                                            '6100000050',
                                            '0',
                                            '0',
                                            '{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")}');SELECT SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(insertQuery);

                _sqlDb.InsertData(cmd, CommandType.Text);
            }

        }
    }
}
