using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class NotesNextStep
    {
        public int EmployeeID { get; set; }
        public string Notes { get; set; }
        public string NextSteps { get; set; }
        public string SchoolName { get; set; }
    }
}