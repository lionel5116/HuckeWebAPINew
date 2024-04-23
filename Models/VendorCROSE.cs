using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class VendorCROSE
    {
        public string VendorNumber { get; set; }
        public string Name { get; set; }
        public string random { get; set; }
        public string CheckNumber { get; set; }
        public DateTime CheckDate { get; set; }
        public double Amount { get; set; }
       
    }
}