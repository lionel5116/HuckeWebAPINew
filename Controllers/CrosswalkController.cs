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


        #region UsingRealDataSchema
        /*PULLING FROM NEW SCHEMA - REAL DATA - EXCLUDES ANY POSITIONS THAT EXIST IN CROSSWALK TALBE*/
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
							  LEFT JOIN CrossWalk b on a.[Position_Name] = b.Position
                              WHERE LEN([Org_Unit_Name]) > 2 AND NES = 'NES'
                              AND
                              [Org_Unit_Name] =  @SchoolName
							  AND
							  a.Position NOT IN (SELECT b.PositionID FROM CrossWalk b WHERE b.Position IS NOT NULL)
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

        /*PULLING FROM NEW SCHEMA - REAL DATA */
        [Route("api/Crosswalk/fetchAllPositionsBySchoolName/{SchoolName}")]
        [HttpGet]
        public List<Positions> fetchAllPositionsBySchoolName(string SchoolName)
        {
            Positions oPositions;
            List<Positions> lstPositionData = new List<Positions>();

            var connectionString = "";
            string SQLCommandText = "";
         

            SQLCommandText = @"SELECT distinct a.Position as PositionNumber,a.[Position_Name] as Position, CONVERT(varchar(20),a.Position) + ' - ' + a.[Position_Name] AS CMBPos,
                              CrossWalked = CASE
		                      WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
							  FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							  LEFT JOIN CrossWalk b on a.[Position_Name] = b.Position
                              WHERE LEN([Org_Unit_Name]) > 2 AND NES = 'NES'
                              AND
                              [Org_Unit_Name] =  @SchoolName
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

        /*PULLING FROM NEW SCHEMA - REAL DATA */
        [Route("api/Crosswalk/fetchUnassignedPositions/{SchoolName}")]
        [HttpGet]
        public List<Positions> fetchUnassignedPositions(string SchoolName)
        {
            Positions oPositions;
            List<Positions> lstPositionData = new List<Positions>();

            var connectionString = "";
            string SQLCommandText = "";
            
            /*
            SQLCommandText = @"SELECT distinct Position as PositionNumber,[Position_Name] as Position,CONVERT(varchar(20),Position) + ' - ' + [Position_Name] AS CMBPos FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT
                              WHERE LEN([Org_Unit_Name]) > 2 AND NES = 'NES'
                              AND
                              [Org_Unit_Name] = @SchoolName
							  AND
							  [Position_Name] NOT IN (SELECT b.Position FROM CrossWalk b WHERE b.Position IS NOT NULL)
                              order by
                              [Position]";

            */

            SQLCommandText = @"SELECT distinct a.Position as PositionNumber,a.[Position_Name] as Position, CONVERT(varchar(20),a.Position) + ' - ' + a.[Position_Name] AS CMBPos,
                              CrossWalked = CASE
		                      WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
							  FROM YPBI_HPAOS_YPAOS_AUTH_POS_REPORT a
							  LEFT JOIN CrossWalk b on a.[Position_Name] = b.Position
                              WHERE LEN([Org_Unit_Name]) > 2 AND NES = 'NES'
                              AND
                              [Org_Unit_Name] =  @SchoolName
							  AND
							  [Position_Name] NOT IN (SELECT b.Position FROM CrossWalk b WHERE b.Position IS NOT NULL)
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



        /*PULLING FROM NEW SCHEMA - REAL DATA */
        [Route("api/Crosswalk/fetcAssignedPositions/{SchoolName}")]
        [HttpGet]
        public List<Positions> fetcAssignedPositions(string SchoolName)
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
							  [Position_Name] NOT IN (SELECT b.Position FROM CrossWalk b WHERE b.Position IS NOT NULL)
                              order by
                              [Position]";

            */
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


        /*PULLING FROM NEW SCHEMA - REAL DATA */
        [Route("api/Crosswalk/fetchAPRData/{SchoolName}")]
        [HttpGet]
        public List<APRReport> fetchAPRData(string SchoolName)
        {
            APRReport oAPRReport;
            List<APRReport> lstAPRReportData = new List<APRReport>();

            var connectionString = "";
            string SQLCommandText = "";
            SQLCommandText = @"SELECT [NES],
                              [Employee],
                              [Employee_Name],
                              [Division],
                             [Unit],
                             [Org_Unit_Name],
                              [Position],
                              [Position_Name],
                              [Job],
                              [Job_Name],
                              [Status]
                          FROM [YPBI_HPAOS_YPAOS_AUTH_POS_REPORT]
                          WHERE Org_Unit_Name = @SchoolName";

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
                        oAPRReport.NES = row["NES"].ToString();
                        oAPRReport.Employee = row["Employee"].ToString();
                        oAPRReport.Employee_Name = row["Employee_Name"].ToString();
                        oAPRReport.Division = row["Division"].ToString();
                        oAPRReport.Unit = row["Unit"].ToString();
                        oAPRReport.Org_Unit_Name = row["Org_Unit_Name"].ToString();
                        oAPRReport.Position = row["Position"].ToString();
                        oAPRReport.Position_Name = row["Position_Name"].ToString();
                        oAPRReport.Job = row["Job"].ToString();
                        oAPRReport.Job_Name = row["Job_Name"].ToString();
                        oAPRReport.Status = row["Status"].ToString();
                        lstAPRReportData.Add(oAPRReport);
                        oAPRReport = null;
                    }
                }
            }

            return lstAPRReportData;
        }

        /*PULLING FROM NEW SCHEMA - REAL DATA */
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

        /*PULLING FROM NEW SCHEMA - REAL DATA */
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


        /*PULLING FROM NEW SCHEMA - REAL DATA */
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

        /*PULLING FROM NEW SCHEMA - REAL DATA */
        [Route("api/Crosswalk/fetchAllEmployeeDataBySchoolName/{SchoolName}")]
        [HttpGet]
        public List<EmployeeTable> fetchAllEmployeeDataBySchoolName(string SchoolName)
        {
            EmployeeTable oEmployeeTable;
            List<EmployeeTable> lstEmployeeTableData = new List<EmployeeTable>();

            var connectionString = "";
            string SQLCommandText = "";

            /*
            var SQLCommandTextNew = @"SELECT a.EmployeeID,a.SchoolName,a.EmployeeName,a.Certification,a.Role,a.Eligibility,b.CRecordID,b.Position,
                                CrossWalked = CASE
		                        WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                FROM EmployeeTable a 
	                            LEFT JOIN CrossWalk b on a.EmployeeID = b.EmployeeID
								WHERE a.SchoolName = @SchoolName";
            */

            
            /*
            var SQLCommandTextNew = @"SELECT a.Employee as EmployeeID,
                                   a.Org_Unit_Name as SchoolName,
	                               a.Employee_Name as EmployeeName,
	                               a.Position_Name as [Role],
                                   b.PositionID,
                                   b.Position as PositionName,
                                   '' as Certification,
	                               '' as Eligibility,
                                   '' As Certification,
	                               b.CRecordID,b.Position,
	                                CrossWalked = CASE
		                            WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                    FROM [YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a 
	                                LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
		                            WHERE a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1";
            */

            var SQLCommandTextNew = @"SELECT a.Employee as EmployeeID,
                                   a.Org_Unit_Name as SchoolName,
	                               a.Employee_Name as EmployeeName,
	                               a.Position_Name as [Role],
                                   b.PositionID,
                                   b.Position as PositionName,
	                               '' as Eligibility,
								   c.[Qualification Text] As Certification,
                                   '' As Certification,
                                   b.CRecordID,b.Position,
	                                CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                   LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
                                    WHERE a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1";


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
                        if(row["PositionID"].ToString().Length > 2)
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
	                               a.Position_Name as [Role],
                                   b.PositionID,
                                    a.Status,
                                   b.Position as PositionName,
	                               '' as Eligibility,
								   c.[Qualification Text] As Certification,
                                   '' As Certification,
                                   b.CRecordID,b.Position,
	                                CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
                                    WHERE 
                                    --a.Position NOT IN (SELECT b.PositionID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                                    a.Employee NOT IN (SELECT b.EmployeeID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                                    AND
                                    a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1";


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


            /*
            var SQLCommandTextNew = @"SELECT a.Employee as EmployeeID,
                                   a.Org_Unit_Name as SchoolName,
	                               a.Employee_Name as EmployeeName,
	                               a.Position_Name as [Role],
                                   b.PositionID,
                                   b.Position as PositionName,
	                               '' as Eligibility,
								   c.[Qualification Text] As Certification,
                                   '' As Certification,
                                   b.CRecordID,b.Position,
	                                CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END
                                    FROM[YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
                                    WHERE 
                                    a.Position NOT IN (SELECT b.PositionID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                                    AND
                                    a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1";
            */
            var SQLCommandTextNew = @"SELECT a.Employee as EmployeeID,a.Org_Unit_Name as SchoolName,a.Employee_Name as EmployeeName,a.Position_Name as [Role],a.Position,
                                    b.Position as PositionName,b.PositionID,'' as Eligibility,
                                    a.Status,
                                    c.[Qualification Text] As Certification,'' As Certification, b.CRecordID,b.Position,
                                    CrossWalked = CASE
                                    WHEN b.CRecordID IS NULL THEN 'NO' ELSE 'YES' END FROM 
                                    [YPBI_HPAOS_YPAOS_AUTH_POS_REPORT] a
                                    LEFT JOIN CrossWalk b on a.Employee = b.EmployeeID
                                    LEFT JOIN
                                    (SELECT[Employee], CERTIFICATIONS as [Qualification Text]  FROM EMPLOYEE_CERT_TABLE) c on a.Employee = c.Employee
                                    WHERE
                                    --a.Position IN (SELECT b.PositionID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                                    a.Employee IN (SELECT b.EmployeeID FROM CrossWalk b WHERE b.Position IS NOT NULL)
                                    AND
                                    a.Org_Unit_Name = @SchoolName AND LEN(a.Employee) > 1
                                    ";

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

        /*FROM ORIGINAL CODE BUT STILL GOOD TO USE */
        [HttpPost]
        [Route("api/Crosswalk/AddOrUpdateCrosswalkRecord")]
        public bool AddOrUpdateCrosswalkRecord([FromBody] CrosswalkData oCrosswalkEntryData)
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

        /*FROM ORIGINAL CODE BUT STILL GOOD TO USE */
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

        /*FROM ORIGINAL CODE BUT STILL GOOD TO USE */
        [HttpPost]
        [Route("api/Crosswalk/DeleteCrosswalkRecord")]
        public bool DeleteCrosswalkRecord([FromBody] CrosswalkData oCrosswalkEntryData)
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


  


        #endregion UsingRealDataSchema




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


        /*
        [Route("api/Crosswalk/fetchEmployeeDataBySchoolName/{SchoolName}")]
        [HttpGet]
        public List<EmployeeTable> fetchEmployeeDataBySchoolName(string SchoolName)
        {
            EmployeeTable oEmployeeTable;
            List<EmployeeTable> lstEmployeeTableData = new List<EmployeeTable>();

            var connectionString = "";
            string SQLCommandText = "";
            string SQLCommandTextMultiline = @"SELECT e.EmployeeID,
               e.SchoolName,
	           e.EmployeeName,
	           e.Role,
	           e.Certification,
	           CrossWalked = CASE

                             WHEN c.CRecordID IS NOT NULL THEN 'YES' ELSE 'NO'

                             END
          FROM EmployeeTable e
          left join
          CrossWalk c
          on e.EmployeeID = c.EmployeeID
          WHERE
          e.SchoolName = @SchoolName";

            string SQLCommandTextMultiline2 = "SELECT e.EmployeeID, e.SchoolName, e.EmployeeName,e.Role,e.Certification,";
            SQLCommandTextMultiline2 += " CrossWalked = CASE WHEN c.CRecordID IS NOT NULL THEN 'YES' ELSE 'NO' END ";
            SQLCommandTextMultiline2 += "FROM EmployeeTable e left join CrossWalk c on e.EmployeeID = c.EmployeeID WHERE e.SchoolName = ";
            SQLCommandTextMultiline2 += "'";
            SQLCommandTextMultiline2 += SchoolName;
            SQLCommandTextMultiline2 += "'";


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
                using (SqlCommand cmd = new SqlCommand(SQLCommandTextMultiline2, CONN))
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
                        oEmployeeTable.CrossWalked = row["CrossWalked"].ToString();


                        lstEmployeeTableData.Add(oEmployeeTable);
                        oEmployeeTable = null;
                    }
                }
            }

            return lstEmployeeTableData;
        }
        */
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
