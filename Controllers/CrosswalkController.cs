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
    public class CrosswalkController : ApiController
    {
        public string s_ConnectionString_CrossWalk = ConfigurationManager.AppSettings["ConnectionString_CrossWalk_PROD"].ToString();
        public string s_ConnectionString_CrossWalk_Local = ConfigurationManager.AppSettings["ConnectionString_CrossWalk_Local"].ToString();
        public string _connStringDataWarehouse_EDB = ConfigurationManager.AppSettings["_DataWarehouseEDB"].ToString();
        public string s_Environment = ConfigurationManager.AppSettings["Environment"].ToString();

      

       

        public static DataSet GetDataSetCommon(string sql, string s_Provider, string s_ConnString)

        {
            try

            {
                DbProviderFactory factory = DbProviderFactories.GetFactory(s_Provider);
                using (IDbConnection cn = factory.CreateConnection())
                {
                    cn.ConnectionString = s_ConnString;
                    cn.Open();
                    IDbDataAdapter da = factory.CreateDataAdapter();
                    da.SelectCommand = cn.CreateCommand();
                    da.SelectCommand.CommandText = sql;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        cn.Close();
                        return ds;
                    }
                    else
                    {
                        cn.Close();
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error in retrieving data " + e.Message);
                return null;
            }

        }


        #region PositionManagement
        
      
        [Route("api/Crosswalk/fetchAllPositionsBySchoolName/{SchoolName}")]
        [HttpGet]
        public List<Positions> fetchAllPositionsBySchoolName(string SchoolName)
        {
            Positions oPositions;
            List<Positions> lstPositionData = new List<Positions>();

            var connectionString = "";
            string SQLCommandText = "";


            /*
            SQLCommandText = @"SELECT distinct a.Position as PositionNumber,a.[Position_Name] as Position, CONVERT(varchar(20),a.Position) + ' - ' + a.[Position_Name] AS CMBPos,
                              CrossWalked = CASE
		                      WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
							  FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							   LEFT JOIN CrossWalk b on a.Position = b.PositionID
                              WHERE LEN([Org_Unit_Name]) > 2 AND NES = 'NES'
                              AND
                              [Org_Unit_Name] =  @SchoolName
                              --  AND LEN(a.Employee) > 1
                              order by
                              [Position]";
            */
            
            /*FIX BELOW TO ACCOUNT FOR KEYDATE */
            SQLCommandText = @"SELECT distinct a.Position as PositionNumber,a.[Position_Name] as Position, CONVERT(varchar(20),a.Position) + ' - ' + a.[Position_Name] AS CMBPos,
                              CrossWalked = CASE
		                      WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
							  FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							   LEFT JOIN CrossWalk b on a.Position = b.PositionID
                              WHERE LEN([Org_Unit_Name]) > 2 AND NES = 'NES'
                              AND
                              [Org_Unit_Name] =  @SchoolName
                              AND a.KeyDate >= CAST(YEAR(GETDATE()) AS VARCHAR) + '-09-01'
                              order by
                              [Position]";


            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
      
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
               
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
             
                    break;
            }


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
                        oPositions.CrossWalked = row["CrossWalked"].ToString();
                        lstPositionData.Add(oPositions);
                        oPositions = null;
                    }
                }
            }

            return lstPositionData;
        }

     
        [Route("api/Crosswalk/fetchUnassignedPositions/{SchoolName}")]
        [HttpGet]
        public List<Positions> fetchUnassignedPositions(string SchoolName)
        {
            Positions oPositions;
            List<Positions> lstPositionData = new List<Positions>();

            var connectionString = "";
            string SQLCommandText = "";



            /*
            SQLCommandText = @"SELECT distinct a.Position as PositionNumber,a.[Position_Name] as Position, CONVERT(varchar(20),a.Position) + ' - ' + a.[Position_Name] AS CMBPos,
                              CrossWalked = CASE
		                      WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
							  FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							  LEFT JOIN CrossWalk b on a.[Position_Name] = b.Position
                              WHERE LEN([Org_Unit_Name]) > 2 AND NES = 'NES'
                              AND
                              [Org_Unit_Name] =  @SchoolName
							  AND
							  a.Position NOT IN (SELECT b.PositionID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                              --  AND LEN(a.Employee) > 1
                              order by
                              [Position]";
            */

            /*FIX FOR KEYDATE */
            SQLCommandText = @"SELECT distinct a.Position as PositionNumber,a.[Position_Name] as Position, CONVERT(varchar(20),a.Position) + ' - ' + a.[Position_Name] AS CMBPos,
                              CrossWalked = CASE
		                      WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
							  FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							  LEFT JOIN CrossWalk b on a.[Position_Name] = b.Position
                              WHERE LEN([Org_Unit_Name]) > 2 AND NES = 'NES'
                              AND
                              [Org_Unit_Name] =  @SchoolName
							  AND
							  a.Position NOT IN (SELECT b.PositionID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                              AND a.KeyDate >= CAST(YEAR(GETDATE()) AS VARCHAR) + '-09-01'
                              order by
                              [Position]";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }


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
                        oPositions.CrossWalked = row["CrossWalked"].ToString();
                        
                        lstPositionData.Add(oPositions);
                        oPositions = null;
                    }
                }
            }

            return lstPositionData;
        }

        
        [Route("api/Crosswalk/fetcAssignedPositions/{SchoolName}")]
        [HttpGet]
        public List<Positions> fetcAssignedPositions(string SchoolName)
        {
            Positions oPositions;
            List<Positions> lstPositionData = new List<Positions>();

            var connectionString = "";
            string SQLCommandText = "";


           
            SQLCommandText = @"SELECT PositionID as PositionNumber,Position,SchoolName,
                               CONVERT(varchar(20),PositionID) + ' - ' + [Position] AS CMBPos,
	                           'YES' as CrossWalked 
	                           FROM CrossWalk
	                           WHERE SchoolName  =  @SchoolName";

            
            
            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }


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
                        oPositions.CrossWalked = row["CrossWalked"].ToString();

                        lstPositionData.Add(oPositions);
                        oPositions = null;
                    }
                }
            }

            return lstPositionData;
        }
        #endregion PositionManagement


        #region FETCH_EMPLOYEE_DATA
        [Route("api/Crosswalk/fetchAllEmployeeDataBySchoolName/{SchoolName}")]
        [HttpGet]
        public List<EmployeeTable> fetchAllEmployeeDataBySchoolName(string SchoolName)
        {
            EmployeeTable oEmployeeTable;
            List<EmployeeTable> lstEmployeeTableData = new List<EmployeeTable>();

            var connectionString = "";
            string SQLCommandText = "";
            var SQLCommandTextSCOLLDIF = "";



            /*
            SQLCommandTextSCOLLDIF = @"SELECT a.Employee as EmployeeID,
                                   a.Org_Unit_Name as SchoolName,
	                               a.Employee_Name as EmployeeName,
	                                  [Role] = CASE
								   WHEN b.[SchoolName] <> a.Org_Unit_Name THEN CONCAT(a.Position, ' - ' + a.Position_Name + ' <-> ' + b.[SchoolName])
								   ELSE
								    CONCAT(a.Position, ' - ' + a.Position_Name)
								   END,
                                    CONCAT(a.Employee, b.PositionID) CWKey,
                                    ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS fake_id,
                                   CRossWalkDiff = CASE
                                    WHEN a.Org_Unit_Name != b.SchoolName THEN 'YES' ELSE 'NO' END,
                                    b.SchoolName as CWSchool,
                                   b.PositionID,
                                   b.Position as PositionName,
	                               d.Eligibility,
								   c.[Qualification Text] As Certification,
                                   '' As Certification,
                                   b.CRecordID,b.Position,
	                                CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    LEFT JOIN EligibilityTable d on a.Employee = d.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
                                    WHERE a.Org_Unit_Name =  @SchoolName AND LEN(a.Employee) > 1
                                    OR
                                    a.Employee IN(SELECT x.EmployeeID FROM CrossWalk x WHERE x.SchoolName =  @SchoolName AND a.Org_Unit_Name != x.SchoolName)
                                    ORDER BY
                                    b.Position ASC";
              
            */
            SQLCommandTextSCOLLDIF = @"SELECT a.Employee as EmployeeID,
                                   a.Org_Unit_Name as SchoolName,
	                               a.Employee_Name as EmployeeName,
	                               CONCAT(a.Position, ' - ' + a.Position_Name) AS Role,

                                   PositionName = CASE

                                   WHEN b.[SchoolName] <> a.Org_Unit_Name THEN CONCAT(b.Position,  ' <-> ' + b.[SchoolName])
								   ELSE

                                   b.Position
                                   END,


                                    CONCAT(a.Employee, b.PositionID) CWKey,
                                    ROW_NUMBER() OVER(ORDER BY(SELECT NULL)) AS fake_id,
                                 CRossWalkDiff = CASE
                                    WHEN a.Org_Unit_Name != b.SchoolName THEN 'YES' ELSE 'NO' END,
                                    b.SchoolName as CWSchool,
                                   b.PositionID,
                                   --b.Position as PositionName,
	                               d.Eligibility,
								   c.[Qualification Text] As Certification,
                                   '' As Certification,
                                   b.CRecordID,b.Position,
	                                CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    LEFT JOIN EligibilityTable d on a.Employee = d.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
                                    WHERE a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1
                                    OR
                                    a.Employee IN(SELECT x.EmployeeID FROM CrossWalk x WHERE x.SchoolName = @SchoolName AND a.Org_Unit_Name != x.SchoolName)
                                    ORDER BY
                                    b.Position ASC";



            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
            }


            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                /*WAS SQLCommandTextNew*/
                using (SqlCommand cmd = new SqlCommand(SQLCommandTextSCOLLDIF, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@SchoolName", SchoolName);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oEmployeeTable = new EmployeeTable();
                        oEmployeeTable.EmployeeID = row["EmployeeID"].ToString();
                        oEmployeeTable.SchoolName = row["SchoolName"].ToString();

                        oEmployeeTable.EmployeeName = row["EmployeeName"].ToString();
                        oEmployeeTable.Role = row["Role"].ToString();
                        oEmployeeTable.Certification = row["Certification"].ToString();
                        oEmployeeTable.Position = row["Position"].ToString();
                        oEmployeeTable.PositionName = row["PositionName"].ToString();
                        oEmployeeTable.CrossWalked = row["CrossWalked"].ToString();
                        oEmployeeTable.Eligibility = row["Eligibility"].ToString();
                        oEmployeeTable.CWSchool = row["CWSchool"].ToString();
                        oEmployeeTable.CWKey = row["CWKey"].ToString();
                        oEmployeeTable.CRossWalkDiff = row["CRossWalkDiff"].ToString();
                        oEmployeeTable.fake_id = row["fake_id"].ToString();


                        if (row["PositionID"].ToString().Length > 2)
                        {
                            oEmployeeTable.PositionID = int.Parse(row["PositionID"].ToString());
                        }
                        else
                        {
                            oEmployeeTable.PositionID = 0;
                        }

                        lstEmployeeTableData.Add(oEmployeeTable);
                        oEmployeeTable = null;
                    }
                }
            }

            return lstEmployeeTableData;
        }

        [Route("api/Crosswalk/fetchAllEmployeeDataBySchoolNameSCHOOLDIFF/{SchoolName}")]
        [HttpGet]
        public List<EmployeeTable> fetchAllEmployeeDataBySchoolNameSCHOOLDIFF(string SchoolName)
        {
            EmployeeTable oEmployeeTable;
            List<EmployeeTable> lstEmployeeTableData = new List<EmployeeTable>();

            var connectionString = "";
            string SQLCommandText = "";
            var SQLCommandTextSCOLLDIF = "";


            /*
            var SQLCommandTextNewx = @"SELECT a.Employee as EmployeeID,
                                   a.Org_Unit_Name as SchoolName,
	                               a.Employee_Name as EmployeeName,
	                               [Role] = CASE
								   WHEN b.[SchoolName] <> a.Org_Unit_Name THEN CONCAT(a.Position, ' - ' + a.Position_Name + ' <-> ' + b.[SchoolName])
								   ELSE
								    CONCAT(a.Position, ' - ' + a.Position_Name)
								   END,
                                 CONCAT(a.Employee,b.PositionID) CWKey,
                                   CRossWalkDiff = CASE
                                    WHEN a.Org_Unit_Name != b.SchoolName THEN 'YES' ELSE 'NO' END,
                                    b.SchoolName as CWSchool,
                                   b.PositionID,
                                   b.Position as PositionName,
	                               d.Eligibility,
								   c.[Qualification Text] As Certification,
                                   '' As Certification,
                                   b.CRecordID,b.Position,
	                                CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
									LEFT JOIN EligibilityTable d on a.Employee = d.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
                                    WHERE a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1
                                    ORDER BY 
                                    b.Position ASC
									";
            */

            SQLCommandTextSCOLLDIF = @"SELECT a.Employee as EmployeeID,
                                   a.Org_Unit_Name as SchoolName,
	                               a.Employee_Name as EmployeeName,
	                                 [Role] = CASE
								    WHEN b.[SchoolName] <> a.Org_Unit_Name THEN CONCAT(a.Position, ' - ' + a.Position_Name + ' <-> ' + b.[SchoolName])
								   ELSE
								    CONCAT(a.Position, ' - ' + a.Position_Name)
								   END,
                                   CONCAT(a.Employee, b.PositionID) CWKey,
                                   CRossWalkDiff = CASE
                                   WHEN a.Org_Unit_Name != b.SchoolName THEN 'YES' ELSE 'NO' END,
                                   b.SchoolName as CWSchool,
                                   b.PositionID,
                                   b.Position as PositionName,
	                               d.Eligibility,
								   c.[Qualification Text] As Certification,
                                   '' As Certification,
                                   b.CRecordID,b.Position,
	                                CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID

                                    LEFT JOIN EligibilityTable d on a.Employee = d.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
                                    WHERE a.Org_Unit_Name =  @SchoolName AND LEN(a.Employee) > 1
                                    OR
                                    a.Employee IN(SELECT x.EmployeeID FROM CrossWalk x WHERE x.SchoolName =  @SchoolName AND a.Org_Unit_Name != x.SchoolName)
                                    ORDER BY
                                    b.Position ASC";


            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
            }


            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandTextSCOLLDIF, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@SchoolName", SchoolName);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oEmployeeTable = new EmployeeTable();
                        oEmployeeTable.EmployeeID = row["EmployeeID"].ToString();
                        oEmployeeTable.SchoolName = row["SchoolName"].ToString();

                        oEmployeeTable.EmployeeName = row["EmployeeName"].ToString();
                        oEmployeeTable.Role = row["Role"].ToString();
                        oEmployeeTable.Certification = row["Certification"].ToString();
                        oEmployeeTable.Position = row["Position"].ToString();
                        oEmployeeTable.PositionName = row["PositionName"].ToString();
                        oEmployeeTable.CrossWalked = row["CrossWalked"].ToString();
                        oEmployeeTable.Eligibility = row["Eligibility"].ToString();
                        oEmployeeTable.CWSchool = row["CWSchool"].ToString();
                        oEmployeeTable.CWKey = row["CWKey"].ToString();
                        oEmployeeTable.CRossWalkDiff = row["CRossWalkDiff"].ToString();


                        if (row["PositionID"].ToString().Length > 2)
                        {
                            oEmployeeTable.PositionID = int.Parse(row["PositionID"].ToString());
                        }
                        else
                        {
                            oEmployeeTable.PositionID = 0;
                        }

                        lstEmployeeTableData.Add(oEmployeeTable);
                        oEmployeeTable = null;
                    }
                }
            }

            return lstEmployeeTableData;
        }

        [Route("api/Crosswalk/fetchAllEmployeeDataBySchoolNameNotCrosswalked/{SchoolName}")]
        [HttpGet]
        public List<EmployeeTable> fetchAllEmployeeDataBySchoolNameNotCrosswalked(string SchoolName)
        {
            EmployeeTable oEmployeeTable;
            List<EmployeeTable> lstEmployeeTableData = new List<EmployeeTable>();

            var connectionString = "";
            string SQLCommandText = "";



            var SQLCommandTextNew = @"SELECT a.Employee as EmployeeID,
                                   a.Org_Unit_Name as SchoolName,
	                               a.Employee_Name as EmployeeName,
	                                [Role] = CASE
								   WHEN b.[SchoolName] <> a.Org_Unit_Name THEN CONCAT(a.Position, ' - ' + a.Position_Name + ' - ' + b.[SchoolName])
								   ELSE
								    CONCAT(a.Position, ' - ' + a.Position_Name)
								   END,
                                   b.PositionID,
                                    a.Status,
                                   b.Position as PositionName,
	                              f.Eligibility,
								   c.[Qualification Text] As Certification,
                                   '' As Certification,
                                   b.CRecordID,b.Position,
	                                CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END,
                                    d.Notes,
                                    e.NextStep
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    LEFT JOIN EligibilityTable f on a.Employee = f.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
                                    LEFT JOIN EmployeeNotesNotCrosswalked d on a.Employee = d.EmployeeID
                                    LEFT JOIN EmployeeNextSteps e on a.Employee = e.EmployeeID
                                    WHERE 
                                    a.Employee NOT IN (SELECT b.EmployeeID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                                    AND
                                    a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1 ORDER BY b.Position ";


            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
            }


            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandTextNew, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@SchoolName", SchoolName);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oEmployeeTable = new EmployeeTable();
                        oEmployeeTable.EmployeeID = row["EmployeeID"].ToString();
                        oEmployeeTable.SchoolName = row["SchoolName"].ToString();

                        oEmployeeTable.EmployeeName = row["EmployeeName"].ToString();
                        oEmployeeTable.Role = row["Role"].ToString();
                        oEmployeeTable.Certification = row["Certification"].ToString();
                        oEmployeeTable.Position = row["Position"].ToString();
                        oEmployeeTable.PositionName = row["PositionName"].ToString();
                        oEmployeeTable.CrossWalked = row["CrossWalked"].ToString();
                        oEmployeeTable.Eligibility = row["Eligibility"].ToString();
                        oEmployeeTable.Status = row["Status"].ToString();
                        oEmployeeTable.SchoolName = row["SchoolName"].ToString();
                        oEmployeeTable.Notes = row["Notes"].ToString();
                        oEmployeeTable.NextStep = row["NextStep"].ToString();

                        if (row["PositionID"].ToString().Length > 2)
                        {
                            oEmployeeTable.PositionID = int.Parse(row["PositionID"].ToString());
                        }
                        else
                        {
                            oEmployeeTable.PositionID = 0;
                        }

                        lstEmployeeTableData.Add(oEmployeeTable);
                        oEmployeeTable = null;
                    }
                }
            }

            return lstEmployeeTableData;
        }

        [Route("api/Crosswalk/fetchAllEmployeeDataBySchoolNameCrossWalked/{SchoolName}")]
        [HttpGet]
        public List<EmployeeTable> fetchAllEmployeeDataBySchoolNameCrossWalked(string SchoolName)
        {
            EmployeeTable oEmployeeTable;
            List<EmployeeTable> lstEmployeeTableData = new List<EmployeeTable>();

            var connectionString = "";
            string SQLCommandText = "";


            var SQLCommandTextNew = @"SELECT a.Employee as EmployeeID,a.Org_Unit_Name as SchoolName,a.Employee_Name as EmployeeName,
                                     [Role] = CASE
								    WHEN b.[SchoolName] <> a.Org_Unit_Name THEN CONCAT(a.Position, ' - ' + a.Position_Name + ' <-> ' + b.[SchoolName])
								   ELSE
								    CONCAT(a.Position, ' - ' + a.Position_Name)
								   END,
                                    a.Position,
                                    b.Position as PositionName,b.PositionID,
                                    e.Eligibility,
                                    a.Status,
                                    c.[Qualification Text] As Certification,'' As Certification, b.CRecordID,b.Position,
                                    CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END,
									d.Notes
									FROM 
                                    [YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    LEFT JOIN EligibilityTable e on a.Employee = e.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
									LEFT JOIN EmployeeNotesCrossWalked d on a.Employee = d.EmployeeID
                                    WHERE
                                    a.Employee IN (SELECT b.EmployeeID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                                    AND
                                    a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1
                                     OR
                                    a.Employee IN(SELECT x.EmployeeID FROM CrossWalk x WHERE x.SchoolName =  @SchoolName AND a.Org_Unit_Name != x.SchoolName)
                                     ORDER BY b.Position ";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
            }


            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandTextNew, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@SchoolName", SchoolName);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oEmployeeTable = new EmployeeTable();
                        oEmployeeTable.EmployeeID = row["EmployeeID"].ToString();
                        oEmployeeTable.SchoolName = row["SchoolName"].ToString();

                        oEmployeeTable.EmployeeName = row["EmployeeName"].ToString();
                        oEmployeeTable.Role = row["Role"].ToString();
                        oEmployeeTable.Certification = row["Certification"].ToString();
                        oEmployeeTable.Position = row["Position"].ToString();
                        oEmployeeTable.PositionName = row["PositionName"].ToString();
                        oEmployeeTable.CrossWalked = row["CrossWalked"].ToString();
                        oEmployeeTable.Eligibility = row["Eligibility"].ToString();
                        oEmployeeTable.Status = row["Status"].ToString();
                        oEmployeeTable.SchoolName = row["SchoolName"].ToString();
                        if (row["PositionID"].ToString().Length > 2)
                        {
                            oEmployeeTable.PositionID = int.Parse(row["PositionID"].ToString());
                        }
                        else
                        {
                            oEmployeeTable.PositionID = 0;
                        }
                        oEmployeeTable.Notes = row["Notes"].ToString();

                        lstEmployeeTableData.Add(oEmployeeTable);
                        oEmployeeTable = null;
                    }
                }
            }

            return lstEmployeeTableData;
        }

        [Route("api/Crosswalk/fetchEmployeesSchoolNameEmployeeLookup/{employeeID}")]
        [HttpGet]
        public List<EmployeeTable> fetchEmployeesSchoolNameEmployeeLookup(string employeeID)
        {
            EmployeeTable oEmployeeTable;
            List<EmployeeTable> lstEmployeeTableData = new List<EmployeeTable>();

            var connectionString = "";
            string SQLCommandText = "";

            var SchoolName = fetchSchoolNameByEmployeeID(employeeID);

           

            
            var SQLCommandTextNew = @"SELECT a.Employee as EmployeeID,
                                   a.Org_Unit_Name as SchoolName,
	                               a.Employee_Name as EmployeeName,
                                   
                                   CONCAT(a.Position, ' - ' + a.Position_Name) AS Role,

                                   PositionName = CASE
								   WHEN b.[SchoolName] <> a.Org_Unit_Name THEN CONCAT(b.Position,  ' <-> ' + b.[SchoolName])
								   ELSE
								   b.Position
								   END,

                                   CONCAT(a.Employee, b.PositionID) CWKey,
                                    ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS fake_id,
                                   CRossWalkDiff = CASE
                                   WHEN a.Org_Unit_Name != b.SchoolName THEN 'YES' ELSE 'NO' END,
                                    b.SchoolName as CWSchool,
                                   b.PositionID,
                                   --b.Position as PositionName,
	                               d.Eligibility,
								   c.[Qualification Text] As Certification,
                                   '' As Certification,
                                   b.CRecordID,b.Position,
	                                CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    LEFT JOIN EligibilityTable d on a.Employee = d.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
                                    WHERE a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1
                                    OR
                                    a.Employee IN(SELECT x.EmployeeID FROM CrossWalk x WHERE x.SchoolName = @SchoolName AND a.Org_Unit_Name != x.SchoolName)
                                    ORDER BY
                                    b.Position ASC";
      
           
            

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
            }


            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandTextNew, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@SchoolName", SchoolName);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oEmployeeTable = new EmployeeTable();
                        oEmployeeTable.EmployeeID = row["EmployeeID"].ToString();
                        oEmployeeTable.SchoolName = row["SchoolName"].ToString();

                        oEmployeeTable.EmployeeName = row["EmployeeName"].ToString();
                        oEmployeeTable.Role = row["Role"].ToString();
                        oEmployeeTable.Certification = row["Certification"].ToString();
                        oEmployeeTable.Position = row["Position"].ToString();
                        oEmployeeTable.PositionName = row["PositionName"].ToString();
                        oEmployeeTable.CrossWalked = row["CrossWalked"].ToString();
                        oEmployeeTable.Eligibility = row["Eligibility"].ToString();
                        oEmployeeTable.CRossWalkDiff = row["CRossWalkDiff"].ToString();
                        if (row["PositionID"].ToString().Length > 2)
                        {
                            oEmployeeTable.PositionID = int.Parse(row["PositionID"].ToString());
                        }
                        else
                        {
                            oEmployeeTable.PositionID = 0;
                        }
                        oEmployeeTable.fake_id = row["fake_id"].ToString();

                        lstEmployeeTableData.Add(oEmployeeTable);
                        oEmployeeTable = null;
                    }
                }
            }

            return lstEmployeeTableData;
        }

        [Route("api/Crosswalk/fetchSchoolNameByEmployeeID/{employeeID}")]
        [HttpGet]
        public string fetchSchoolNameByEmployeeID(string employeeID)
        {
            var connectionString = "";
            var school = "";

            var SQLCommandTextNew = "SELECT a.Org_Unit_Name FROM [YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a  WHERE a.Employee = ";
            SQLCommandTextNew += int.Parse(employeeID);


            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }


            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandTextNew, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    //cmd.Parameters.AddWithValue("@employeeID", employeeID);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        school = ds.Tables[0].Rows[0]["Org_Unit_Name"].ToString();
                    }

                }
            }

            return school;
        }
        #endregion FETCH_EMPLOYEE_DATA

        #region FetchsAPRSchoolListingsCertETC
        [Route("api/Crosswalk/fetchAPRData/{SchoolName}")]
        [HttpGet]
        public List<APRReport> fetchAPRData(string SchoolName)
        {
            APRReport oAPRReport;
            List<APRReport> lstAPRReportData = new List<APRReport>();

            var connectionString = "";
            //string SQLCommandText = "";

            var SQLCommandText = @"SELECT a.NES,
                              a.Employee,
                              a.Employee_Name,
                              a.Position,
                              Position_Name,
                              a.Job,
                              a.Job_Name,
                              a.Status,
                              Status as CSS,
                              '' as Intent,
                              d.Eligibility,
							  b.PositionID,
							  b.Position  as CPosition,
							  b.SchoolName,
							   CrossWalked = CASE
                               WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END,
							   c.CERTIFICATIONS as Certification
                              FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                               LEFT JOIN CrossWalk b
                               ON a.Employee = b.EmployeeID
                                LEFT JOIN EMPLOYEE_CERT_TABLE c
                               ON a.Employee = c.Employee
                                LEFT JOIN EligibilityTable d on a.Employee = d.EmployeeID
                              WHERE Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }


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
                        oAPRReport = new APRReport();
                        oAPRReport.Employee = row["Employee"].ToString();
                        oAPRReport.Employee_Name = row["Employee_Name"].ToString();
                        oAPRReport.CSS = row["CSS"].ToString();   
                        oAPRReport.Position = row["Position"].ToString();
                        oAPRReport.Position_Name = row["Position_Name"].ToString();
                        oAPRReport.Intent = row["Intent"].ToString();
                        oAPRReport.Eligibility = row["Eligibility"].ToString();
                        oAPRReport.Certification = row["Certification"].ToString();
                        oAPRReport.CrossWalked = row["CrossWalked"].ToString();
                        oAPRReport.PositionID = row["PositionID"].ToString();
                        oAPRReport.CPosition = row["CPosition"].ToString();
                        oAPRReport.SchoolName = row["SchoolName"].ToString();
      
                        lstAPRReportData.Add(oAPRReport);
                        oAPRReport = null;
                    }
                }
            }

            return lstAPRReportData;
        }

        [Route("api/Crosswalk/fetchAPRDataMultiSchools/{SchoolNameList}")]
        [HttpGet]
        public List<APRReport> fetchAPRDataMultiSchools(string SchoolNameList)
        {
            APRReport oAPRReport;
            List<APRReport> lstAPRReportData = new List<APRReport>();
            string formattedValues = string.Join(",", SchoolNameList.Split(',').Select(n => $"'{n.Trim()}'"));

            //string sql = $"SELECT * FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT WHERE Org_Unit_Name IN ({formattedValues})";
            //Console.WriteLine(sql);

            string sql2 = $@"SELECT a.NES,
                              a.Employee,
                              a.Employee_Name,
                              a.Position,
                              Position_Name,
                              CONCAT(a.Position, ' - ' + a.Position_Name) AS Role,
                              a.Job,
                              a.Job_Name,
                              a.Status,
                              Status as CSS,
                              '' as Intent,
                              d.Eligibility,
							  b.PositionID,
							  b.Position as CPosition,
                                CONCAT( b.PositionID, ' - ' + b.Position) AS FCWP,
							 -- b.SchoolName,
                                a.Org_Unit_Name as SchoolName,
							   CrossWalked = CASE
                               WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END,
							   c.CERTIFICATIONS as Certification
                              FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                               LEFT JOIN CrossWalk b
                               ON a.Employee = b.EmployeeID
                                LEFT JOIN EMPLOYEE_CERT_TABLE c
                               ON a.Employee = c.Employee
                                LEFT JOIN EligibilityTable d on a.Employee = d.EmployeeID
                              WHERE Org_Unit_Name IN ({formattedValues}) AND LEN(a.Employee) > 1
                               ORDER BY a.Org_Unit_Name ASC";



            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

          

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    //cmd.Parameters.AddWithValue("@formattedValues", formattedValues);
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oAPRReport = new APRReport();
                        oAPRReport.Employee = row["Employee"].ToString();
                        oAPRReport.Employee_Name = row["Employee_Name"].ToString();
                        oAPRReport.CSS = row["CSS"].ToString();
                        oAPRReport.Position = row["Position"].ToString();
                        oAPRReport.Position_Name = row["Position_Name"].ToString();
                        oAPRReport.Intent = row["Intent"].ToString();
                        oAPRReport.Eligibility = row["Eligibility"].ToString();
                        oAPRReport.Certification = row["Certification"].ToString();
                        oAPRReport.CrossWalked = row["CrossWalked"].ToString();
                        oAPRReport.PositionID = row["PositionID"].ToString();
                        oAPRReport.CPosition = row["CPosition"].ToString();
                        oAPRReport.SchoolName = row["SchoolName"].ToString();
                        oAPRReport.FCWP = row["FCWP"].ToString(); 
                        oAPRReport.Role = row["Role"].ToString();

                        lstAPRReportData.Add(oAPRReport);
                        oAPRReport = null;
                    }
                }
            }

            return lstAPRReportData;
        }

        [Route("api/Crosswalk/fetchAPRDataMultiSchoolsALLSchools/")]
        [HttpGet]
        public List<APRReport> fetchAPRDataMultiSchoolsALLSchools()
        {
            APRReport oAPRReport;
            List<APRReport> lstAPRReportData = new List<APRReport>();
          

            string sql2 = $@"SELECT a.NES,
                              a.Employee,
                              a.Employee_Name,
                              a.Org_Unit_Name as SchoolName,
                              a.Position,
                              Position_Name,
                              CONCAT(a.Position, ' - ' + a.Position_Name) AS Role,
                              a.Job,
                              a.Job_Name,
                              a.Status,
                              Status as CSS,
                              '' as Intent,
                              d.Eligibility,
							  b.PositionID,
							  b.Position as CPosition,
                                CONCAT( b.PositionID, ' - ' + b.Position) AS FCWP,
							  --b.SchoolName,
							   CrossWalked = CASE
                               WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END,
							   c.CERTIFICATIONS as Certification
                              FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                               LEFT JOIN CrossWalk b
                               ON a.Employee = b.EmployeeID
                                LEFT JOIN EMPLOYEE_CERT_TABLE c
                               ON a.Employee = c.Employee
                                LEFT JOIN EligibilityTable d on a.Employee = d.EmployeeID
                              WHERE LEN(a.Employee) > 1
                              ORDER BY a.Org_Unit_Name ASC";



            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }



            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    //cmd.Parameters.AddWithValue("@formattedValues", formattedValues);
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oAPRReport = new APRReport();
                        oAPRReport.Employee = row["Employee"].ToString();
                        oAPRReport.Employee_Name = row["Employee_Name"].ToString();
                        oAPRReport.CSS = row["CSS"].ToString();
                        oAPRReport.Position = row["Position"].ToString();
                        oAPRReport.Position_Name = row["Position_Name"].ToString();
                        oAPRReport.Intent = row["Intent"].ToString();
                        oAPRReport.Eligibility = row["Eligibility"].ToString();
                        oAPRReport.Certification = row["Certification"].ToString();
                        oAPRReport.CrossWalked = row["CrossWalked"].ToString();
                        oAPRReport.PositionID = row["PositionID"].ToString();
                        oAPRReport.CPosition = row["CPosition"].ToString();
                        oAPRReport.SchoolName = row["SchoolName"].ToString();
                        oAPRReport.Role = row["Role"].ToString();
                        oAPRReport.FCWP = row["FCWP"].ToString();

                        lstAPRReportData.Add(oAPRReport);
                        oAPRReport = null;
                    }
                }
            }

            return lstAPRReportData;
        }

        [Route("api/Crosswalk/fetchSchoolListingsRealData/")]
        [HttpGet]
        public List<SchoolListing> fetchSchoolListingsRealData()
        {
            SchoolListing oSchoolLisingData;
            List<SchoolListing> lstSchoolistingData = new List<SchoolListing>();


            var connectionString = "";
            string SQLCommandText = "";

            SQLCommandText = $"SELECT DISTINCT([Org_Unit_Name]),Org_Unit FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT WHERE  LEN([Org_Unit_Name]) > 2 AND NES = 'NES'";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }


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
                        oSchoolLisingData = new SchoolListing();

                        if (row["Org_Unit_Name"] != null && row["Org_Unit"].ToString() != "")
                        {
                            oSchoolLisingData.EducationOrgNaturalKey = row["Org_Unit"].ToString();
                        }
                        else
                        {
                            oSchoolLisingData.EducationOrgNaturalKey = "N/A";
                        }


                        oSchoolLisingData.NameOfInstitution = row["Org_Unit_Name"].ToString();
                        lstSchoolistingData.Add(oSchoolLisingData);
                        oSchoolLisingData = null;
                    }
                }
            }

            return lstSchoolistingData;
        }

     
        [Route("api/Crosswalk/fetchSchoolListingsFilteredByDivAndUnit/{area}")]
        [HttpGet]
        public List<SchoolListing> fetchSchoolListingsFilteredByDivAndUnit(string area)
        {
            SchoolListing oSchoolLisingData;
            List<SchoolListing> lstSchoolistingData = new List<SchoolListing>();

            string Division = "'" + area.Split('|')[0].ToString() + "'";
            string Unit = "'" + area.Split('|')[1].ToString() + "'";



            var connectionString = "";
            string SQLCommandText = "";

            SQLCommandText = $"SELECT DISTINCT([Org_Unit_Name]),Org_Unit FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT WHERE [Division] = {Division} AND Unit = {Unit}  " +
                             $"AND LEN([Org_Unit_Name]) > 2 AND NES = 'NES'";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }


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
                        oSchoolLisingData = new SchoolListing();

                        if (row["Org_Unit_Name"] != null && row["Org_Unit"].ToString() != "")
                        {
                            oSchoolLisingData.EducationOrgNaturalKey = row["Org_Unit"].ToString();
                        }
                        else
                        {
                            oSchoolLisingData.EducationOrgNaturalKey = "N/A";
                        }


                        oSchoolLisingData.NameOfInstitution = row["Org_Unit_Name"].ToString();
                        lstSchoolistingData.Add(oSchoolLisingData);
                        oSchoolLisingData = null;
                    }
                }
            }

            return lstSchoolistingData;
        }


        [Route("api/Crosswalk/fetchCERTData/{employeeID}")]
        [HttpGet]
        public List<CertificationData> fetchCERTData(string employeeID)
        {
            CertificationData oCERTData;
            List<CertificationData> lstCERTData = new List<CertificationData>();

            var connectionString = "";
            string SQLCommandText = "";
            /*
            SQLCommandText = @"select [Employee], [Employee Text] as EmployeeName,[Qualification Text] As Certification from YPBI_HPA_ZEMPQUAL_CERT_TABLE
	                            WHERE [Employee] = @employeeID";
            */
            SQLCommandText = @"Select [Employee], EmployeeName,CERTIFICATIONS As Certification from EMPLOYEE_CERT_TABLE
                             WHERE [Employee] = @employeeID";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }


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


        [Route("api/Crosswalk/fetchCERTDataLB/{employeeID}")]
        [HttpGet]
        public List<CertificationData> fetchCERTDataLB(string employeeID)
        {
            CertificationData oCERTData;
            List<CertificationData> lstCERTData = new List<CertificationData>();

            var connectionString = "";
            string SQLCommandText = "";
          
            SQLCommandText = @"Select [Employee], EmployeeName,CERTIFICATIONS As Certification from EMPLOYEE_CERT_TABLE_LB
                             WHERE [Employee] = @employeeID";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }


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

        #endregion FetchsAPRSchoolListingsCertETC

        #region CrosswalkRecordMgmt
        [HttpPost]
        [Route("api/Crosswalk/AddOrUpdateCrosswalkRecord")]
        public bool AddOrUpdateCrosswalkRecord([FromBody] CrosswalkData oCrosswalkEntryData)
        {
            bool bSuccess = false;
            //CALL DELETE NEXT STEP AND NOTE RECORD TO KEEP CROSSWALK AND NEXT/NOTES IN SYNC
            var EmpIDAndSchoolName = oCrosswalkEntryData.EmployeeID + '|' + oCrosswalkEntryData.SchoolName;
            DeleteEmployeeNextSteps(EmpIDAndSchoolName);
            DeleteEmployeeNotesNotCrosswalked(EmpIDAndSchoolName);

            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":
                    //use local as the same for connection string while in test
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }

            //IF NOT EXISTS(SELECT 1 FROM StudentEntryData WHERE id = @id)
            const string sql2 = @"
                                IF NOT EXISTS(SELECT 1 FROM CrossWalk WHERE EmployeeID = @EmployeeID AND SchoolName = @SchoolName)
                                    BEGIN
                                        INSERT INTO CrossWalk(
                                           EmployeeID,
                                            PositionID,
                                            Position ,
                                            SchoolName,
                                            DateAdded )
                                        VALUES(
                                                @EmployeeID ,
                                                @PositionID,
                                                @Position ,
                                                @SchoolName,
                                                @DateAdded )
                                    END
                                ELSE
                                    BEGIN
                                       UPDATE CrossWalk SET 
                                       EmployeeID = @EmployeeID ,
                                       Position = @Position ,
                                       PositionID= @PositionID ,
                                       SchoolName = @SchoolName,
                                       DateAdded = @DateAdded 
                                       WHERE EmployeeID = @EmployeeID AND SchoolName = @SchoolName
                                    END";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@EmployeeID", oCrosswalkEntryData.EmployeeID);
                    cmd.Parameters.AddWithValue("@PositionID", oCrosswalkEntryData.PositionID);
                    cmd.Parameters.AddWithValue("@Position", oCrosswalkEntryData.Position);
                    cmd.Parameters.AddWithValue("@SchoolName", oCrosswalkEntryData.SchoolName);
                    cmd.Parameters.AddWithValue("@DateAdded", DateTime.Today);
                    


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

  
        [HttpPost]
        [Route("api/Crosswalk/UpdateCrosswalkRecord")]
        public bool UpdateCrosswalkRecord([FromBody] CrosswalkData oCrosswalkEntryData)
        {
            bool bSuccess = false;


            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":
                    //use local as the same for connection string while in test
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }


            const string sql2 = @"UPDATE CrossWalk SET 
                                       Position = @Position ,
                                       PositionID = @PositionID,
                                       DateAdded = @DateAdded 
                                       WHERE EmployeeID = @EmployeeID AND SchoolName = @SchoolName";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@EmployeeID", oCrosswalkEntryData.EmployeeID);
                    cmd.Parameters.AddWithValue("@Position", oCrosswalkEntryData.Position);
                    cmd.Parameters.AddWithValue("@PositionID", oCrosswalkEntryData.PositionID);
                    cmd.Parameters.AddWithValue("@SchoolName", oCrosswalkEntryData.SchoolName);
                    cmd.Parameters.AddWithValue("@DateAdded", DateTime.Today);


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

    
        [HttpPost]
        [Route("api/Crosswalk/DeleteCrosswalkRecord")]
        public bool DeleteCrosswalkRecord([FromBody] CrosswalkData oCrosswalkEntryData)
        {
            bool bSuccess = false;
            //CALL DELETE NEXT STEP AND NOTE RECORD TO KEEP CROSSWALK AND NEXT/NOTES IN SYNC
            var EmpIDAndSchoolName = oCrosswalkEntryData.EmployeeID + '|' + oCrosswalkEntryData.SchoolName;
            DeleteEmployeeNextSteps(EmpIDAndSchoolName);
            DeleteEmployeeNotesNotCrosswalked(EmpIDAndSchoolName);

            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }

            string SQLCommandText = "";
            SQLCommandText = @"DELETE CrossWalk ";
            SQLCommandText += "WHERE SchoolName = ";
            SQLCommandText += "'";
            SQLCommandText += oCrosswalkEntryData.SchoolName;
            SQLCommandText += "'";
            SQLCommandText += " AND ";
            SQLCommandText += "Position = ";
            SQLCommandText += "'";
            SQLCommandText += oCrosswalkEntryData.Position;
            SQLCommandText += "'";
            SQLCommandText += " AND ";
            SQLCommandText += "EmployeeID = ";
            SQLCommandText += "'";
            SQLCommandText += oCrosswalkEntryData.EmployeeID;
            SQLCommandText += "'";


            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
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
        #endregion CrosswalkRecordMgmt


        #region CrosswalkEmployeeDiffSchool

        [Route("api/Crosswalk/fetchEmployeeCrosswalkDiffSchool/{employeename}")]
        [HttpGet]
        public List<EmployeeTable> fetchEmployeeCrosswalkDiffSchool(string employeename)
        {
            EmployeeTable oEmployeeTable;
            List<EmployeeTable> lstEmployeeTableData = new List<EmployeeTable>();

            var connectionString = "";
            string SQLCommandText = "";



            var SQLCommandTextNew = @"SELECT a.Employee as EmployeeID,
                                   a.Org_Unit_Name as SchoolName,
	                               a.Employee_Name as EmployeeName,
	                                 [Role] = CASE
								   WHEN b.[SchoolName] <> a.Org_Unit_Name THEN CONCAT(a.Position, ' - ' + a.Position_Name + ' <-> ' + b.[SchoolName])
								   ELSE
								    CONCAT(a.Position, ' - ' + a.Position_Name)
								   END,

								   CONCAT(a.Employee,b.PositionID) CWKey,
                                   CRossWalkDiff = CASE
                                   WHEN a.Org_Unit_Name != b.SchoolName THEN 'YES' ELSE 'NO' END,

                                   b.PositionID,
                                    a.Status,
                                   b.Position as PositionName,
                                    d.Eligibility,
								   c.[Qualification Text] As Certification,
                                   b.CRecordID,b.Position,
                                   b.SchoolName as CWSchool,
	                                CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    LEFT JOIN EligibilityTable d on a.Employee = d.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
                                    WHERE 
                                    --a.Employee NOT IN (SELECT b.EmployeeID FROM CrossWalk b WHERE b.Position IS NOT NULL)
									--AND
                                    a.Employee_Name like ";
                                    SQLCommandTextNew += "'%";
                                    SQLCommandTextNew += employeename;
                                    SQLCommandTextNew += "%'";
                                    SQLCommandTextNew += " ";
                                    SQLCommandTextNew += " ORDER BY b.Position ";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                 
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                
                    break;
            }


            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandTextNew, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    //cmd.Parameters.AddWithValue("@employeename", employeename);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oEmployeeTable = new EmployeeTable();
                        oEmployeeTable.EmployeeID = row["EmployeeID"].ToString();
                        oEmployeeTable.SchoolName = row["SchoolName"].ToString();
                        oEmployeeTable.EmployeeName = row["EmployeeName"].ToString();
                        oEmployeeTable.Role = row["Role"].ToString();
                        oEmployeeTable.Certification = row["Certification"].ToString();
                        oEmployeeTable.Position = row["Position"].ToString();
                        oEmployeeTable.PositionName = row["PositionName"].ToString();
                        oEmployeeTable.CrossWalked = row["CrossWalked"].ToString();
                        oEmployeeTable.CWSchool = row["CWSchool"].ToString();
                        oEmployeeTable.CWKey = row["CWKey"].ToString();
                        oEmployeeTable.CRossWalkDiff = row["CRossWalkDiff"].ToString();
                        oEmployeeTable.Eligibility = row["Eligibility"].ToString();

                        if (row["PositionID"].ToString().Length > 2)
                        {
                            oEmployeeTable.PositionID = int.Parse(row["PositionID"].ToString());
                        }
                        else
                        {
                            oEmployeeTable.PositionID = 0;
                        }

                        lstEmployeeTableData.Add(oEmployeeTable);
                        oEmployeeTable = null;
                    }
                }
            }

            return lstEmployeeTableData;
        }


        [Route("api/Crosswalk/fetchEmployeeCrosswalkDiffSchoolEmployeeID/{employeeID}")]
        [HttpGet]
        public List<EmployeeTable> fetchEmployeeCrosswalkDiffSchoolEmployeeID(string employeeID)
        {
            EmployeeTable oEmployeeTable;
            List<EmployeeTable> lstEmployeeTableData = new List<EmployeeTable>();

            var connectionString = "";
            string SQLCommandText = "";



            var SQLCommandTextNew = @"SELECT a.Employee as EmployeeID,
                                   a.Org_Unit_Name as SchoolName,
	                               a.Employee_Name as EmployeeName,
	                                 [Role] = CASE
								    WHEN b.[SchoolName] <> a.Org_Unit_Name THEN CONCAT(a.Position, ' - ' + a.Position_Name + ' <-> ' + b.[SchoolName])
								   ELSE
								    CONCAT(a.Position, ' - ' + a.Position_Name)
								   END,
                                    CONCAT(a.Employee,b.PositionID) CWKey,
                                    CRossWalkDiff = CASE
                                   WHEN a.Org_Unit_Name != b.SchoolName THEN 'YES' ELSE 'NO' END,
                                   b.PositionID,
                                    a.Status,
                                   b.Position as PositionName,
                                    b.SchoolName as CWSchool,
								   c.[Qualification Text] As Certification,
                                   b.CRecordID,b.Position,
                                   d.Eligibility,
	                                CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    LEFT JOIN EligibilityTable d on a.Employee = d.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
                                   
                                    WHERE 
                                    --a.Employee NOT IN (SELECT b.EmployeeID FROM CrossWalk b WHERE b.Position IS NOT NULL)
									--AND
                                   a.Employee =  ";
                                   SQLCommandTextNew += employeeID;
                                   SQLCommandTextNew += " ";
                                   SQLCommandTextNew += " ORDER BY b.Position ";


            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }


            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandTextNew, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    //cmd.Parameters.AddWithValue("@employeename", employeename);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oEmployeeTable = new EmployeeTable();
                        oEmployeeTable.EmployeeID = row["EmployeeID"].ToString();
                        oEmployeeTable.SchoolName = row["SchoolName"].ToString();
                        oEmployeeTable.EmployeeName = row["EmployeeName"].ToString();
                        oEmployeeTable.Role = row["Role"].ToString();
                        oEmployeeTable.Certification = row["Certification"].ToString();
                        oEmployeeTable.Position = row["Position"].ToString();
                        oEmployeeTable.PositionName = row["PositionName"].ToString();
                        oEmployeeTable.CrossWalked = row["CrossWalked"].ToString();
                        oEmployeeTable.CWSchool = row["CWSchool"].ToString();
                        oEmployeeTable.CWKey = row["CWKey"].ToString();
                        oEmployeeTable.CRossWalkDiff = row["CRossWalkDiff"].ToString();
                        oEmployeeTable.Eligibility = row["Eligibility"].ToString();

                        if (row["PositionID"].ToString().Length > 2)
                        {
                            oEmployeeTable.PositionID = int.Parse(row["PositionID"].ToString());
                        }
                        else
                        {
                            oEmployeeTable.PositionID = 0;
                        }

                        lstEmployeeTableData.Add(oEmployeeTable);
                        oEmployeeTable = null;
                    }
                }
            }

            return lstEmployeeTableData;
        }

        [Route("api/Crosswalk/DeleteCrosswalkRecordDiffSchool/{EmpIDAndSchoolName}")]
        [HttpGet]
        public bool DeleteCrosswalkRecordDiffSchool(string EmpIDAndSchoolName)
        {


            string Employee = EmpIDAndSchoolName.Split('|')[0].ToString();
            string SchoolName = EmpIDAndSchoolName.Split('|')[1].ToString();

            bool bSuccess = false;
            var connectionString = "";

            var SQLCommandText = "DELETE CrossWalk WHERE EmployeeID = ";
            SQLCommandText += Employee;
            SQLCommandText += " AND ";
            SQLCommandText += "SchoolName =  ";
            SQLCommandText += "'";
            SQLCommandText += SchoolName;
            SQLCommandText += "'";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
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

        [Route("api/Crosswalk/DeleteCrosswalkRecordDiffSchoolRemoveAllOtherEntries/{employeeID}")]
        [HttpGet]
        public bool DeleteCrosswalkRecordDiffSchoolRemoveAllOtherEntries(string employeeID)
        {

            bool bSuccess = false;
            var connectionString = "";

            var SQLCommandText = "DELETE CrossWalk WHERE EmployeeID = ";
            SQLCommandText += employeeID;
         

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
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


        #endregion CrosswalkEmployeeDiffSchool

        #region NotesNextStepsCWAndNCW
        /*NOTES AND NEXT STEPS CROSSWALKED AND NOT CROSSWALKED  */
                [HttpPost]
        [Route("api/Crosswalk/AddUpdateEmployeeNotesCrossWalked")]
        public bool AddUpdateEmployeeNotesCrossWalked([FromBody] NotesNextStep oNotesNextStepData)
        {
            bool bSuccess = false;


            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":

                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }

            string sql2 = "";

            sql2 = @"IF NOT EXISTS(SELECT 1 FROM EmployeeNotesCrossWalked WHERE EmployeeID = @EmployeeID AND SchoolName = @SchoolName)
                                    BEGIN
                                      INSERT INTO EmployeeNotesCrossWalked (
                                            EmployeeID,
                                            Notes,
                                            SchoolName)
                                        VALUES(
                                            @EmployeeID,
                                            @Notes,
                                            @SchoolName)
                                    END
                                ELSE
                                    BEGIN
                                       UPDATE EmployeeNotesCrossWalked SET
                                       Notes = @Notes 
                                       WHERE EmployeeID = @EmployeeID AND SchoolName = @SchoolName
                                    END";




            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@EmployeeID", oNotesNextStepData.EmployeeID);
                    cmd.Parameters.AddWithValue("@Notes", oNotesNextStepData.Notes);
                    cmd.Parameters.AddWithValue("@SchoolName", oNotesNextStepData.SchoolName);

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
        [HttpPost]
        [Route("api/Crosswalk/AddUpdateEmployeeNotesNotCrosswalked")]
        public bool AddUpdateEmployeeNotesNotCrosswalked([FromBody] NotesNextStep oNotesNextStepData)
        {
            bool bSuccess = false;


            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":

                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }

            string sql2 = "";

            sql2 = @"IF NOT EXISTS(SELECT 1 FROM EmployeeNotesNotCrosswalked WHERE EmployeeID = @EmployeeID AND SchoolName = @SchoolName)
                                    BEGIN
                                      INSERT INTO EmployeeNotesNotCrosswalked (
                                            EmployeeID,
                                            Notes,
                                            SchoolName)
                                        VALUES(
                                            @EmployeeID,
                                            @Notes,
                                            @SchoolName)
                                    END
                                ELSE
                                    BEGIN
                                       UPDATE EmployeeNotesNotCrosswalked SET
                                       Notes = @Notes 
                                       WHERE EmployeeID = @EmployeeID AND SchoolName = @SchoolName
                                    END";




            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@EmployeeID", oNotesNextStepData.EmployeeID);
                    cmd.Parameters.AddWithValue("@Notes", oNotesNextStepData.Notes);
                    cmd.Parameters.AddWithValue("@SchoolName", oNotesNextStepData.SchoolName);

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

        [HttpPost]
        [Route("api/Crosswalk/AddUpdateEmployeeNextSteps")]
        public bool AddUpdateEmployeeNextSteps([FromBody] NotesNextStep oNotesNextStepData)
        {
            bool bSuccess = false;


            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":

                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }

            string sql2 = "";

            sql2 = @"IF NOT EXISTS(SELECT 1 FROM EmployeeNextSteps WHERE EmployeeID = @EmployeeID AND SchoolName = @SchoolName)
                                    BEGIN
                                      INSERT INTO EmployeeNextSteps (
                                            EmployeeID,
                                            NextStep,
                                            SchoolName)
                                        VALUES(
                                            @EmployeeID,
                                            @NextStep,
                                            @SchoolName)
                                    END
                                ELSE
                                    BEGIN
                                       UPDATE EmployeeNextSteps SET
                                       NextStep = @NextStep 
                                       WHERE EmployeeID = @EmployeeID AND SchoolName = @SchoolName
                                    END";




            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@EmployeeID", oNotesNextStepData.EmployeeID);
                    cmd.Parameters.AddWithValue("@NextStep", oNotesNextStepData.NextStep);
                    cmd.Parameters.AddWithValue("@SchoolName", oNotesNextStepData.SchoolName);
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
        
        
        
        [Route("api/Crosswalk/DeleteEmployeeNextSteps/{EmpIDAndSchoolName}")]
        [HttpGet]
        public bool DeleteEmployeeNextSteps(string EmpIDAndSchoolName)
        {


            string Employee = EmpIDAndSchoolName.Split('|')[0].ToString();
            string SchoolName = EmpIDAndSchoolName.Split('|')[1].ToString();

            bool bSuccess = false;
            var connectionString = "";

            var SQLCommandText = "DELETE EmployeeNextSteps WHERE EmployeeID = ";
            SQLCommandText += Employee;
            SQLCommandText += " AND ";
            SQLCommandText += "SchoolName =  ";
            SQLCommandText += "'";
            SQLCommandText += SchoolName;
            SQLCommandText += "'";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    //cmd.Parameters.AddWithValue("@Employee", int.Parse(Employee));
                    //cmd.Parameters.AddWithValue("@SchoolName", SchoolName);
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

        [Route("api/Crosswalk/DeleteEmployeeNotesNotCrosswalked/{EmpIDAndSchoolName}")]
        [HttpGet]
        public bool DeleteEmployeeNotesNotCrosswalked(string EmpIDAndSchoolName)
        {

            string Employee = EmpIDAndSchoolName.Split('|')[0].ToString();
            string SchoolName = EmpIDAndSchoolName.Split('|')[1].ToString();

            var SQLCommandText = "DELETE EmployeeNotesNotCrosswalked WHERE EmployeeID = ";
            SQLCommandText += Employee;
            SQLCommandText += " AND ";
            SQLCommandText += "SchoolName =  ";
            SQLCommandText += "'";
            SQLCommandText += SchoolName;
            SQLCommandText += "'";
            bool bSuccess = false;
            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    //cmd.Parameters.AddWithValue("@Employee", int.Parse(Employee));
                    //cmd.Parameters.AddWithValue("@SchoolName", SchoolName);
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
        /*END NOTES AND NEXT STEPS  */
        #endregion NotesNextStepsCWAndNCW

        #region NextStepAdmin
        /*NEXT STEPS LIST ITEMS - ADMIN SCREEN*/

        [Route("api/Crosswalk/fetchNextStepListItems/")]
        [HttpGet]
        public List<tblNextStep>fetchNextStepListItems()
        {

            tblNextStep oStepData;
            List<tblNextStep> lstStepData = new List<tblNextStep>();

            var connectionString = "";

            var SQLCommandText = @"select stepID,NextStep from tblNextStep";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

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
                        oStepData = new tblNextStep();
                        oStepData.stepID = int.Parse(row["stepID"].ToString());
                        oStepData.NextStep = row["NextStep"].ToString();
                        lstStepData.Add(oStepData);

                    }
                }
            }

            return lstStepData;
        }

        [HttpPost]
        [Route("api/Crosswalk/AddOrUpdateNextStepRecordListItems/")]
        public bool AddOrUpdateNextStepRecordListItems([FromBody] tblNextStep oStepData)
        {
            bool bSuccess = false;


            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":
                    //use local as the same for connection string while in test
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }

            const string sql2 = @"
                                IF NOT EXISTS(SELECT 1 FROM tblNextStep WHERE stepID = @stepID)
                                    BEGIN
                                        INSERT INTO tblNextStep(
                                           NextStep )
                                        VALUES(
                                               @NextStep)
                                    END
                                ELSE
                                    BEGIN
                                       UPDATE tblNextStep SET 
                                       NextStep = @NextStep
                                       WHERE stepID = @stepID 
                                    END";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@stepID", oStepData.stepID);
                    cmd.Parameters.AddWithValue("@NextStep", oStepData.NextStep);
                   
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

        [Route("api/Crosswalk/DeleteNextStepRecordListItems/{stepID}")]
        [HttpGet]
        public bool DeleteNextStepRecordListItems(int stepID)
        {

            bool bSuccess = false;
            var connectionString = "";

            //DELETE tblNextStep WHERE stepID = 7
            var SQLCommandText = @"DELETE tblNextStep WHERE stepID = @stepID";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@stepID", stepID);
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

        [Route("api/Crosswalk/AddNextStepRecordListItem/{NextStep}")]
        [HttpGet]
        public bool AddNextStepRecordListItem(string NextStep)
        {

            bool bSuccess = false;
            var connectionString = "";

            var SQLCommandText = @"INSERT INTO tblNextStep([NextStep]) VALUES(@NextStep)";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@NextStep", NextStep);
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

        /*END NEXT STEPS*/
        #endregion NextStepAdmin


        #region Charting
        /*CHARTING DATA*/
        [Route("api/Crosswalk/fetchCrosswalkDashboardData/{SchoolName}")]
        [HttpGet]
        public List<ChartDataMainDBoard> fetchCrosswalkDashboardData(string SchoolName)
        {

            ChartDataMainDBoard oCrosswalkChartData;
            List<ChartDataMainDBoard> lstCrosswalkChartData = new List<ChartDataMainDBoard>();

            var connectionString = "";

            var SQLCommandTextx = @"SELECT count(CRecordID) as NumCrosswalked,[SchoolName] from CrossWalk
                                  group by 
                                  [SchoolName]";


            /*
              var SQLCommandText = @"WITH AssignedCount AS(
                      SELECT COUNT(PositionID) AS ASSIGNED
                      FROM CrossWalk
                      WHERE SchoolName  = @SchoolName
                  ),
                  NotStartedCount AS(
                      SELECT COUNT(a.Position) AS NOTSTARTED
                      FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                      LEFT JOIN CrossWalk b ON a.[Position_Name] = b.Position
                      WHERE LEN([Org_Unit_Name]) > 2
                        AND NES = 'NES'
                        AND[Org_Unit_Name] = @SchoolName
                        AND a.Employee NOT IN(
                            SELECT b.EmployeeID
                            FROM CrossWalk b
                            WHERE b.EmployeeID IS NOT NULL
                        )
                        AND LEN(a.Employee) > 1
                  ),
                  InProgressCount AS(
                      SELECT COUNT(a.Position) AS INPROGRESS
                      FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                      LEFT JOIN CrossWalk b ON a.[Position_Name] = b.Position
                      WHERE LEN([Org_Unit_Name]) > 2
                        AND NES = 'NES'
                        AND[Org_Unit_Name] = @SchoolName
                        AND a.Employee NOT IN(
                            SELECT b.EmployeeID
                            FROM CrossWalk b
                            WHERE b.EmployeeID IS NOT NULL
                        )
                        AND LEN(a.Employee) > 1
                  ),
                  SubmittedCount AS(
                     SELECT COUNT(PositionID) AS SUBMITTED
                      FROM CrossWalk
                      WHERE SchoolName= @SchoolName
                  )
                  SELECT
                      ac.ASSIGNED, 
                      nsc.NOTSTARTED,
                      ipc.INPROGRESS,
                      scnt.SUBMITTED
                  FROM
                      AssignedCount ac,
                      NotStartedCount nsc,
                      InProgressCount ipc,
                      SubmittedCount scnt";

      */

            var SQLCommandText = @"WITH AssignedCount AS(
                            SELECT COUNT(PositionID) AS ASSIGNED
                            FROM CrossWalk
                            WHERE SchoolName= @SchoolName
                        ),
                        NotStartedCount AS(
                           SELECT  Count(a.Position) as NOTSTARTED
							  FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							  LEFT JOIN CrossWalk b on a.[Position_Name] = b.Position
                              WHERE LEN([Org_Unit_Name]) > 2 AND NES = 'NES'
                              AND
                              [Org_Unit_Name] = @SchoolName
							  AND
							  a.Position NOT IN (SELECT b.PositionID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                        ),
                        InProgressCount AS(
                            SELECT COUNT(a.Position) AS INPROGRESS
                            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                            LEFT JOIN CrossWalk b ON a.[Position_Name] = b.Position
                            WHERE LEN([Org_Unit_Name]) > 2
                              AND NES = 'NES'
                              AND[Org_Unit_Name] = @SchoolName
                              AND a.Employee NOT IN(
                                  SELECT b.EmployeeID
                                  FROM CrossWalk b
                                  WHERE b.EmployeeID IS NOT NULL
                              )
                             -- AND LEN(a.Employee) > 1
                        ),
                        SubmittedCount AS(
                           SELECT COUNT(PositionID) AS SUBMITTED
                            FROM CrossWalk
                            WHERE SchoolName= 'Alcott ES'
                        )
                        SELECT
                            ac.ASSIGNED, 
                            nsc.NOTSTARTED,
	                        ipc.INPROGRESS,
	                        scnt.SUBMITTED
                        FROM
                            AssignedCount ac,
                            NotStartedCount nsc,
	                        InProgressCount ipc,
                            SubmittedCount scnt";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        oCrosswalkChartData = new ChartDataMainDBoard();
                        oCrosswalkChartData.ASSIGNED = int.Parse(row["ASSIGNED"].ToString());
                        oCrosswalkChartData.NOTSTARTED = int.Parse(row["NOTSTARTED"].ToString());
                        oCrosswalkChartData.INPROGRESS = int.Parse(row["INPROGRESS"].ToString());
                        oCrosswalkChartData.SUBMITTED = int.Parse(row["SUBMITTED"].ToString());
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }

        [Route("api/Crosswalk/fetchCrosswalPostionStatus/")]
        [HttpGet]
        public List<ChartDataPositionStatus> fetchCrosswalPostionStatus()
        {

            ChartDataPositionStatus oCrosswalkChartData;
            List<ChartDataPositionStatus> lstCrosswalkChartData = new List<ChartDataPositionStatus>();

            var connectionString = "";

           
            var SQLCommandText = @"WITH CurrentStaffCount AS(
                             SELECT COUNT(Employee) AS CurrentStaff
                            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT
                            WHERE NES = 'NES'
                        ),
                        TransfeCount AS(
                           SELECT COUNT(PositionID) AS Transfers
                            FROM CrossWalk
                        ),
                        VacantCount AS(
                         SELECT 30 AS Vacant
                        )
                        SELECT
                            csc.CurrentStaff, 
                            trc.Transfers,
	                        vc.Vacant
                        FROM
                            CurrentStaffCount csc,
                            TransfeCount trc,
	                        VacantCount vc
                       ";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        oCrosswalkChartData = new ChartDataPositionStatus();
                        oCrosswalkChartData.CURRENTSTAFF = int.Parse(row["CurrentStaff"].ToString());
                        oCrosswalkChartData.TRANSFERS = int.Parse(row["Transfers"].ToString());
                        oCrosswalkChartData.VACANT = int.Parse(row["Vacant"].ToString());
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }


        [Route("api/Crosswalk/fetchCrosswalkSubmissionStatus/")]
        [HttpGet]
        public List<ChartDataSubmissionStatus> fetchCrosswalkSubmissionStatus()
        {

            ChartDataSubmissionStatus oCrosswalkChartData;
            List<ChartDataSubmissionStatus> lstCrosswalkChartData = new List<ChartDataSubmissionStatus>();

            var connectionString = "";


            var SQLCommandText = @"WITH 
                        submittedCount AS (
                            SELECT COUNT(a.Employee) AS Submitted
                            FROM AknowledgementTable a
                        ),
                        InProgressCount AS (
                            SELECT COUNT(a.Employee) AS InProgress
                            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                            WHERE a.Employee IN (SELECT b.EmployeeID FROM CrossWalk b)
                            AND a.NES = 'NES'
                        ),
                        NotStartedCount AS (
                            SELECT COUNT(a.Employee) AS NotStarted
                            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                            WHERE a.Employee NOT IN (SELECT b.EmployeeID FROM CrossWalk b)
                            AND a.NES = 'NES'
                        )
                        SELECT 'Submitted' AS Category, COALESCE(cs.Submitted, 0) AS Count
                        FROM submittedCount cs
                        UNION ALL
                        SELECT 'InProgress' AS Category, COALESCE(t.InProgress, 0) AS Count
                        FROM InProgressCount t
                        UNION ALL
                        SELECT 'NotStarted' AS Category, COALESCE(v.NotStarted, 0) AS Count
                        FROM NotStartedCount v;";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        oCrosswalkChartData = new ChartDataSubmissionStatus();
                        oCrosswalkChartData.Category = row["Category"].ToString();
                        oCrosswalkChartData.Count = int.Parse(row["Count"].ToString());
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }

        [Route("api/Crosswalk/fetchCrosswalkCompletionStatus/")]
        [HttpGet]
        public List<ChartDataCompletionStatus> fetchCrosswalkCompletionStatus()
        {

            ChartDataCompletionStatus oCrosswalkChartData;
            List<ChartDataCompletionStatus> lstCrosswalkChartData = new List<ChartDataCompletionStatus>();

            var connectionString = "";


            var SQLCommandText = @" WITH PrincPlacedCount AS(
                           SELECT COUNT(Employee) AS PrinciplePlaced
                            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT
                            WHERE NES = 'NES'
                        ),
                        NonPlacedCount AS(
                            SELECT COUNT(a.Position) AS NonPlaced
                            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                            LEFT JOIN CrossWalk b ON a.[Position_Name] = b.Position
                            WHERE LEN([Org_Unit_Name]) > 2
                              AND NES = 'NES'
                              AND a.Employee NOT IN(
                                  SELECT b.EmployeeID
                                  FROM CrossWalk b
                                  WHERE b.EmployeeID IS NOT NULL
                              )
                              AND LEN(a.Employee) > 1
                        ),
                          EligiblePlacedCount AS(
                          SELECT 300 AS EligiblePlaced
                        ),
						NonEligiblePlacedCount AS(
                          SELECT 250 AS NonEligiblePlaced
                        )
                        SELECT
                            pp.PrinciplePlaced, 
                            np.NonPlaced,
	                        ep.EligiblePlaced,
							nep.NonEligiblePlaced
                        FROM
                            PrincPlacedCount pp,
                            NonPlacedCount np,
	                        EligiblePlacedCount ep,
							NonEligiblePlacedCount nep
                       ";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        oCrosswalkChartData = new ChartDataCompletionStatus();
                        oCrosswalkChartData.PRINCIPLEPLACED = int.Parse(row["PrinciplePlaced"].ToString());
                        oCrosswalkChartData.NONPLACED = int.Parse(row["NonPlaced"].ToString());
                        oCrosswalkChartData.ELIGIBLEPLACED = int.Parse(row["EligiblePlaced"].ToString());
                        oCrosswalkChartData.NONELIGIBLEPLACED = int.Parse(row["NonEligiblePlaced"].ToString());
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }
        [Route("api/Crosswalk/fetchCrosswalkChartDataNESEligibility/")]
        [HttpGet]
        public List<ChartDataNESEligibility> fetchCrosswalkChartDataNESEligibility()
        {

            ChartDataNESEligibility oCrosswalkChartData;
            List<ChartDataNESEligibility> lstCrosswalkChartData = new List<ChartDataNESEligibility>();

            var connectionString = "";


            var SQLCommandText = @"WITH EligibleNICount AS(
                           SELECT COUNT(Employee) AS EligibleNoIdentified
                            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT
                            WHERE NES = 'NES'
                        ),
                        PrincipleCount AS(
                            SELECT COUNT(a.Position) AS Principle
                            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                            LEFT JOIN CrossWalk b ON a.[Position_Name] = b.Position
                            WHERE LEN([Org_Unit_Name]) > 2
                              AND NES = 'NES'
                              AND a.Employee NOT IN(
                                  SELECT b.EmployeeID
                                  FROM CrossWalk b
                                  WHERE b.EmployeeID IS NOT NULL
                              )
                              AND LEN(a.Employee) > 1
                        ),
                          EligibleCount AS(
                          SELECT 300 AS Eligible
                        ),
						NonEligibleCount AS(
                          SELECT 250 AS NonEligible
                        )
                        SELECT
                            eni.EligibleNoIdentified, 
                            pc.Principle,
	                        ec.Eligible,
							ne.NonEligible
                        FROM
                            EligibleNICount eni,
                            PrincipleCount pc,
	                        EligibleCount ec,
							NonEligibleCount ne";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        oCrosswalkChartData = new ChartDataNESEligibility();
                        oCrosswalkChartData.EligibleNoIdentified = int.Parse(row["EligibleNoIdentified"].ToString());
                        oCrosswalkChartData.Principle = int.Parse(row["Principle"].ToString());
                        oCrosswalkChartData.Eligible = int.Parse(row["Eligible"].ToString());
                        oCrosswalkChartData.NonEligible = int.Parse(row["NonEligible"].ToString());
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }



        [Route("api/Crosswalk/fetchCrosswalkChartData/")]
        [HttpGet]
        public List<CrossWalkChartData> fetchCrosswalkChartData()
        {

            CrossWalkChartData oCrosswalkChartData;
            List<CrossWalkChartData> lstCrosswalkChartData = new List<CrossWalkChartData>();

            var connectionString = "";

            var SQLCommandText = @"SELECT count(CRecordID) as NumCrosswalked,[SchoolName] from CrossWalk
                                  group by 
                                  [SchoolName]";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    //cmd.Parameters.AddWithValue("@SchoolName", SchoolName);
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oCrosswalkChartData = new CrossWalkChartData();
                        oCrosswalkChartData.NumCrosswalked = int.Parse(row["NumCrosswalked"].ToString());
                        oCrosswalkChartData.SchoolName = row["SchoolName"].ToString();
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }

        [Route("api/Crosswalk/fetchCrosswalkChartDataByDivisionAndUnit/{area}")]
        [HttpGet]
        public List<CrossWalkChartData> fetchCrosswalkChartDataByDivisionAndUnit(string area)
        {

            string Division = "'" + area.Split('|')[0].ToString() + "'";
            string Unit = "'" + area.Split('|')[1].ToString() + "'";

            CrossWalkChartData oCrosswalkChartData;
            List<CrossWalkChartData> lstCrosswalkChartData = new List<CrossWalkChartData>();

            var connectionString = "";

            var SQLCommandText = $"SELECT count(a.CRecordID) as NumCrosswalked,a.SchoolName,b.Division,b.Unit from CrossWalk a " +
                                 $"LEFT JOIN [YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] b on a.EmployeeID = b.Employee " +
                                 $"WHERE b.Division = {Division} AND b.Unit = {Unit} group by a.SchoolName,b.Division,b.Unit";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    //cmd.Parameters.AddWithValue("@Division", Division);
                    //cmd.Parameters.AddWithValue("@Unit", Unit);
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oCrosswalkChartData = new CrossWalkChartData();
                        oCrosswalkChartData.NumCrosswalked = int.Parse(row["NumCrosswalked"].ToString());
                        oCrosswalkChartData.SchoolName = row["SchoolName"].ToString();
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }

        [Route("api/Crosswalk/fetchCrosswalkMessageData/")]
        [HttpGet]
        public String fetchCrosswalkMessageData()
        {

            CrossWalkChartData oCrosswalkChartData;
            List<CrossWalkChartData> lstCrosswalkChartData = new List<CrossWalkChartData>();

            var connectionString = "";

            var SQLCommandText = @"SELECT Message from tblMessage";
            string Message = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    //cmd.Parameters.AddWithValue("@SchoolName", SchoolName);
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Message  = row["Message"].ToString();
                      
                    }
                }
            }

            return Message;
        }

        [HttpPost]
        [Route("api/Crosswalk/UpdateMessageData/")]
        public bool UpdateMessageData([FromBody] tblMessage oMessageData)
        {
            bool bSuccess = false;


            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":
                    //use local as the same for connection string while in test
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }

            const string sql2 = @"UPDATE tblMessage SET Message= @Message,Type = 'Standard'";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@Message", oMessageData.Message);
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

        [Route("api/Crosswalk/fetchSubmissionData/")]
        [HttpGet]
        public List<SubmissionStatus> fetchSubmissionData()
        {

            SubmissionStatus oSubmissionStatus;
            List<SubmissionStatus> lstSubmissionStatus = new List<SubmissionStatus>();

            var connectionString = "";

                  var SQLCommandText = @"SELECT Max(a.[Org_Unit_Name]) as SchoolName,
                  Status = CASE WHEN max(b.CRecordID) IS NULL THEN 'NOT STARTED'
				                WHEN max(a.[Org_Unit_Name]) = max(c.[Org_Unit_Name]) THEN 'SUBMITTED' 
								 ELSE 'IN PROGRESS' 
								END,
				  ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS fake_id
                  FROM [dbo].[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                  left join CrossWalk b on a.Employee = b.EmployeeID
                  --left join AknowledgementTable c on a.Employee = c.[Employee]
				  left join AknowledgementTable c on a.[Org_Unit_Name] = c.[Org_Unit_Name]
                  WHERE a.[Org_Unit_Name] IS NOT NULL AND [NES] = 'NES'
                  group by a.Org_Unit_Name 
                  order by a.Org_Unit_Name asc";

           
            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

       
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
                        oSubmissionStatus = new SubmissionStatus();
                        oSubmissionStatus.SchoolName = row["SchoolName"].ToString();
                        oSubmissionStatus.Status = row["Status"].ToString();
                        oSubmissionStatus.fake_id = row["fake_id"].ToString();
                        lstSubmissionStatus.Add(oSubmissionStatus);
                    }
                }
            }

            return lstSubmissionStatus;
        }

        [Route("api/Crosswalk/fetchSubmissionDataBySchoolName/{SchoolName}")]
        [HttpGet]
        public List<SubmissionStatus> fetchSubmissionDataBySchoolName(string SchoolName)
        {

            SubmissionStatus oSubmissionStatus;
            List<SubmissionStatus> lstSubmissionStatus = new List<SubmissionStatus>();

            var connectionString = "";

            var SQLCommandText = @"SELECT Max(a.[Org_Unit_Name]) as SchoolName,
                  Status = CASE WHEN max(b.CRecordID) IS NULL THEN 'NOT STARTED'
				                WHEN max(a.[Org_Unit_Name]) = max(c.[Org_Unit_Name]) THEN 'SUBMITTED' 
								 ELSE 'IN PROGRESS' 
								END,
				  ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS fake_id
                  FROM [dbo].[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                  left join CrossWalk b on a.Employee = b.EmployeeID
				  left join AknowledgementTable c on a.[Org_Unit_Name] = c.[Org_Unit_Name]
                  WHERE  a.Org_Unit_Name = @SchoolName
                  group by a.Org_Unit_Name 
                  order by a.Org_Unit_Name asc";


            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }


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
                        oSubmissionStatus = new SubmissionStatus();
                        oSubmissionStatus.SchoolName = row["SchoolName"].ToString();
                        oSubmissionStatus.Status = row["Status"].ToString();
                        oSubmissionStatus.fake_id = row["fake_id"].ToString();
                        lstSubmissionStatus.Add(oSubmissionStatus);
                    }
                }
            }

            return lstSubmissionStatus;
        }

        [Route("api/Crosswalk/fetchDataMetricsCompletionStatus/")]
        [HttpGet]
        public List<ChartDataCompletionStatus> fetchDataMetricsCompletionStatus()
        {

            ChartDataCompletionStatus oCrosswalkChartData;
            List<ChartDataCompletionStatus> lstCrosswalkChartData = new List<ChartDataCompletionStatus>();

            var connectionString = "";


            var SQLCommandText = @"WITH NotPlaced AS(
                             SELECT COUNT(a.Employee) AS NotPlaced
                            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							WHERE a.Employee NOT IN (SELECT b.EmployeeID FROM CrossWalk b)
							AND NES = 'NES'
                        ),
                        PrinciplePlacedCount AS(
						  SELECT COUNT(a.Employee) AS PrinciplePlaced
                            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							WHERE a.Employee IN (SELECT b.EmployeeID FROM CrossWalk b)
							AND NES = 'NES'
                        ),
                        EligiblePlacedCount AS(
						  SELECT COUNT(a.Employee) AS EligiblePlaced
                            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							WHERE a.Employee IN (SELECT b.EmployeeID FROM EligibilityTable b)
							AND NES = 'NES'
                        ),
						 NotEligiblePlacedCount AS(
						  SELECT COUNT(a.Employee) AS NotEligiblePlaced
                            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							WHERE a.Employee NOT IN (SELECT b.EmployeeID FROM EligibilityTable b)
							AND NES = 'NES'
                        )
                        SELECT
                            np.NotPlaced, 
                            pp.PrinciplePlaced,
	                        ep.EligiblePlaced,
							ne.NotEligiblePlaced
                        FROM
                            NotPlaced np,
                            PrinciplePlacedCount pp,
	                        EligiblePlacedCount ep,
							NotEligiblePlacedCount ne";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        oCrosswalkChartData = new ChartDataCompletionStatus();
                        oCrosswalkChartData.NONPLACED = int.Parse(row["NotPlaced"].ToString());
                        oCrosswalkChartData.PRINCIPLEPLACED = int.Parse(row["PrinciplePlaced"].ToString());
                        oCrosswalkChartData.ELIGIBLEPLACED = int.Parse(row["EligiblePlaced"].ToString());
                        oCrosswalkChartData.NONELIGIBLEPLACED = int.Parse(row["NotEligiblePlaced"].ToString());
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }



        [Route("api/Crosswalk/fetchEligibilityChartData/")]
        [HttpGet]
        public List<NESEligibilityForChartData> fetchEligibilityChartData()
        {

            NESEligibilityForChartData oCrosswalkChartData;
            List<NESEligibilityForChartData> lstCrosswalkChartData = new List<NESEligibilityForChartData>();

            var connectionString = "";

            var SQLCommandText = @"SELECT max(Eligibility) As Status,count(a.EmployeeID) As Count
                                   FROM EligibilityTable a
                                   group by 
                                   Eligibility
                                    UNION ALL
                                    SELECT 
                                        'Eligible - Not Defined' AS Status, 
                                        COUNT(a.Employee) AS Count
                                    FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                                    LEFT JOIN EligibilityTable b ON a.Employee = b.EmployeeID
                                    WHERE b.Eligibility IS NULL;
    ";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        oCrosswalkChartData = new NESEligibilityForChartData();
                        oCrosswalkChartData.Status = row["Status"].ToString();
                        oCrosswalkChartData.Count = int.Parse(row["Count"].ToString());
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }

        [Route("api/Crosswalk/fetchDataEligibilityBySchool/{SchoolName}")]
        [HttpGet]
        public List<NESEligibilityForChartData> fetchDataEligibilityBySchool(string SchoolName)
        {

            NESEligibilityForChartData oCrosswalkChartData;
            List<NESEligibilityForChartData> lstCrosswalkChartData = new List<NESEligibilityForChartData>();

            var connectionString = "";

            var SQLCommandText = @"SELECT max(Eligibility) As Status,count(a.EmployeeID) As Count
                                   FROM EligibilityTable a
								   INNER JOIN YPBI_HPAOS_YPAOS_AUTH_POS_REPORT b ON a.EmployeeID = b.Employee
								    WHERE b.[Org_Unit_Name] = @SchoolName
                                   group by 
                                   Eligibility;
    ";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        oCrosswalkChartData = new NESEligibilityForChartData();
                        oCrosswalkChartData.Status = row["Status"].ToString();
                        oCrosswalkChartData.Count = int.Parse(row["Count"].ToString());
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }

        [Route("api/Crosswalk/fetchDoughnutEligibilityCWCompletionStatusChartData/")]
        [HttpGet]
        public List<NESEligibilityForChartData> fetchDoughnutEligibilityCWCompletionStatusChartData()
        {

            NESEligibilityForChartData oCrosswalkChartData;
            List<NESEligibilityForChartData> lstCrosswalkChartData = new List<NESEligibilityForChartData>();

            var connectionString = "";

            var SQLCommandText = @"SELECT max(Eligibility) As Status,count(a.EmployeeID) As Count
                   FROM EligibilityTable a
	               INNER JOIN CrossWalk b on a.[EmployeeID] = b.EmployeeID
                   group by 
                   Eligibility
            UNION ALL
            SELECT 
                'Eligible - Not Defined' AS Status, 
                COUNT(a.Employee) AS Count
            FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
            LEFT JOIN EligibilityTable b ON a.Employee = b.EmployeeID
            INNER JOIN CrossWalk c on a.Employee = c.EmployeeID
            WHERE 
            b.Eligibility IS NULL;";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        oCrosswalkChartData = new NESEligibilityForChartData();
                        oCrosswalkChartData.Status = row["Status"].ToString();
                        oCrosswalkChartData.Count = int.Parse(row["Count"].ToString());
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }

        [Route("api/Crosswalk/fetchDoughnutEligibilityCWCompletionStatusChartDataBySchool/{SchoolName}")]
        [HttpGet]
        public List<NESEligibilityForChartData> fetchDoughnutEligibilityCWCompletionStatusChartDataBySchool(string SchoolName)
        {

            NESEligibilityForChartData oCrosswalkChartData;
            List<NESEligibilityForChartData> lstCrosswalkChartData = new List<NESEligibilityForChartData>();

            var connectionString = "";

            var SQLCommandText = @"   SELECT max(Eligibility) As Status,count(a.EmployeeID) As Count
            FROM EligibilityTable a
	        INNER JOIN CrossWalk b on a.[EmployeeID] = b.EmployeeID
			 WHERE a.[SchoolName] = @SchoolName
            group by 
            Eligibility
          ";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        oCrosswalkChartData = new NESEligibilityForChartData();
                        oCrosswalkChartData.Status = row["Status"].ToString();
                        oCrosswalkChartData.Count = int.Parse(row["Count"].ToString());
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }


        [Route("api/Crosswalk/fetchCrosswalkPostionStatusChartData/")]
        [HttpGet]
        public List<CrossWalkPostionStatusChartData> fetchCrosswalkPostionStatusChartData()
        {

            CrossWalkPostionStatusChartData oCrosswalkChartData;
            List<CrossWalkPostionStatusChartData> lstCrosswalkChartData = new List<CrossWalkPostionStatusChartData>();

            var connectionString = "";

            var SQLCommandText = @"  WITH 
                                    CurrentStaffCount AS (
                                        SELECT COUNT(a.Employee) AS [Current Staff]
                                        FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
										JOIN CrossWalk b on a.Employee = b.[EmployeeID]
                                        WHERE a.NES = 'NES' AND a.[Org_Unit_Name] = b.[SchoolName]
                                    ),
                                    TransfersCount AS (
										SELECT COUNT(a.Employee) AS Transfers
                                        FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
										JOIN CrossWalk b on a.Employee = b.[EmployeeID]
                                        WHERE a.NES = 'NES' AND a.[Org_Unit_Name] != b.[SchoolName]
                                    ),
                                    VacantCount AS (
                                        SELECT COUNT(a.Employee) AS Vacant
                                        FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                                        WHERE a.Employee NOT IN (SELECT b.EmployeeID FROM CrossWalk b)
                                        AND a.NES = 'NES'
                                    )
                                    SELECT 'Current Staff' AS Category, COALESCE(cs.[Current Staff], 0) AS Count
                                    FROM CurrentStaffCount cs
                                    UNION ALL
                                    SELECT 'Transfers' AS Category, COALESCE(t.Transfers, 0) AS Count
                                    FROM TransfersCount t
                                    UNION ALL
                                    SELECT 'Vacant' AS Category, COALESCE(v.Vacant, 0) AS Count
                                    FROM VacantCount v;
                                    ";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        oCrosswalkChartData = new CrossWalkPostionStatusChartData();
                        oCrosswalkChartData.Category = row["Category"].ToString();
                        oCrosswalkChartData.Count = int.Parse(row["Count"].ToString());
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }



        [Route("api/Crosswalk/fetchCWPositonStatusBySchool/{SchoolName}")]
        [HttpGet]
        public List<CrossWalkPostionStatusChartData> fetchCWPositonStatusBySchool(string SchoolName)
        {

            CrossWalkPostionStatusChartData oCrosswalkChartData;
            List<CrossWalkPostionStatusChartData> lstCrosswalkChartData = new List<CrossWalkPostionStatusChartData>();

            var connectionString = "";

            var SQLCommandText = @"WITH 
                                    CurrentStaffCount AS (
                                        SELECT COUNT(a.Employee) AS [Current Staff]
                                        FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
										JOIN CrossWalk b on a.Employee = b.[EmployeeID]
                                        WHERE a.[Org_Unit_Name] = @SchoolName
                                    ),
                                    TransfersCount AS (
											SELECT COUNT(a.Employee) AS Transfers
                                        FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
										JOIN CrossWalk b on a.Employee = b.[EmployeeID]
                                        WHERE 
										a.NES = 'NES' 
										AND a.[Org_Unit_Name] != b.[SchoolName]
										AND
										a.[Org_Unit_Name] = @SchoolName
                                    ),
                                    VacantCount AS (
                                        SELECT COUNT(a.Employee) AS Vacant
                                        FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
                                        WHERE a.Employee NOT IN (SELECT b.EmployeeID FROM CrossWalk b)
                                        AND a.[Org_Unit_Name] = @SchoolName
                                    )
                                    SELECT 'Current Staff' AS Category, COALESCE(cs.[Current Staff], 0) AS Count
                                    FROM CurrentStaffCount cs
                                    UNION ALL
                                    SELECT 'Transfers' AS Category, COALESCE(t.Transfers, 0) AS Count
                                    FROM TransfersCount t
									UNION ALL
                                    SELECT 'Vacant' AS Category, COALESCE(v.Vacant, 0) AS Count
                                    FROM VacantCount v;
                                    ";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        oCrosswalkChartData = new CrossWalkPostionStatusChartData();
                        oCrosswalkChartData.Category = row["Category"].ToString();
                        oCrosswalkChartData.Count = int.Parse(row["Count"].ToString());
                        lstCrosswalkChartData.Add(oCrosswalkChartData);
                    }
                }
            }

            return lstCrosswalkChartData;
        }
        /*END  CHARTING DATA   */
        #endregion Charting

        #region Acknowledgement
        /*Aknowledgment  */


        /*THIS IS THE METHOD THAT HAS THE LOGIC THAT FETCHES COUNTS BEFORE PROCEDEING TO ADKNOWLEDGEMENT */
        [Route("api/Crosswalk/fetchNextStepComplete/{SchoolName}")]
        [HttpGet]
        public bool fetchNextStepComplete(string SchoolName)
        {

            bool bReadyForAcknowledge = false;
            var NotesCount = 0;
            var NextStepsCount = 0;
            var NotCrosswalkedCount = 0;
            var NotCrosswalkedCountExtraCheck = 0;

            NotCrosswalkedCount = fetchEmployeeNotCrossWalkedCount(SchoolName);
            NotCrosswalkedCountExtraCheck = fetchEmployeeNotesNotCrosswalkedCountExtraCheck(SchoolName);
            NotesCount = fetchEmployeeNotesNotCrosswalkedCount(SchoolName);
            NextStepsCount = fetchEmployeeNextStepsCount(SchoolName);
            NotCrosswalkedCountExtraCheck = fetchEmployeeNotesNotCrosswalkedCountExtraCheck(SchoolName);

            /*IF NOTES AND NEXTSTEP COUNTS ARE EQUAL */
            if (NotesCount >= NotCrosswalkedCount &&
                NextStepsCount >= NotCrosswalkedCount &&
                NotCrosswalkedCountExtraCheck == NotCrosswalkedCount)
            {
                bReadyForAcknowledge = true;
            }
            else
            {
                bReadyForAcknowledge = false;
            }
            

            /*
            var connectionString = "";

            var SQLCommandText = @"SELECT count(*) As NotCrsWlkedVSNextSetp  --zero means that the record count does not match
				               FROM 
				               (
					                     (SELECT x.TotalEmployees,y.TotalEmployeesWithNextStepRecord
							              FROM 
							              (SELECT count(a.Employee) As TotalEmployees
							              FROM  [YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
							              WHERE
							              a.Employee NOT IN (SELECT b.EmployeeID FROM CrossWalk b) 
							              AND
							              a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1) x ,
							              (SELECT count(a.EmployeeID) As TotalEmployeesWithNextStepRecord FROM  EmployeeNotesNextStep a
							              WHERE
							              LEN(a.NextSteps) > 2 
							              AND
							              a.SchoolName = @SchoolName )y
							              WHERE 
							              x.TotalEmployees = y.TotalEmployeesWithNextStepRecord)
				              )z";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int recordCount = 0;
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
                        recordCount = int.Parse(row["NotCrsWlkedVSNextSetp"].ToString());
                    }
                }
            }
            */

            return bReadyForAcknowledge;
        }

        /*COUNTS BELOW */
        [Route("api/Crosswalk/fetchEmployeeNextStepsCount/{SchoolNamee}")]
        [HttpGet]
        public int fetchEmployeeNextStepsCount(string SchoolName)
        {


            var connectionString = "";

            var SQLCommandText = @"SELECT count(EmployeeID) AS employeeCount FROM EmployeeNextSteps WHERE SchoolName = @SchoolName";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int _employeeCount = 0;
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

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            _employeeCount = int.Parse(row["employeeCount"].ToString());
                        }
                    }
                    else
                    {
                        _employeeCount = 0;
                    }

                }
            }

            return _employeeCount;
        }

        [Route("api/Crosswalk/fetchEmployeeNotesNotCrosswalkedCount/{SchoolName}")]
        [HttpGet]
        public int fetchEmployeeNotesNotCrosswalkedCount(string SchoolName)
        {


            var connectionString = "";

            var SQLCommandText = @"SELECT count(EmployeeID) AS employeeCount FROM EmployeeNotesNotCrosswalked WHERE SchoolName = @SchoolName";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int _employeeCount = 0;
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

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            _employeeCount = int.Parse(row["employeeCount"].ToString());
                        }
                    }
                    else
                    {
                        _employeeCount = 0;
                    }

                }
            }

            return _employeeCount;
        }

        [Route("api/Crosswalk/fetchEmployeeNotesNotCrosswalkedCount/{SchoolName}")]
        [HttpGet]
        public int fetchEmployeeNotCrossWalkedCount(string SchoolName)
        {


            var connectionString = "";

            var SQLCommandText = @"SELECT count(a.Employee) as NotCrossWalkedCount,
                                    max(a.Org_Unit_Name) as SchoolName
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    WHERE 
                                    a.Employee NOT IN (SELECT b.EmployeeID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                                    AND
                                    a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int _employeeCount = 0;
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

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            _employeeCount = int.Parse(row["NotCrossWalkedCount"].ToString());
                        }
                    }
                    else
                    {
                        _employeeCount = 0;
                    }

                }
            }

            return _employeeCount;
        }

        [Route("api/Crosswalk/fetchEmployeeNotesNotCrosswalkedCountExtraCheck/{SchoolName}")]
        [HttpGet]
        public int fetchEmployeeNotesNotCrosswalkedCountExtraCheck(string SchoolName)
        {


            var connectionString = "";

            /*
            var SQLCommandText = @"SELECT count(a.Employee) as NotCrossWalkedCount,
                                    max(a.Org_Unit_Name) as SchoolName
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    WHERE 
                                    a.Employee NOT IN (SELECT b.EmployeeID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                                    AND
                                    a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1";
            */

            var SQLCommandText = @"SELECT count(a.Employee)  as NotCrossWalkedCount,
                                   max(a.Org_Unit_Name) as SchoolName
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    LEFT JOIN EmployeeNotesNotCrosswalked d on a.Employee = d.EmployeeID
                                    LEFT JOIN EmployeeNextSteps e on a.Employee = e.EmployeeID
                                    WHERE 
                                    a.Employee NOT IN (SELECT b.EmployeeID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                                    AND
                                    a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1 
									AND
									d.Notes IS NOT NULL 
									AND e.NextStep  IS NOT NULL ";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            int _employeeCount = 0;
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

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            _employeeCount = int.Parse(row["NotCrossWalkedCount"].ToString());
                        }
                    }
                    else
                    {
                        _employeeCount = 0;
                    }

                }
            }

            return _employeeCount;
        }

        /*END COUNTS BELOW */

        [HttpPost]
        [Route("api/Crosswalk/AddAknowledgementData/")]
        public bool AddAknowledgementData([FromBody] AknowledgementTable oAcknowledgmentData)
        {
            bool bSuccess = false;


            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":
                    //use local as the same for connection string while in test
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }

            const string sql2 = @"
                                IF NOT EXISTS(SELECT 1 FROM AknowledgementTable WHERE Employee = @Employee)
                                    BEGIN
                                        INSERT INTO AknowledgementTable(
                                           Employee,Org_Unit_Name,CompletionDate)
                                        VALUES(
                                          @Employee,@Org_Unit_Name,@CompletionDate)
                                    END
                                ELSE
                                    BEGIN
                                       UPDATE AknowledgementTable SET 
                                       Employee= @Employee,
                                       Org_Unit_Name= @Org_Unit_Name,
                                       CompletionDate= @CompletionDate
                                       WHERE Employee = @Employee 
                                    END";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@Employee", oAcknowledgmentData.Employee);
                    cmd.Parameters.AddWithValue("@Org_Unit_Name", oAcknowledgmentData.Org_Unit_Name);
                    cmd.Parameters.AddWithValue("@CompletionDate", oAcknowledgmentData.CompletionDate);
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

        [Route("api/Crosswalk/fetchAknowledgementEmployee/{Employee}")]
        [HttpGet]
        public string fetchAknowledgementEmployee(int Employee)
        {


            var connectionString = "";

            var SQLCommandText = @"SELECT Employee FROM AknowledgementTable WHERE Employee = @Employee";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            string _employee = "";
            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@Employee", Employee);
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            _employee = row["Employee"].ToString();
                        }
                    }
                    else
                    {
                        _employee = "";
                    }
                   
                }
            }

            return _employee;
        }

        [Route("api/Crosswalk/DeleteAknowledgementEmployee/{Employee}")]
        [HttpGet]
        public bool DeleteAknowledgementEmployee(int Employee)
        {

            bool bSuccess = false;
            var connectionString = "";

            var SQLCommandText = @"DELETE AknowledgementTable WHERE Employee = @Employee";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@Employee", Employee);
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

        [Route("api/Crosswalk/checkAcknowledgmentDateRange/{currentDate}")]
        [HttpGet]
        public bool checkAcknowledgmentDateRange(string currentDate)
        {

            DateTime _date = DateTime.Parse(currentDate);
        

            int currentYear = DateTime.Now.Year;
            bool ValidDateRange = false;
            DateTime startDate;
            DateTime endDate;

            AppDateRange oDateRange = new AppDateRange();
            oDateRange = fetcApplicationDateRangeByYearAndType(currentYear.ToString(), "ACKNOWLEDGEMT");
            startDate = oDateRange.StartDate;
            endDate = oDateRange.EndDate;


            if (_date >= startDate && _date <= endDate)
            {
                ValidDateRange = true;
            }
            else
            {
                ValidDateRange = false;
            }


            return ValidDateRange;
        }

        /*End Aknowledgment   */
        #endregion Acknowledgement

        #region ApplicationDateRange
        /*Application Date Range */
        [HttpPost]
        [Route("api/Crosswalk/AddApplicationDateRangeParameters")]
        public bool AddApplicationDateRangeParameters([FromBody] AppDateRange oAppDateRange)
        {
            bool bSuccess = false;


            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":

                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }

            string sql2 = "";

            sql2 = @"IF NOT EXISTS(SELECT 1 FROM ApplicationDateRangeParameters WHERE Year = @Year AND Type = @Type )
                                    BEGIN
                                      INSERT INTO ApplicationDateRangeParameters (
                                            StartDate,
                                            EndDate,
                                            Year,
                                            Type)
                                        VALUES(
                                            @StartDate,
                                            @EndDate,
                                            @Year,
                                            @Type)
                                    END
                               ";




            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@StartDate", oAppDateRange.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", oAppDateRange.EndDate);
                    cmd.Parameters.AddWithValue("@Year", oAppDateRange.Year);
                    cmd.Parameters.AddWithValue("@Type", oAppDateRange.Type);


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

        [HttpPost]
        [Route("api/Crosswalk/EditApplicationDateRangeParameters")]
        public bool EditApplicationDateRangeParameters([FromBody] AppDateRange oAppDateRange)
        {
            bool bSuccess = false;


            var connectionString = "";

            switch (s_Environment)
            {
                case "PROD":

                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }

            string sql2 = "";

            sql2 = @"BEGIN
                           UPDATE ApplicationDateRangeParameters
                            SET 
                                StartDate = @StartDate,
                                EndDate = @EndDate,
                                Year = @Year,
                                Type = @Type
                            WHERE rowID = @rowID;
                                    END";




            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@StartDate", oAppDateRange.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", oAppDateRange.EndDate);
                    cmd.Parameters.AddWithValue("@Year", oAppDateRange.Year);
                    cmd.Parameters.AddWithValue("@Type", oAppDateRange.Type);
                    cmd.Parameters.AddWithValue("@rowID", oAppDateRange.rowID);


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

        [Route("api/Crosswalk/fetcApplicationDateRange/{year}")]
        [HttpGet]
        public List<AppDateRange> fetcApplicationDateRange(string year)
        {

            AppDateRange oAppDateRange;
            List<AppDateRange> lstDateRangeData = new List<AppDateRange>();

            var connectionString = "";

            var SQLCommandText = @"select StartDate,EndDate,Year from ApplicationDateRangeParameters WHERE Year = @year";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    //cmd.Parameters.Add(year);
                    cmd.Parameters.AddWithValue("@year", year);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oAppDateRange = new AppDateRange();
                        oAppDateRange.StartDate  = DateTime.Parse(row["StartDate"].ToString());
                        oAppDateRange.EndDate = DateTime.Parse(row["EndDate"].ToString());
                        oAppDateRange.Year = row["Year"].ToString();
                        lstDateRangeData.Add(oAppDateRange);

                    }
                }
            }

            return lstDateRangeData;
        }

        [Route("api/Crosswalk/fetchAllApplicationDateRanges/")]
        [HttpGet]
        public List<AppDateRange> fetchAllApplicationDateRanges()
          {

            AppDateRange oAppDateRange;
            List<AppDateRange> lstDateRangeData = new List<AppDateRange>();

            var connectionString = "";

            var SQLCommandText = @"select rowID,StartDate,EndDate,Year,Type,ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS fake_id from ApplicationDateRangeParameters";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

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
                        oAppDateRange = new AppDateRange();
                        oAppDateRange.StartDate = DateTime.Parse(row["StartDate"].ToString());
                        oAppDateRange.EndDate = DateTime.Parse(row["EndDate"].ToString());
                        oAppDateRange.Year = row["Year"].ToString();
                        oAppDateRange.rowID = int.Parse(row["rowID"].ToString());
                        oAppDateRange.Type = row["Type"].ToString();
                        oAppDateRange.fake_id = int.Parse(row["fake_id"].ToString());
                        lstDateRangeData.Add(oAppDateRange);

                    }
                }
            }

            return lstDateRangeData;
        }

        [Route("api/Crosswalk/DeleteAppDateRanges/{rowID}")]
        [HttpGet]
        public bool DeleteAppDateRanges(int rowID)
        {

            bool bSuccess = false;
            var connectionString = "";

            var SQLCommandText = @"DELETE ApplicationDateRangeParameters WHERE rowID = @rowID";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@rowID", rowID);
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

        public AppDateRange fetcApplicationDateRangeByYearAndType(string year,string type)
        {

            AppDateRange oAppDateRange;
            

            var connectionString = "";

            var SQLCommandText = @"select StartDate,EndDate,Year from ApplicationDateRangeParameters WHERE year = @year AND type = @type";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            oAppDateRange = new AppDateRange();
            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@year", year);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        
                        oAppDateRange.StartDate = DateTime.Parse(row["StartDate"].ToString());
                        oAppDateRange.EndDate = DateTime.Parse(row["EndDate"].ToString());
                        oAppDateRange.Year = row["Year"].ToString();
                        oAppDateRange.Type = row["Year"].ToString();
                    }
                }
            }

            return oAppDateRange;
        }

        [Route("api/Crosswalk/checkSystemDateRange/{currentDate}")]
        [HttpGet]
        public bool checkSystemDateRange(string currentDate)
        {
      
            DateTime _date = DateTime.Parse(currentDate);
            //Console.WriteLine(_date);

            int currentYear = DateTime.Now.Year;
            bool ValidDateRange = false;
            DateTime startDate;
            DateTime endDate;

            AppDateRange oDateRange = new AppDateRange();
            oDateRange = fetcApplicationDateRangeByYearAndType(currentYear.ToString(), "SYSTEM");
            startDate = oDateRange.StartDate;
            endDate = oDateRange.EndDate;


            if (_date >= startDate && _date <= endDate)
            {
                ValidDateRange = true;
            }
            else
            {
                ValidDateRange = false;
            }


            return ValidDateRange;
        }

        /*END Application Date Range  */
        #endregion ApplicationDateRange


        #region CascadingAndSearchablePostion
        /*FETCH UNIT BASED ON DIVISION - CASCADING DROPDOWNS - AND SEARCHABLE*/
        [Route("api/Crosswalk/fetchUnitsBasedOnDivision/{Division}")]
        [HttpGet]
        public List<UnitsForDivision> fetchUnitsBasedOnDivision(string Division)
        {

            UnitsForDivision oUnits;
            List<UnitsForDivision> lstUnitData = new List<UnitsForDivision>();

            var connectionString = "";

            var SQLCommandText = @"SELECT Unit AS UNIT FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT WHERE NES = 'NES' AND Division = @Division AND UNIT != '#N/A'
                                  GROUP BY UNIT
                                  ORDER BY Unit ASC";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }

            string Unit = "";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    //cmd.Parameters.Add(year);
                    cmd.Parameters.AddWithValue("@Division", Division);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                   
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oUnits = new UnitsForDivision();
                        oUnits.Units =  row["UNIT"].ToString();
                        lstUnitData.Add(oUnits);

                    }
                }
            }

            return lstUnitData;
        }
        [Route("api/Crosswalk/fetchAllPositionsBySchoolNameWOCrosswalk/{SchoolName}")]
        [HttpGet]
        public List<Positions> fetchAllPositionsBySchoolNameWOCrosswalk(string SchoolName)
        {
            Positions oPositions;
            List<Positions> lstPositionData = new List<Positions>();

            var connectionString = "";
            string SQLCommandText = "";


            SQLCommandText = @"SELECT distinct a.Position as PositionNumber,a.[Position_Name] as Position, CONVERT(varchar(20),a.Position) + ' - ' + a.[Position_Name] AS CMBPos,
                              CrossWalked = CASE
		                      WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
							  FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							   LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                              WHERE LEN([Org_Unit_Name]) > 2 AND NES = 'NES'
                              AND
                              [Org_Unit_Name] =  @SchoolName
							  AND
							  a.Position NOT IN (SELECT b.PositionID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                             AND a.KeyDate >= CAST(YEAR(GETDATE()) AS VARCHAR) + '-09-01'
                              order by
                              [Position]";


            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;

                    break;
            }


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
                        oPositions.CrossWalked = row["CrossWalked"].ToString();
                        lstPositionData.Add(oPositions);
                        oPositions = null;
                    }
                }
            }

            return lstPositionData;
        }
        #endregion CascadingAndSearchablePostion


        #region OriginalFakeData
        [Route("api/Crosswalk/fetchSchoolListings/")]
        [HttpGet]
        public List<SchoolListing> fetchSchoolListings()
        {
            SchoolListing oSchoolLisingData;
            List<SchoolListing> lstSchoolistingData = new List<SchoolListing>();

            var connectionString = "";
            string SQLCommandText = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = _connStringDataWarehouse_EDB;
                    SQLCommandText = @"SELECT  max(a.EducationOrgNaturalKey)  EducationOrgNaturalKey, 
                                        a.NameOfInstitution 
	                                    FROM [EDB].[EXT].EducationOrganization a
	                                    group by a.NameOfInstitution
	                                    HAVING LEN(a.NameOfInstitution) > 2 AND max(OrgGrpNaturalKey) = 'Campus' 
                                        ORDER BY a.NameOfInstitution";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT  max(a.SchoolID) as  EducationOrgNaturalKey, 
                                        a.[School Name]  as NameOfInstitution
	                                   FROM SchoolListing a
	                                   group by a.[School Name]
	                                   HAVING LEN(a.[School Name]) > 0
                                       ORDER BY a.[School Name]";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT  max(a.SchoolID) as  EducationOrgNaturalKey, 
                                        a.[School Name]  as NameOfInstitution
	                                   FROM SchoolListing a
	                                   group by a.[School Name]
	                                   HAVING LEN(a.[School Name]) > 0
                                       ORDER BY a.[School Name]";
                    break;
            }


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
                        oSchoolLisingData = new SchoolListing();

                        if (row["EducationOrgNaturalKey"] != null && row["EducationOrgNaturalKey"].ToString() != "")
                        {
                            oSchoolLisingData.EducationOrgNaturalKey = row["EducationOrgNaturalKey"].ToString();
                        }
                        else
                        {
                            oSchoolLisingData.EducationOrgNaturalKey = "N/A";
                        }


                        oSchoolLisingData.NameOfInstitution = row["NameOfInstitution"].ToString();
                        lstSchoolistingData.Add(oSchoolLisingData);
                        oSchoolLisingData = null;
                    }
                }
            }

            return lstSchoolistingData;
        }

        [Route("api/Crosswalk/fetchPositions/")]
        [HttpGet]
        public List<Positions> fetchPositions()
        {
            Positions oPositions;
            List<Positions> lstPositionData = new List<Positions>();

            var connectionString = "";
            string SQLCommandText = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT p.Position from Positions p WHERE p.Position NOT IN (SELECT b.Position FROM CrossWalk b WHERE b.Position IS NOT NULL) ORDER BY Position";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT p.Position from Positions p WHERE p.Position NOT IN (SELECT b.Position FROM CrossWalk b WHERE b.Position IS NOT NULL) ORDER BY Position";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT p.Position from Positions p WHERE p.Position NOT IN (SELECT b.Position FROM CrossWalk b WHERE b.Position IS NOT NULL) ORDER BY Position";
                    break;
            }


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
                        oPositions = new Positions();
                        oPositions.Position = row["Position"].ToString();
                        lstPositionData.Add(oPositions);
                        oPositions = null;
                    }
                }
            }

            return lstPositionData;
        }


        [Route("api/Crosswalk/fetchAllPositions/")]
        [HttpGet]
        public List<Positions> fetchAllPositions()
        {
            Positions oPositions;
            List<Positions> lstPositionData = new List<Positions>();

            var connectionString = "";
            string SQLCommandText = "";
            SQLCommandText = @"SELECT p.Position from Positions p ORDER BY Position";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT p.Position from Positions p ORDER BY Position";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT p.Position from Positions p ORDER BY Position";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT p.Position from Positions p ORDER BY Position";
                    break;
            }


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
                        oPositions = new Positions();
                        oPositions.Position = row["Position"].ToString();
                        lstPositionData.Add(oPositions);
                        oPositions = null;
                    }
                }
            }

            return lstPositionData;
        }

        [Route("api/Crosswalk/AssignedPositions/")]
        [HttpGet]
        public List<Positions> AssignedPositions()
        {
            Positions oPositions;
            List<Positions> lstPositionData = new List<Positions>();

            var connectionString = "";
            string SQLCommandText = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT p.Position from Positions p WHERE p.Position  IN (SELECT b.Position FROM CrossWalk b WHERE b.Position IS NOT NULL) ORDER BY Position";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT p.Position from Positions p WHERE p.Position  IN (SELECT b.Position FROM CrossWalk b WHERE b.Position IS NOT NULL) ORDER BY Position";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT p.Position from Positions p WHERE p.Position IN (SELECT b.Position FROM CrossWalk b WHERE b.Position IS NOT NULL) ORDER BY Position";
                    break;
            }


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
                        oPositions = new Positions();
                        oPositions.Position = row["Position"].ToString();
                        lstPositionData.Add(oPositions);
                        oPositions = null;
                    }
                }
            }

            return lstPositionData;
        }

        [Route("api/Crosswalk/fetchCrosswalkEntries/")]
        [HttpGet]
        public List<CrosswalkData> fetchCrosswalkEntries()
        {
            CrosswalkData oCrosswalkData;
            List<CrosswalkData> lstCrosswalkData = new List<CrosswalkData>();

            var connectionString = "";
            string SQLCommandText = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT EmployeeID,Position,SchoolName,DateAdded,CRecordID FROM CrossWalk ORDER BY Position";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT EmployeeID,Position,SchoolName,DateAdded,CRecordID FROM CrossWalk ORDER BY Position";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT EmployeeID,Position,SchoolName,DateAdded,CRecordID FROM CrossWalk ORDER BY Position";
                    break;
            }


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
                        oCrosswalkData = new CrosswalkData();
                        oCrosswalkData.EmployeeID = row["EmployeeID"].ToString();
                        oCrosswalkData.Position = row["Position"].ToString();
                        oCrosswalkData.SchoolName = row["SchoolName"].ToString();
                        oCrosswalkData.DateAdded = DateTime.Parse(row["DateAdded"].ToString());
                        oCrosswalkData.CRecordID = int.Parse(row["CRecordID"].ToString());
                        lstCrosswalkData.Add(oCrosswalkData);
                        oCrosswalkData = null;
                    }
                }
            }

            return lstCrosswalkData;
        }
        
        [Route("api/Crosswalk/fetchCrosswalkEntriesFilteredBySchool/{SchoolName}")]
        [HttpGet]
        public List<CrosswalkData> fetchCrosswalkEntriesFilteredBySchool(string SchoolName)
        {
            CrosswalkData oCrosswalkData;
            List<CrosswalkData> lstCrosswalkData = new List<CrosswalkData>();

            var connectionString = "";
            string SQLCommandText = "";
            SQLCommandText = @"SELECT EmployeeID,Position,SchoolName,DateAdded,CRecordID FROM CrossWalk ";
            SQLCommandText += "WHERE SchoolName = ";
            SQLCommandText += "'";
            SQLCommandText += SchoolName;
            SQLCommandText += "'";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    break;
            }


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
                        oCrosswalkData = new CrosswalkData();
                        oCrosswalkData.EmployeeID = row["EmployeeID"].ToString();
                        oCrosswalkData.Position = row["Position"].ToString();
                        oCrosswalkData.SchoolName = row["SchoolName"].ToString();
                        oCrosswalkData.DateAdded = DateTime.Parse(row["DateAdded"].ToString());
                        oCrosswalkData.CRecordID = int.Parse(row["CRecordID"].ToString());
                        lstCrosswalkData.Add(oCrosswalkData);
                        oCrosswalkData = null;
                    }
                }
            }

            return lstCrosswalkData;
        }
        
        [Route("api/Crosswalk/fetchCrosswalkEntriesAllParameters")]
        [HttpPost]
        public List<CrosswalkData> fetchCrosswalkEntriesAllParameters([FromBody] CrosswalkData oCrosswalkEntryData)
        {
            CrosswalkData oCrosswalkData;
            List<CrosswalkData> lstCrosswalkData = new List<CrosswalkData>();

            var connectionString = "";
            string SQLCommandText = "";
            SQLCommandText = @"SELECT * FROM CrossWalk ";
            SQLCommandText += "WHERE SchoolName = ";
            SQLCommandText += "'";
            SQLCommandText += oCrosswalkEntryData.SchoolName;
            SQLCommandText += "'";
            SQLCommandText += " AND ";
            SQLCommandText += "Position = ";
            SQLCommandText += "'";
            SQLCommandText += oCrosswalkEntryData.Position;
            SQLCommandText += "'";
            SQLCommandText += " AND ";
            SQLCommandText += "EmployeeID = ";
            SQLCommandText += "'";
            SQLCommandText += oCrosswalkEntryData.EmployeeID;
            SQLCommandText += "'";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    break;
            }


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
                        oCrosswalkData = new CrosswalkData();
                        oCrosswalkData.EmployeeID = row["EmployeeID"].ToString();
                        oCrosswalkData.Position = row["Position"].ToString();
                        oCrosswalkData.SchoolName = row["SchoolName"].ToString();
                        oCrosswalkData.DateAdded = DateTime.Parse(row["DateAdded"].ToString());
                        oCrosswalkData.CRecordID = int.Parse(row["CRecordID"].ToString());
                        lstCrosswalkData.Add(oCrosswalkData);
                        oCrosswalkData = null;
                    }
                }
            }

            return lstCrosswalkData;
        }

        [HttpPost]
        [Route("api/Crosswalk/AddEmployees")]
        public void AddEmployees([FromBody] EmployeeTable oEmployeeTableItem)
        {

            var sqlStatement = "";
            var connectionString = "";
            var _SEARCH_STRING = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }

            _SEARCH_STRING += "INSERT INTO EmployeeTable (EmployeeID,SchoolName) ";
            _SEARCH_STRING += " VALUES ";
            _SEARCH_STRING += "(";
            _SEARCH_STRING += "'";
            _SEARCH_STRING += oEmployeeTableItem.EmployeeID;
            _SEARCH_STRING += "'";

            _SEARCH_STRING += ",";
            

            _SEARCH_STRING += "'";
            _SEARCH_STRING += oEmployeeTableItem.SchoolName;
            _SEARCH_STRING += "'";
            _SEARCH_STRING += ")";

            sqlStatement = _SEARCH_STRING;



            // return;

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlStatement, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();

                    da.SelectCommand = cmd;

                    CONN.Open();
                    int nRecsAffected = cmd.ExecuteNonQuery();

                    CONN.Close();
                }
            }


        }


        [Route("api/Crosswalk/fetchEmployeeData/{EmployeeID}")]
        [HttpGet]
        public List<EmployeeTable> fetchEmployeeData(string EmployeeID)
        {
            EmployeeTable oEmployeeTable;
            List<EmployeeTable> lstEmployeeTableData = new List<EmployeeTable>();

            var connectionString = "";
            string SQLCommandText = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE EmployeeID =  ";
                    SQLCommandText += "'";
                    SQLCommandText += EmployeeID;
                    SQLCommandText += "'";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE EmployeeID =  ";
                    SQLCommandText += "'";
                    SQLCommandText += EmployeeID;
                    SQLCommandText += "'";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE EmployeeID =  ";
                    SQLCommandText += "'";
                    SQLCommandText += EmployeeID;
                    SQLCommandText += "'";
                    break;
            }


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
                        oEmployeeTable = new EmployeeTable();
                        oEmployeeTable.EmployeeID = row["EmployeeID"].ToString();
                        oEmployeeTable.SchoolName = row["SchoolName"].ToString();

                        oEmployeeTable.EmployeeName = row["EmployeeName"].ToString();
                        oEmployeeTable.Role = row["Role"].ToString();
                        oEmployeeTable.Certification = row["Certification"].ToString();



                        lstEmployeeTableData.Add(oEmployeeTable);
                        oEmployeeTable = null;
                    }
                }
            }

            return lstEmployeeTableData;
        }

        [Route("api/Crosswalk/fetchEmployeeIDs/")]
        [HttpGet]
        public List<EmployeeIDs> fetchEmployeeIDs()
        {
            EmployeeIDs oEmployeeTable;
            List<EmployeeIDs> lstEmployeeTableData = new List<EmployeeIDs>();

            var connectionString = "";
            string SQLCommandText = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT EmployeeID FROM [EmployeeTable] ORDER BY EmployeeID";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT EmployeeID FROM [EmployeeTable] ORDER BY EmployeeID";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT EmployeeID FROM [EmployeeTable] ORDER BY EmployeeID";
                    break;
            }


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
                        oEmployeeTable = new EmployeeIDs();
                        oEmployeeTable.EmployeeID = row["EmployeeID"].ToString();
                        lstEmployeeTableData.Add(oEmployeeTable);
                        oEmployeeTable = null;
                    }
                }
            }

            return lstEmployeeTableData;
        }

        
        //fetch from Crosswalk Entries  *****
        [Route("api/Crosswalk/fetchEmployeeDetails/{EmployeeID}")]
        [HttpGet]
        public List<CrosswalkData> fetchEmployeeDetails(string SchoolName)
        {
            CrosswalkData oCrosswalkData;
            List<CrosswalkData> lstCrosswalkData = new List<CrosswalkData>();

            var connectionString = "";
            string SQLCommandText = "";
            SQLCommandText = @"SELECT EmployeeID,Position,SchoolName,DateAdded,CRecordID FROM CrossWalk ";
            SQLCommandText += "WHERE SchoolName = ";
            SQLCommandText += "'";
            SQLCommandText += SchoolName;
            SQLCommandText += "'";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    break;
            }


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
                        oCrosswalkData = new CrosswalkData();
                        oCrosswalkData.EmployeeID = row["EmployeeID"].ToString();
                        oCrosswalkData.Position = row["Position"].ToString();
                        oCrosswalkData.SchoolName = row["SchoolName"].ToString();
                        oCrosswalkData.DateAdded = DateTime.Parse(row["DateAdded"].ToString());
                        oCrosswalkData.CRecordID = int.Parse(row["CRecordID"].ToString());
                        lstCrosswalkData.Add(oCrosswalkData);
                        oCrosswalkData = null;
                    }
                }
            }

            return lstCrosswalkData;
        }


        [Route("api/Crosswalk/fetchEmployeeDataBySchoolName/{SchoolName}")]
        [HttpGet]
        public List<EmployeeTable> fetchEmployeeDataBySchoolName(string SchoolName)
        {
            EmployeeTable oEmployeeTable;
            List<EmployeeTable> lstEmployeeTableData = new List<EmployeeTable>();

            var connectionString = "";
            string SQLCommandText = "";

            var SQLCommandTextNew = @"SELECT a.EmployeeID,a.SchoolName,a.EmployeeName,a.Certification,a.Eligibility,a.Role,b.CRecordID,b.Position,
                                CrossWalked = CASE
		                        WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                FROM EmployeeTable a 
	                            LEFT JOIN CrossWalk b on a.EmployeeID = b.EmployeeID
								WHERE a.SchoolName = @SchoolName";


            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT * FROM [EmployeeTable] WHERE SchoolName =  ";
                    SQLCommandText += "'";
                    SQLCommandText += SchoolName;
                    SQLCommandText += "'";
                    break;
            }


            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandTextNew, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@SchoolName", SchoolName);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oEmployeeTable = new EmployeeTable();
                        oEmployeeTable.EmployeeID = row["EmployeeID"].ToString();
                        oEmployeeTable.SchoolName = row["SchoolName"].ToString();

                        oEmployeeTable.EmployeeName = row["EmployeeName"].ToString();
                        oEmployeeTable.Role = row["Role"].ToString();
                        oEmployeeTable.Certification = row["Certification"].ToString();
                        oEmployeeTable.Position = row["Position"].ToString();
                        oEmployeeTable.CrossWalked = row["CrossWalked"].ToString();
                        oEmployeeTable.Eligibility = row["Eligibility"].ToString();



                        lstEmployeeTableData.Add(oEmployeeTable);
                        oEmployeeTable = null;
                    }
                }
            }

            return lstEmployeeTableData;
        }

        #endregion OriginalFakeData


        #region local_db_home
        [HttpPost]
        [Route("api/Crosswalk/AddEmployeesAllColumns")]
        public void AddEmployeesAllColumns([FromBody] EmployeeTable oEmployeeTableItem)
        {

            var sqlStatement = "";
            var connectionString = "";
            var _SEARCH_STRING = "";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }

            _SEARCH_STRING += "INSERT INTO EmployeeTable (EmployeeID,SchoolName,EmployeeName,Certification,Role) ";
            _SEARCH_STRING += " VALUES ";
            _SEARCH_STRING += "(";
            _SEARCH_STRING += "'";
            _SEARCH_STRING += oEmployeeTableItem.EmployeeID;
            _SEARCH_STRING += "'";

            _SEARCH_STRING += ",";


            _SEARCH_STRING += "'";
            _SEARCH_STRING += oEmployeeTableItem.SchoolName;
            _SEARCH_STRING += "'";


            _SEARCH_STRING += ",";


            _SEARCH_STRING += "'";
            _SEARCH_STRING += oEmployeeTableItem.EmployeeName;
            _SEARCH_STRING += "'";


            _SEARCH_STRING += ",";


            _SEARCH_STRING += "'";
            _SEARCH_STRING += oEmployeeTableItem.Certification;
            _SEARCH_STRING += "'";


            _SEARCH_STRING += ",";


            _SEARCH_STRING += "'";
            _SEARCH_STRING += oEmployeeTableItem.Role;
            _SEARCH_STRING += "'";
            _SEARCH_STRING += ")";

            sqlStatement = _SEARCH_STRING;



            // return;

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlStatement, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();

                    da.SelectCommand = cmd;

                    CONN.Open();
                    int nRecsAffected = cmd.ExecuteNonQuery();

                    CONN.Close();
                }
            }


        }

        [Route("api/Crosswalk/fetchSchoolListingsLocalDB/")]
        [HttpGet]
        public List<SchoolListing> fetchSchoolListingsLocalDB()
        {
            SchoolListing oSchoolLisingData;
            List<SchoolListing> lstSchoolistingData = new List<SchoolListing>();

            var connectionString = "";
            string SQLCommandText = "";
            SQLCommandText = @"SELECT   a.EducationOrgNaturalKey, 
                                        a.NameOfInstitution 
	                                    FROM EducationOrganization a ORDER BY a.NameOfInstitution";

            switch (s_Environment)
            {
                case "PROD":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk_Local;

                    break;
            }


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
                        oSchoolLisingData = new SchoolListing();

                        if (row["EducationOrgNaturalKey"] != null && row["EducationOrgNaturalKey"].ToString() != "")
                        {
                            oSchoolLisingData.EducationOrgNaturalKey = row["EducationOrgNaturalKey"].ToString();
                        }
                        else
                        {
                            oSchoolLisingData.EducationOrgNaturalKey = "N/A";
                        }


                        oSchoolLisingData.NameOfInstitution = row["NameOfInstitution"].ToString();
                        lstSchoolistingData.Add(oSchoolLisingData);
                        oSchoolLisingData = null;
                    }
                }
            }

            return lstSchoolistingData;
        }


        #endregion local_db_home


    }


}
