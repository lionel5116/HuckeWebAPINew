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

using System.Web.Script.Serialization;

namespace HuckeWEBAPI.Controllers
{
    public class CroseController : ApiController

    {

        public string s_ConnectionStringCROSE = ConfigurationManager.AppSettings["CONN_STRING_CROSE"].ToString();
        public string s_DBaseProvider = ConfigurationManager.AppSettings["currProvider"].ToString();
        public string s_Environment = ConfigurationManager.AppSettings["Environment"].ToString();

        public struct VendorArgs
        {
            public string searchType { get; }
            public string VendorNumber { get; }
            public string VendorName { get; }
            public double MinAmount { get; }
            public double MaxAmount { get; }
            public DateTime StartDate { get; }
            public DateTime EndDate { get; }
        }

        [Route("api/crose/getCroseVendorData/{croseArgs}")]
        [HttpGet]
        public List<VendorCROSE> getCroseVendorData(string croseArgs)
        {

            string[] _searchValues = croseArgs.Split('|');


            VendorCROSE oVendorCROSE;
            List<VendorCROSE> lstCROSEData = new List<VendorCROSE>();
            var connectionString = s_ConnectionStringCROSE;

            string SQLCommandText = $"SELECT (VendorNumber + CONVERT(CHAR,[CheckDate]))as random,VendorNumber,Name,CheckNumber, CheckDate,Amount FROM [EDB].[EXT].[VendorCROSE] ";
            //string SQLCommandText = $"SELECT (VendorNumber + CONVERT(CHAR,[CheckDate]))as random,VendorNumber,Name,CheckNumber, CheckDate,Amount FROM VendorData";


            switch (_searchValues[4])
            {
                case "VendorNumber":
                    SQLCommandText += " WHERE VendorNumber LIKE " + "'%" + _searchValues[0] + "%'";
                    break;
                case "VendorName":
                    SQLCommandText += " WHERE Name LIKE " + "'%" + _searchValues[1] + "%'";
                    break;
                case "VendorNameDate":
                    SQLCommandText += " WHERE Name LIKE " + "'%" + _searchValues[1] + "%'" + " AND " + " CheckDate BETWEEN " + "'" + _searchValues[2] + "'" + "AND " + "'" + _searchValues[3] + "'";
                    break;
                case "VendorNumberDate":
                    SQLCommandText += " WHERE VendorNumber LIKE " + "'%" + _searchValues[0] + "%'" + " AND " + " CheckDate BETWEEN " + "'" + _searchValues[2] + "'" + "AND " + "'" + _searchValues[3] + "'";
                    break;
                default:
                    break;
            }

            SQLCommandText += " ORDER BY LTRIM(RTRIM(Name)),CheckDate";

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
                    int i = 0;
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oVendorCROSE = new VendorCROSE();
                        oVendorCROSE.random = i.ToString();
                        oVendorCROSE.VendorNumber = row["VendorNumber"].ToString();
                        oVendorCROSE.Name = row["Name"].ToString();
                        oVendorCROSE.CheckNumber = row["CheckNumber"].ToString();
                        oVendorCROSE.CheckDate = DateTime.Parse(row["CheckDate"].ToString());
                        oVendorCROSE.Amount = double.Parse(row["Amount"].ToString());

                        lstCROSEData.Add(oVendorCROSE);
                        i++;
                        oVendorCROSE = null;
                    }

                }
            }
            

            return lstCROSEData;
        }

        [Route("api/crose/getCroseVendorList/")]
        [HttpGet]
        public List<VendorCROSE> getCroseVendorList()
        {



            VendorCROSE oVendorCROSE;
            List<VendorCROSE> lstCROSEData = new List<VendorCROSE>();
            var connectionString = s_ConnectionStringCROSE;

            string SQLCommandText = $"SELECT distinct(VendorNumber), Name FROM [EDB].[EXT].[VendorCROSE] ";
            SQLCommandText += " ORDER BY Name";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    int i = 0;
                    if (ds.Tables[0].Rows.Count > 0) { } else { return null; };

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oVendorCROSE = new VendorCROSE();
                        oVendorCROSE.random = i.ToString();
                        oVendorCROSE.VendorNumber = row["VendorNumber"].ToString();
                        oVendorCROSE.Name = row["Name"].ToString();
                        lstCROSEData.Add(oVendorCROSE);
                        oVendorCROSE = null;
                        i++;
                    }

                }
            }


            return lstCROSEData;
        }
    }
}
