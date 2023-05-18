using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class MainAccountInfo
    {
        public string TaxYear { get; set; }
        public string CABORO { get; set; }
        public string CABLOK { get; set; }
        public string CALOT { get; set; }
        public string CATXID { get; set; }
        public string HSENUM { get; set; }
        public string CASNAM { get; set; }
        public string CAZIPC { get; set; }
        public int YEARBUILT { get; set; }
        public string OwnerName { get; set; }
        public int TotalUnits { get; set; }
        public int GBA { get; set; }
        public int NoOfStories { get; set; }
        public int NoOfBuildings { get; set; }
        public string TaxCLS { get; set; }

        public float TRNLND { get; set; }
        public float TRNTTL { get; set; }
        public float ACTLND { get; set; }
        public float ACTTTL { get; set; }
        public float CurrActAssdTotal { get; set; }
    }
}