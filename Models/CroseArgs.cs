using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class CroseArgs
    {
        public string searchType { get; }
        public string VendorNumber { get; }
        public string VendorName { get; }
        public double MinAmount { get; }
        public double MaxAmount { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
    }
}