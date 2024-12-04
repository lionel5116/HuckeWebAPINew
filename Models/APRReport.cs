using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class APRReport
    {
        public string NES { get; set; }
        public string Employee { get; set; }
        public string Employee_Name { get; set; }
        public string Division { get; set; }

        public string Unit { get; set; }
        public string Org_Unit_Name { get; set; }
        public string Position { get; set; }
        public string Position_Name { get; set; }
        public string Job { get; set; }
        public string Job_Name { get; set; }
        public string Status { get; set; }
    }
}