using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class Capping
    {
        public string Campus { get; set; }
        public string Grade { get; set; }
        public string Program { get; set; }

        public int Enrollment { get; set; }

        public int RatioDivisor { get; set; }

        public int TeacherAPR { get; set; }

        public int StaffRatio { get; set; }

        public string StaffPlusMinus { get; set; }
        public string CapStatus { get; set; }
        public string Guideline { get; set; }
        public string Waiver { get; set; }

        public int fake_id { get; set; }


    }
}