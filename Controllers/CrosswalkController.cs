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
        public string s_ConnectionString_CrossWalk = ConfigurationManager.AppSettings["ConnectionString_CrossWalk"].ToString();
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
    }
}
