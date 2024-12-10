using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class EmployeeTable
    {
        public string EmployeeID { get; set; }
        public string SchoolName { get; set; }

        public string EmployeeName { get; set; }

        public string Certification { get; set; }

        public string Role { get; set; }

        public string CrossWalked { get; set; }

        public string Position { get; set; }

        public string Eligibility { get; set; }

        public int PositionID { get; set; }

        public string PositionName { get; set; }
        public string Status { get; set; }

    }
}