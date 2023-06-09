using System;

namespace IWT.OracleSync.Business
{
    public class OracleModel
    {
        public decimal? TRANSNO { get; set; }
        public string RFIDTAGUID { get; set; }
        public DateTime? FIRSTWTDT { get; set; }
        public string FIRSTWTTM { get; set; }
        public decimal? FIRSTWT { get; set; }
        public DateTime? SECONDWTDT { get; set; }
        public string SECONDWTTM { get; set; }
        public decimal? SECONDWT { get; set; }
        public decimal? NETWT { get; set; }
        public decimal? WBTOLLMIN { get; set; }
        public decimal? WBTOLLMAX { get; set; }
        public decimal? WBNO_F { get; set; }
        public decimal? WBNO_S { get; set; }
        public string IMAGENO1 { get; set; }
        public string IMAGENO2 { get; set; }
        public string STATUS_FLG { get; set; }
        public DateTime? GINDT { get; set; }
        public string CFLAG { get; set; }
        public string VEHINO { get; set;}
        public string PRODCODE { get; set;}
        public string PRODDESC { get; set;}
        public string SUPPCODE { get; set;}
        public string SUPPDESC { get; set;}
        public string TRANSTYPE { get; set; }
        //public string IMAGENO3 { get; set;}
        //public string IMAGENO4 { get; set;}
        public decimal? PARTYWT { get; set;}
        public string AFLAG { get; set; }
    }

    public class ERRLOGS
    {
        public decimal? TRANS_NO { get; set; }
        public string VEH_NO { get; set; }
        public string ERRDES { get; set; }
        public DateTime? ERRDATE { get; set; }
        public string RFIDID { get; set; }
        public string ERRTIME { get; set; }
        public decimal WB_NO { get; set; }
    }
}
