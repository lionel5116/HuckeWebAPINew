using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class ChartDataNESEligibility
    {
        public int EligibleNoIdentified { get; set; }
        public int Principle { get; set; }
        public int Eligible { get; set; }
        public int NonEligible { get; set; }
    }
}