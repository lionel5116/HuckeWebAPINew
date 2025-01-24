using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class ChartDataCompletionStatus
    {
        public int PRINCIPLEPLACED { get; set; }
        public int NONPLACED { get; set; }
        public int ELIGIBLEPLACED { get; set; }
        public int NONELIGIBLEPLACED { get; set; }
    }
}