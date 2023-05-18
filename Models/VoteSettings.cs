using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HuckeWEBAPI.Models
{
    public class VoteSettings
    {
        public int VotingSettingID { get; set; }
        public string SchoolYear { get; set; }
        public string VotingStartDate { get; set; }
        public string VotingEndDate { get; set; }
        public bool IsActive { get; set; }
    }
}