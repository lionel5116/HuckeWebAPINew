using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class EmployeeTableLeveling
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Division { get; set; }

        public string CampusName { get; set; }

        public string CampusType { get; set; }

        public int CurrentPosition { get; set; }

        public string CurrentPositionName { get; set; }
        public string fake_id { get; set; }
    }
}