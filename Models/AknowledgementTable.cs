using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class AknowledgementTable
    {
        public string Employee { get; set; }
        public string Org_Unit_Name { get; set; }
        public DateTime CompletionDate { get; set; }
    }
}