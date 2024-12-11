using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class APRReport
    {
        public string Employee { get; set; }
        public string Employee_Name { get; set; }
        public string Position { get; set; }
        public string Position_Name { get; set; }
        public string CSS { get; set; }
        public string Intent { get; set; }
        public string Eligibility { get; set; }
        public string PositionID { get; set; }
        public string CPosition { get; set; }

        public string SchoolName { get; set; }
        public string CrossWalked { get; set; }
        public string Certification { get; set; }

    }
}