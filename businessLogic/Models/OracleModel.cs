using System;

namespace IWT.OracleSync.Business
{
    public class OracleModel
    {
        public int TransId { get; set; }
        public string RFIDTAGUID { get; set; }
        public string FIRSTWTDT { get; set; }
        public string FIRSTWTTM { get; set; }
        public string FIRSTWT { get; set; }
        public string SECONDWTDT { get; set; }
        public string SECONDWTTM { get; set; }
        public string SECONDWT { get; set; }
        public string NETWT { get; set; }
        public int WBTOLLMIN { get; set; }
        public int WBTOLLMAX { get; set; }
        public string WBNO_F { get; set; }
        public string WBNO_S { get; set; }
        public string IMAGENO1 { get; set; }
        public string IMAGENO2 { get; set; }
        public string STATUS_FLAG { get; set; }
        public string GINDT { get; set; }
        public string CFLAG { get; set; }
        public string VEHICLE_NUMBER { get; set;}
        public string MATERIAL_CODE { get; set;}
        public string MATERIAL_DESCRIPTION { get; set;}
        public string SUPPLIER_CODE { get; set;}
        public string SUPPLIER_DESCRIPTION { get; set;}
        public string TRANSTYPE { get; set; }
        public string IMAGENO3 { get; set;}
        public string IMAGENO4 { get; set;}
        public int? PARTYWT { get; set;}
        public string AFLAG { get; set; }
    }

    public class ERRLOGS
    {
        public int Id { get; set; }
        public int TRANS_NO { get; set; }
        public string VEH_NO { get; set; }
        public string ERRDES { get; set; }
        public DateTime? ERRDATE { get; set; }
        public string RFIDID { get; set; }
        public string ERRTIME { get; set; }
        public string WB_NO { get; set; }
    }
}
