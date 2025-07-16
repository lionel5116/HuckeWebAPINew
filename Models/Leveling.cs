using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class Leveling
    {
        public string CampusName { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
       public string PositionDescription { get; set; }
        public int NewPosition { get; set; }

        public string Status { get; set; }

        public string Division { get; set; }

        public string ActionType { get; set; }

        public string fake_id { get; set; }
    }
}