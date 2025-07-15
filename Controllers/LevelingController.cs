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
    public class LevelingController : ApiController
    {
        public string s_ConnectionString_leveling = ConfigurationManager.AppSettings["ConnectionString_Leveling"].ToString();

        [Route("api/leveling/fetchEmployeeData/{EmployeeID}")]
        [HttpGet]
        public List<EmployeeTableLeveling> fetchEmployeeData(int EmployeeID)
        {
            EmployeeTableLeveling oEmployeeTable;
            List<EmployeeTableLeveling> lstEmployeeTableData = new List<EmployeeTableLeveling>();
  
           string SQLCommandText = @"SELECT a.Employee_Name,
                                       a.Employee ,
	                                   a.Division ,
	                                   a.Org_Unit_Name,
                                       a.NES,
                                       a.Position,
                                       a.Position_Name,
                                       ROW_NUMBER() OVER(ORDER BY(SELECT NULL)) AS fake_id
                                       FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                        WHERE a.Employee = @EmployeeID";


            var connectionString = s_ConnectionString_leveling;

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@EmployeeID", EmployeeID);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oEmployeeTable = new EmployeeTableLeveling();
                        oEmployeeTable.EmployeeID= int.Parse(row["Employee"].ToString());
                        oEmployeeTable.EmployeeName = row["Employee_Name"].ToString();
                        oEmployeeTable.Division = row["Division"].ToString();
                        oEmployeeTable.CampusName = row["Org_Unit_Name"].ToString();
                        oEmployeeTable.CampusType = row["NES"].ToString();
                        oEmployeeTable.CurrentPosition = int.Parse(row["Position"].ToString());
                        oEmployeeTable.CurrentPositionName = row["Position_Name"].ToString();
                        oEmployeeTable.fake_id = row["fake_id"].ToString();

                        lstEmployeeTableData.Add(oEmployeeTable);
                        oEmployeeTable = null;
                    }
                }
            }

            return lstEmployeeTableData;
        }
    }
}
