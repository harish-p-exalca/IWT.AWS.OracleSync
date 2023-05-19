using System;

namespace IWT.OracleSync.Business
{
    public class RFIDAllocation
    {
        public int AllocationId { get; set; }
        public string GatePassId { get; set; }
        public string TransType { get; set; }
        public string IsSapBased { get; set; }
        public string DocNumber { get; set; }
        public string TransMode { get; set; }
        public string IsLoaded { get; set; }
        public string VehicleNumber { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string TareWeight { get; set; }
        public string AllocationType { get; set; }
        public string ExpiryDate { get; set; }
        public string RFIDTag { get; set; }
        public string Status { get; set; }
        public string CustomFieldValues { get; set; }
        public string GatePassNumber { get; set; }
        public string TokenNumber { get; set; }
        public string NoOfMaterial { get; set; }
        public string Remarks { get; set; }
        public string CreatedOn { get; set; }
        public string ModifiedOn { get; set; }
        public string OracleData { get; set;}
        public string OracleStatus { get; set;}
        public int? TransId { get; set; }
        public string FTError { get; set; }
        public string STError { get; set;}
    }

    public class RFIDAllocationWithTrans
    {
        public int? TransId { get; set; }
        public int AllocationId { get; set; }        
        public string TransType { get; set; }        
        public int? EmptyWeight { get; set; }
        public int? LoadWeight { get; set; }
        public DateTime? EmptyWeightDate { get; set; }
        public string EmptyWeightTime { get; set; }
        public DateTime? LoadWeightDate { get; set; }
        public string LoadWeightTime { get; set; }
        public int? NetWeight { get; set; }
        public int? TicketNo { get; set; }
        public string SystemID { get; set; }
        public string State { get; set; }
        public string FTError { get; set; }
        public string STError { get; set; }
        public DateTime? FTErrorDate { get; set; }
        public DateTime? STErrorDate { get; set;}
        public string VehicleNumber { get; set;}
        public string RFIDTag { get; set;}
    }
}
