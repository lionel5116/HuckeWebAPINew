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
                                IF NOT EXISTS(SELECT 1 FROM CrossWalk WHERE EmployeeID = @EmployeeID AND Position = @Position)
                                    BEGIN
                                        INSERT INTO CrossWalk(
                                           EmployeeID,
                                            Position ,
                                            SchoolName,
                                            DateAdded )
                                        VALUES(
                                                @EmployeeID ,
                                                @Position ,
                                                @SchoolName,
                                                @DateAdded )
                                    END
                                ELSE
                                    BEGIN
                                       UPDATE CrossWalk SET 
                                       EmployeeID = @EmployeeID ,
                                       Position = @Position ,
                                       SchoolName = @SchoolName,
                                       DateAdded = @DateAdded 
                                       WHERE EmployeeID = @EmployeeID AND Position = @Position
                                    END";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql2, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@EmployeeID", oCrosswalkEntryData.EmployeeID);
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
        [Route("api/Crosswalk/AddPositions")]
        public void AddPositions([FromBody] Positions oPositionDDItem)
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

            _SEARCH_STRING += "INSERT INTO Positions (Position) ";
            _SEARCH_STRING += " VALUES ";
            _SEARCH_STRING += "(";
            _SEARCH_STRING += "'";
            _SEARCH_STRING += oPositionDDItem.Position;
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
                    SQLCommandText = @"SELECT Position FROM Positions ORDER BY Position";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT Position FROM Positions ORDER BY Position";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT Position FROM Positions ORDER BY Position";
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
                    SQLCommandText = @"SELECT EmployeeID,Position,SchoolName,DateAdded FROM CrossWalk ORDER BY Position";
                    break;
                case "DEV":
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT EmployeeID,Position,SchoolName,DateAdded FROM CrossWalk ORDER BY Position";
                    break;
                default:
                    connectionString = s_ConnectionString_CrossWalk;
                    SQLCommandText = @"SELECT EmployeeID,Position,SchoolName,DateAdded FROM CrossWalk ORDER BY Position";
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
                        lstCrosswalkData.Add(oCrosswalkData);
                        oCrosswalkData = null;
                    }
                }
            }

            return lstCrosswalkData;
        }
    }
}
