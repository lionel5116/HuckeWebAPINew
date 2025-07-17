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


        #region EmployeeData
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
                        oEmployeeTable.EmployeeID = int.Parse(row["Employee"].ToString());
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


        [HttpPost]
        [Route("api/leveling/AddOrUpdateEmployeeRecord")]
        public bool AddOrUpdateEmployeeRecord([FromBody] EmployeeTableLeveling oEmployeeTableLeveling)
        {
            bool bSuccess = false;

            var connectionString = s_ConnectionString_leveling;


            const string sql2 = @"
                                IF NOT EXISTS(SELECT 1 FROM Employee WHERE EmployeeID = @EmployeeID)
                                    BEGIN
                                        INSERT INTO Employee(
                                           EmployeeID,
                                            EmployeeName,
                                            Division ,
                                            CampusName,
                                            CampusType,
                                            CurrentPosition,
                                            CurrentPositionName,
                                            UpdatedBy
                                                )
                                        VALUES(
                                            @EmployeeID,
                                            @EmployeeName,
                                            @Division ,
                                            @CampusName,
                                            @CampusType,
                                            @CurrentPosition,
                                            @CurrentPositionName,
                                            @UpdatedBy)
                                    END
                                ELSE
                                    BEGIN
                                       UPDATE Employee SET 
                                            EmployeeID = @EmployeeID,
                                            EmployeeName = @EmployeeName,
                                            Division =@Division ,
                                            CampusName =@CampusName,
                                            CampusType =@CampusType,
                                            CurrentPosition =@CurrentPosition,
                                            CurrentPositionName = @CurrentPositionName,
                                            UpdatedBy = @UpdatedBy 
                                            WHERE EmployeeID = @EmployeeID
                                    END";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@EmployeeID", oEmployeeTableLeveling.EmployeeID);
                    cmd.Parameters.AddWithValue("@EmployeeName", oEmployeeTableLeveling.EmployeeName);
                    cmd.Parameters.AddWithValue("@Division", oEmployeeTableLeveling.Division);
                    cmd.Parameters.AddWithValue("@CampusName", oEmployeeTableLeveling.CampusName);
                    cmd.Parameters.AddWithValue("@CampusType", oEmployeeTableLeveling.CampusType);
                    cmd.Parameters.AddWithValue("@CurrentPosition", oEmployeeTableLeveling.CurrentPosition);
                    cmd.Parameters.AddWithValue("@CurrentPositionName", oEmployeeTableLeveling.CurrentPositionName);
                    cmd.Parameters.AddWithValue("@UpdatedBy", oEmployeeTableLeveling.UpdatedBy);



                    da.SelectCommand = cmd;

                    CONN.Open();
                    int nRecsAffected = cmd.ExecuteNonQuery();
                    if (nRecsAffected > 0)
                    {
                        bSuccess = true;
                    }
                    else
                    {
                        bSuccess = false;
                    }
                    CONN.Close();


                }
            }

            return bSuccess;
        }

        [Route("api/leveling/fetchLevelingData/{Division}")]
        [HttpGet]
        public List<Leveling> fetchLevelingData(string Division)
        {
            Leveling oLevelingTable;
            List<Leveling> lstLevelingTableData = new List<Leveling>();

            string SQLCommandText = @"Select a.CampusName,
                                     a.EmployeeID,
                                     a.EmployeeName,
		                             PositionDescription =  CONCAT(a.CurrentPosition, ' - ' + a.CurrentPositionName),
		                             a.Division,
		                             b.Status,
		                             b.ActionType,
                                     c.CERTIFICATIONS,
		                             ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS fake_id
		                             from Employee a
		                             left join
	                                 Leveling b
		                             on a.EmployeeID = b.EmployeeID
                                     left join [dbo].[EMPLOYEE_CERT_TABLE] c
                                     on a.EmployeeID = c.Employee
		                             WHERE
		                             a.Division = @Division";


            var connectionString = s_ConnectionString_leveling;

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@Division", Division);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oLevelingTable = new Leveling();
                        oLevelingTable.CampusName = row["CampusName"].ToString();
                        oLevelingTable.EmployeeID = int.Parse(row["EmployeeID"].ToString());
                        oLevelingTable.EmployeeName = row["EmployeeName"].ToString();
                        oLevelingTable.Division = row["Division"].ToString();
                        oLevelingTable.PositionDescription = row["PositionDescription"].ToString();
                        oLevelingTable.Status = row["Status"].ToString();
                        oLevelingTable.ActionType = row["ActionType"].ToString();
                        oLevelingTable.CERTIFICATIONS = row["CERTIFICATIONS"].ToString();
                        oLevelingTable.fake_id = row["fake_id"].ToString();  //CERTIFICATIONS

                        lstLevelingTableData.Add(oLevelingTable);
                        oLevelingTable = null;
                    }
                }
            }

            return lstLevelingTableData;
        }

        #endregion EmployeeData

        #region certData
        [Route("api/leveling/fetchCERTData/{employeeID}")]
        [HttpGet]
        public List<CertificationData> fetchCERTData(string employeeID)
        {
            CertificationData oCERTData;
            List<CertificationData> lstCERTData = new List<CertificationData>();

          
            string SQLCommandText = "";
           
            SQLCommandText = @"Select [Employee], EmployeeName,CERTIFICATIONS As Certification from EMPLOYEE_CERT_TABLE
                             WHERE [Employee] = @employeeID";
            
            var connectionString = s_ConnectionString_leveling;


            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@employeeID", employeeID);
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oCERTData = new CertificationData();
                        oCERTData.Employee = row["Employee"].ToString();
                        oCERTData.EmployeeName = row["EmployeeName"].ToString();
                        oCERTData.Certification = row["Certification"].ToString();

                        lstCERTData.Add(oCERTData);
                        oCERTData = null;
                    }
                }
            }

            return lstCERTData;
        }
        #endregion certData



    }
}
