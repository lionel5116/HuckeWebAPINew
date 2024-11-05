using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class CrosswalkData
    {
        public string EmployeeID { get; set; }
        public string Position { get; set; }
        public string SchoolName { get; set; }
        public int CRecordID { get; set; }

        public DateTime DateAdded { get; set; }
    }
}