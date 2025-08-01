﻿using System;
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
        // public string s_ConnectionString_leveling = ConfigurationManager.AppSettings["ConnectionString_Leveling"].ToString();

        //MOVED TO HISDCustomDevAppsWebAPI  ****************************
        public string s_ConnectionString_leveling = null;


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

        #region PositionManagement
        [Route("api/leveling/fetchAllPositionsBySchoolNameLeveling/{SchoolName}")]
        [HttpGet]
        public List<Positions> fetchAllPositionsBySchoolNameLeveling(string SchoolName)
        {
            Positions oPositions;
            List<Positions> lstPositionData = new List<Positions>();

            var connectionString = s_ConnectionString_leveling;
            string SQLCommandText = "";



            /*FIX BELOW TO ACCOUNT FOR KEYDATE 
            SQLCommandText = @"SELECT distinct a.Position as PositionNumber,a.[Position_Name] as Position, CONVERT(varchar(20),a.Position) + ' - ' + a.[Position_Name] AS CMBPos,
                              
							  FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							   LEFT JOIN CrossWalk b on a.Position = b.PositionID
                              WHERE LEN([Org_Unit_Name]) > 2 AND NES = 'NES'
                              AND
                              [Org_Unit_Name] =  @SchoolName
                                  
                              AND a.KeyDate = CAST(CAST(YEAR(GETDATE()) AS VARCHAR) + '-09-01' AS DATE) --added 03/25/2025 for obsolete
                              order by
                              [Position]";

            */

            SQLCommandText = @"SELECT distinct
            a.Position as PositionNumber,
            a.[Position_Name] as Position,
            CONVERT(varchar(20), a.Position) + ' - ' + a.[Position_Name] AS CMBPos
            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
             WHERE LEN([Org_Unit_Name]) > 2 AND NES = 'NES'
             AND
             [Org_Unit_Name] =  @SchoolName
             AND a.KeyDate = CAST(CAST(YEAR(GETDATE()) AS VARCHAR) + '-09-01' AS DATE)--added 03 / 25 / 2025 for obsolete
             order by
             [Position]";
   

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@SchoolName", SchoolName);
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oPositions = new Positions();
                        oPositions.Position = row["Position"].ToString();
                        oPositions.PositionNumber = row["PositionNumber"].ToString();
                        oPositions.CMBPos = row["CMBPos"].ToString();
                        lstPositionData.Add(oPositions);
                        oPositions = null;
                    }
                }
            }

            return lstPositionData;
        }
        #endregion  PositionManagement

        #region Capping
        [Route("api/leveling/fetchCappingData/")]
        [HttpGet]
        public List<Capping> fetchCappingData()
        {
           
            List<Capping> lstCappingData = new List<Capping>();

            var connectionString = s_ConnectionString_leveling;
            string SQLCommandText = "";



            SQLCommandText = @"SELECT Campus,Grade,Program,Enrollment,RatioDivisor,
                               TeacherAPR,StaffRatio,StaffPlusMinus,CapStatus,
                               Guideline,fake_id,Waiver
                               FROM Capped";


            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        try
                        {
                            Capping oCappings = new Capping();
                            oCappings.Campus = row["Campus"].ToString();
                            oCappings.Grade = row["Grade"].ToString();
                            oCappings.Program = row["Program"].ToString();

                            if(row.IsNull("Enrollment"))
                            {
                                oCappings.Enrollment = 0;
                            }
                            else
                            {
                                oCappings.Enrollment = int.Parse(row["Enrollment"].ToString());
                            }
                           

                            oCappings.RatioDivisor = int.Parse(row["RatioDivisor"].ToString());
                            oCappings.TeacherAPR = int.Parse(row["TeacherAPR"].ToString());
                            oCappings.StaffRatio = int.Parse(row["StaffRatio"].ToString());
                            oCappings.StaffPlusMinus = row["StaffPlusMinus"].ToString();
                            oCappings.CapStatus = row["CapStatus"].ToString();
                            oCappings.Guideline = row["Guideline"].ToString();
                            oCappings.Waiver = row["Waiver"].ToString();
                            oCappings.fake_id = int.Parse(row["fake_id"].ToString());

                            lstCappingData.Add(oCappings);
                            // Setting oCappings to null here is not necessary as it will be out of scope 
                            // and a new one is created in the next iteration.
                            // oCappings = null; 
                        }
                        catch (Exception ex)
                        {
                            // You can handle the exception here. For example, log the error
                            // or skip the problematic row and continue with the next one.
                            // For debugging purposes, you might want to print the error:
                            Console.WriteLine("An error occurred while processing a row: " + ex.Message);
                            // Depending on your requirements, you might want to 'continue;' 
                            // to the next iteration or 'break;' the loop.
                        }
                    }
                }
            }

            return lstCappingData;
        }

        [HttpPost]
        [Route("api/leveling/UpdateCappingRecord")]
        public bool UpdateCappingRecord([FromBody] Capping oCappingData)
        {
            bool bSuccess = false;


            var connectionString = s_ConnectionString_leveling;
           

            const string sql2 = @"UPDATE Capped SET 
                                       Enrollment = @Enrollment 
                                       WHERE Campus = @Campus AND Program = @Program";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@Campus", oCappingData.Campus);
                    cmd.Parameters.AddWithValue("@Program", oCappingData.Program);
                    cmd.Parameters.AddWithValue("@Enrollment", oCappingData.Enrollment);
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


        [HttpGet]
        [Route("api/leveling/UpdateCappingRecordByID/{args}")]
        public bool UpdateCappingRecordByID(string args)
        {
            // 1. Validate the input first
            if (string.IsNullOrWhiteSpace(args))
            {
                return false;
            }

            string[] myArgs = args.Split('|');
            if (myArgs.Length != 2)
            {
                // Invalid arguments format
                return false;
            }

            // Use TryParse for safe conversion from string to int
            if (!int.TryParse(myArgs[0], out int enrollment) || !int.TryParse(myArgs[1], out int fakeId))
            {
                // One of the arguments was not a valid number
                return false;
            }

            try
            {
                string connectionString = s_ConnectionString_leveling;

                // 2. Use a parameterized query to prevent SQL injection
                string sqlQuery = "UPDATE Capped SET Enrollment = @Enrollment WHERE fake_id = @FakeId;";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                    {
                        // 3. Add parameters safely
                        cmd.Parameters.AddWithValue("@Enrollment", enrollment);
                        cmd.Parameters.AddWithValue("@FakeId", fakeId);

                        conn.Open();

                        // 4. Use ExecuteNonQuery() for UPDATE, INSERT, DELETE commands
                        // It returns the number of rows affected.
                        int rowsAffected = cmd.ExecuteNonQuery();

                        // 5. Return true if exactly one row was updated
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception in a real application (e.g., using Serilog, NLog)
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        #endregion Capping

    }
}
