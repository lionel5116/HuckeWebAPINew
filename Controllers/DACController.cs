using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HuckeWEBAPI.Models;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace HuckeWEBAPI.Controllers
{
    public class DACController : ApiController
    {

        public string s_ConnectionStringDAC = ConfigurationManager.AppSettings["CONN_STRING_DAC"].ToString();
        public string s_DBaseProvider = ConfigurationManager.AppSettings["currProvider"].ToString();
        public string s_Environment = ConfigurationManager.AppSettings["Environment"].ToString();


        [Route("api/VoteSetting/FetchAllVoteSettings/")]
        [HttpGet]
        public List<VoteSettings> FetchAllVoteSettings()
        {
            VoteSettings oVoteSettings;
            List<VoteSettings> lstVoteSettingsData = new List<VoteSettings>();
            var connectionString = s_ConnectionStringDAC;

            string SQLCommandText = $"SELECT * from VotingSetting";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    if (ds.Tables[0].Rows.Count > 0) { } else { return null; };

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oVoteSettings = new VoteSettings();
                        oVoteSettings.VotingSettingID = int.Parse(row["VotingSettingID"].ToString());
                        oVoteSettings.SchoolYear = row["SchoolYear"].ToString();
                        oVoteSettings.VotingStartDate = row["VotingStartDate"].ToString();
                        oVoteSettings.VotingEndDate = row["VotingEndDate"].ToString();
                        oVoteSettings.IsActive = bool.Parse(row["IsActive"].ToString());

                        lstVoteSettingsData.Add(oVoteSettings);
                        oVoteSettings = null;
                    }

                }
            }

            return lstVoteSettingsData;
        }
    }
}
