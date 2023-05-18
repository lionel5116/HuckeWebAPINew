using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;
using HuckeWEBAPI.Models;

namespace HuckeWEBAPI.Controllers
{
    public class PropertyInformationController : ApiController
    {
        public string s_ConnectionString = ConfigurationManager.AppSettings["CONN_STRING"].ToString();
        public string HUCKE_WEB_CONN_STRING = ConfigurationManager.AppSettings["HUCKE_WEB"].ToString();
        public string s_DBaseProvider = ConfigurationManager.AppSettings["currProvider"].ToString();
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

        [Route("api/PropertyInfo/FetchPropertyInformation/{CATXID},{TaxYear}")]
        [HttpGet]
        public List<PropertyInfo> FetchPropertyInformation(string CATXID,string TaxYear)
        {
            PropertyInfo oPropertyInfo;
            List<PropertyInfo> lstPropertyInfo = new List<PropertyInfo>();

            var connectionString = s_ConnectionString;

            string SQLCommandText = $"SELECT * from  PropertyInformation WHERE CATXID = '{CATXID}' AND TaxYear = '{TaxYear}'";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    
                    if(ds.Tables[0].Rows.Count > 0) { } else { return null; };

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oPropertyInfo = new PropertyInfo();

                        oPropertyInfo.id = int.Parse(row["id"].ToString());
                        oPropertyInfo.PCode = row["PCode"].ToString();
                        oPropertyInfo.ClientCode = row["ClientCode"].ToString();
                        oPropertyInfo.TaxYear = row["TaxYear"].ToString();
                        oPropertyInfo.CABORO = row["CABORO"].ToString();
                        oPropertyInfo.CABLOK = row["CABLOK"].ToString();
                        oPropertyInfo.CALOT = row["CALOT"].ToString();
                        oPropertyInfo.CATXID = row["CATXID"].ToString();
                        oPropertyInfo.HSENUM = row["HSENUM"].ToString();
                        oPropertyInfo.CASNAM = row["CASNAM"].ToString();
                        oPropertyInfo.CAZIPC = row["CAZIPC"].ToString();
                        oPropertyInfo.YEARBUILT = int.Parse(row["YEARBUILT"].ToString());
                        oPropertyInfo.OwnerName = row["OwnerName"].ToString();
                        oPropertyInfo.TotalUnits = int.Parse(row["TotalUnits"].ToString());
                        oPropertyInfo.GBA = int.Parse(row["GBA"].ToString());
                        oPropertyInfo.NoOfStories = int.Parse(row["NoOfStories"].ToString());
                        oPropertyInfo.NoOfBuildings = int.Parse(row["NoOfBuildings"].ToString());
                        oPropertyInfo.TaxCLS = row["TaxCLS"].ToString();
                        oPropertyInfo.TRNLND = float.Parse(row["TRNLND"].ToString());
                        oPropertyInfo.TRNTTL = float.Parse(row["TRNTTL"].ToString());
                        oPropertyInfo.ACTLND = float.Parse(row["ACTLND"].ToString());
                        oPropertyInfo.ACTTTL = float.Parse(row["ACTTTL"].ToString());
                        oPropertyInfo.CurrActAssdTotal = float.Parse(row["CurrActAssdTotal"].ToString());
                        oPropertyInfo.FeeType = row["FeeType"].ToString();
                        oPropertyInfo.TaxRate = float.Parse(row["TaxRate"].ToString());
                        oPropertyInfo.FeeAmount = float.Parse(row["FeeAmount"].ToString());
                        oPropertyInfo.TotalEstimatedTaxSavings = float.Parse(row["TotalEstimatedTaxSavings"].ToString());
                        oPropertyInfo.FinalAssessedValue = row["FinalAssessedValue"].ToString() != "" ? float.Parse(row["FinalAssessedValue"].ToString()) : 0;
                        //FinalAssessedValue

                        lstPropertyInfo.Add(oPropertyInfo);
                        oPropertyInfo = null;
                    }
                }
            }

            return lstPropertyInfo;
        }
        [Route("api/PropertyInfo/fetchPropertyRecords/{_SEARCH_STRING_}")]
        [HttpGet]
        public List<PropertyInfo> fetchPropertyRecords(string _SEARCH_STRING_)
        {
            PropertyInfo oPropertyInfo;
            List<PropertyInfo> lstPropertyInfo = new List<PropertyInfo>();

            string[] searchCriteria = _SEARCH_STRING_.Split('|');
            //0 = Type
            //1 = Value

            var connectionString = s_ConnectionString;

            string SQLCommandText = "";

            switch (searchCriteria[0])
            {
                case "ClientCode":
                    SQLCommandText = $"SELECT * from  PropertyInformation WHERE ClientCode = '{searchCriteria[1]}'";
                    break;
                case "CATXID":
                    SQLCommandText = $"SELECT * from  PropertyInformation WHERE CATXID = '{searchCriteria[1]}'";
                    break;
                case "ID":
                    SQLCommandText = $"SELECT * from  PropertyInformation WHERE id = {searchCriteria[1]}";
                    break;
                default:
                    SQLCommandText = "";
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

                    if (ds.Tables[0].Rows.Count > 0) { } else { return null; };

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        oPropertyInfo = new PropertyInfo();

                        oPropertyInfo.id = int.Parse(row["id"].ToString());
                        oPropertyInfo.PCode = row["PCode"].ToString();
                        oPropertyInfo.ClientCode = row["ClientCode"].ToString();
                        oPropertyInfo.TaxYear = row["TaxYear"].ToString();
                        oPropertyInfo.CABORO = row["CABORO"].ToString();
                        oPropertyInfo.CABLOK = row["CABLOK"].ToString();
                        oPropertyInfo.CALOT = row["CALOT"].ToString();
                        oPropertyInfo.CATXID = row["CATXID"].ToString();
                        oPropertyInfo.HSENUM = row["HSENUM"].ToString();
                        oPropertyInfo.CASNAM = row["CASNAM"].ToString();
                        oPropertyInfo.CAZIPC = row["CAZIPC"].ToString();
                        oPropertyInfo.YEARBUILT = int.Parse(row["YEARBUILT"].ToString());
                        oPropertyInfo.OwnerName = row["OwnerName"].ToString();
                        oPropertyInfo.TotalUnits = int.Parse(row["TotalUnits"].ToString());
                        oPropertyInfo.GBA = int.Parse(row["GBA"].ToString());
                        oPropertyInfo.NoOfStories = int.Parse(row["NoOfStories"].ToString());
                        oPropertyInfo.NoOfBuildings = int.Parse(row["NoOfBuildings"].ToString());
                        oPropertyInfo.TaxCLS = row["TaxCLS"].ToString();
                        oPropertyInfo.TRNLND = float.Parse(row["TRNLND"].ToString());
                        oPropertyInfo.TRNTTL = float.Parse(row["TRNTTL"].ToString());
                        oPropertyInfo.ACTLND = float.Parse(row["ACTLND"].ToString());
                        oPropertyInfo.ACTTTL = float.Parse(row["ACTTTL"].ToString());
                        oPropertyInfo.CurrActAssdTotal = float.Parse(row["CurrActAssdTotal"].ToString());
                        oPropertyInfo.FeeType = row["FeeType"].ToString();
                        oPropertyInfo.TaxRate = float.Parse(row["TaxRate"].ToString());
                        oPropertyInfo.FeeAmount = float.Parse(row["FeeAmount"].ToString());
                        oPropertyInfo.TotalEstimatedTaxSavings = float.Parse(row["TotalEstimatedTaxSavings"].ToString());
                        oPropertyInfo.FinalAssessedValue = row["FinalAssessedValue"].ToString() != "" ? float.Parse(row["FinalAssessedValue"].ToString()) : 0;
                        

                        lstPropertyInfo.Add(oPropertyInfo);
                        oPropertyInfo = null;
                    }
                }
            }

            return lstPropertyInfo;
        }

        [Route("api/PropertyInfo/FetchMainAccountInfo/{CATXID}")]
        [HttpGet]
        public List<MainAccountInfo> FetchMainAccountInfo(string CATXID)
        {
            MainAccountInfo oMainAccountInfo;
            List<MainAccountInfo> lstMainAcctPropertyInfo = new List<MainAccountInfo>();

            var connectionString = HUCKE_WEB_CONN_STRING;

            string SQLCommandText = $"SELECT * from  MainAcctDataFinal WHERE CATXID = '{CATXID}'";

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
                        oMainAccountInfo = new MainAccountInfo();

                        oMainAccountInfo.TaxYear = row["year"].ToString();
                        oMainAccountInfo.CABORO = row["CABORO"].ToString();
                        oMainAccountInfo.CABLOK = row["CABLOK"].ToString();
                        oMainAccountInfo.CALOT = row["CALOT"].ToString();
                        oMainAccountInfo.CATXID = row["CATXID"].ToString();
                        oMainAccountInfo.HSENUM = row["HSENUM"].ToString();
                        oMainAccountInfo.CASNAM = row["CASNAM"].ToString();
                        oMainAccountInfo.CAZIPC = row["CAZIPC"].ToString();
                        oMainAccountInfo.YEARBUILT = int.Parse(row["YEARBUILT"].ToString());
                        oMainAccountInfo.OwnerName = row["OwnerName"].ToString();
                        oMainAccountInfo.TotalUnits = int.Parse(row["TotalUnits"].ToString());
                        oMainAccountInfo.GBA = int.Parse(row["GBA"].ToString());
                        oMainAccountInfo.NoOfStories = int.Parse(row["NoOfStories"].ToString());
                        oMainAccountInfo.NoOfBuildings = int.Parse(row["NoOfBuildings"].ToString());
                        oMainAccountInfo.TaxCLS = row["TaxCLS"].ToString();
                        oMainAccountInfo.TRNLND = float.Parse(row["TRNLND"].ToString());
                        oMainAccountInfo.TRNTTL = float.Parse(row["TRNTTL"].ToString());
                        oMainAccountInfo.ACTLND = float.Parse(row["ACTLND"].ToString());
                        oMainAccountInfo.ACTTTL = float.Parse(row["ACTTTL"].ToString());
                        oMainAccountInfo.CurrActAssdTotal = float.Parse(row["CurrActAssdTotal"].ToString());


                        lstMainAcctPropertyInfo.Add(oMainAccountInfo);
                        oMainAccountInfo = null;
                    }
                }
            }

            if(lstMainAcctPropertyInfo.Count > 0)
            {
                return lstMainAcctPropertyInfo;
            }
            else
            {
                return null;
            }
            
        }

        [HttpPost]
        [Route("api/PropertyInfo/AddOrUpdatePropertyRecord")]
        public bool AddOrUpdatePropertyRecord([FromBody] PropertyInfo oPropertyInfo)
        {
            bool bSuccess = false;
            var connectionString = "";
            connectionString = s_ConnectionString;

            const string sql = @"
                                IF NOT EXISTS(SELECT 1 FROM PropertyInformation WHERE CATXID = @CATXID AND TaxYear = @TaxYear)
                                    BEGIN
                                        INSERT INTO PropertyInformation(
											PCode,
											ClientCode ,
											TaxYear,
											CABORO,
											CABLOK ,
											CALOT,
											CATXID,
											HSENUM ,
											CASNAM ,
											CAZIPC ,
											YEARBUILT ,
											OwnerName ,
											TotalUnits ,
											GBA ,
											NoOfStories ,
											NoOfBuildings,
											TaxCLS,
											TRNLND,
											TRNTTL,
											ACTLND ,
											ACTTTL,
											CurrActAssdTotal,
											FeeType,
											TaxRate,
											FeeAmount,
											TotalEstimatedTaxSavings,
                                            FinalAssessedValue)
                                        VALUES(
											@PCode,
											@ClientCode ,
											@TaxYear,
											@CABORO,
											@CABLOK ,
											@CALOT,
											@CATXID,
											@HSENUM ,
											@CASNAM ,
											@CAZIPC ,
											@YEARBUILT ,
											@OwnerName ,
											@TotalUnits ,
											@GBA ,
											@NoOfStories ,
											@NoOfBuildings,
											@TaxCLS,
											@TRNLND,
											@TRNTTL,
											@ACTLND ,
											@ACTTTL,
											@CurrActAssdTotal,
											@FeeType,
											@TaxRate,
											@FeeAmount,
											@TotalEstimatedTaxSavings,
                                            @FinalAssessedValue)
                                    END
                                ELSE
                                    BEGIN
                                       UPDATE PropertyInformation SET 
                                        PCode= @PCode,
										ClientCode = @ClientCode ,
										TaxYear= @TaxYear,
										CABORO = @CABORO,
										CABLOK = @CABLOK ,
										CALOT = @CALOT,
										CATXID = @CATXID,
										HSENUM = @HSENUM ,
										CASNAM = @CASNAM ,
										CAZIPC = @CAZIPC ,
										YEARBUILT = @YEARBUILT ,
										OwnerName = @OwnerName ,
										TotalUnits = @TotalUnits ,
										GBA = @GBA ,
										NoOfStories = @NoOfStories ,
										NoOfBuildings = @NoOfBuildings,
										TaxCLS = @TaxCLS,
										TRNLND = @TRNLND,
										TRNTTL = @TRNTTL,
										ACTLND = @ACTLND ,
										ACTTTL = @ACTTTL,
										CurrActAssdTotal = @CurrActAssdTotal,
										FeeType = @FeeType,
										TaxRate = @TaxRate,
										FeeAmount = @FeeAmount,
										TotalEstimatedTaxSavings = @TotalEstimatedTaxSavings,
                                        FinalAssessedValue = @FinalAssessedValue
                                        WHERE CATXID = @CATXID AND TaxYear = @TaxYear
                                    END";

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@PCode", oPropertyInfo.PCode);
                    cmd.Parameters.AddWithValue("@ClientCode", oPropertyInfo.ClientCode);
                    cmd.Parameters.AddWithValue("@TaxYear", oPropertyInfo.TaxYear);
                    cmd.Parameters.AddWithValue("@CABORO", oPropertyInfo.CABORO);
                    cmd.Parameters.AddWithValue("@CABLOK", oPropertyInfo.CABLOK);
                    cmd.Parameters.AddWithValue("@CALOT", oPropertyInfo.CALOT);
                    cmd.Parameters.AddWithValue("@CATXID", oPropertyInfo.CATXID);
                    cmd.Parameters.AddWithValue("@HSENUM", oPropertyInfo.HSENUM);
                    cmd.Parameters.AddWithValue("@CASNAM", oPropertyInfo.CASNAM);
                    cmd.Parameters.AddWithValue("@CAZIPC", oPropertyInfo.CAZIPC);
                    cmd.Parameters.AddWithValue("@YEARBUILT", oPropertyInfo.YEARBUILT);
                    cmd.Parameters.AddWithValue("@OwnerName", oPropertyInfo.OwnerName);
                    cmd.Parameters.AddWithValue("@TotalUnits", oPropertyInfo.TotalUnits);
                    cmd.Parameters.AddWithValue("@GBA", oPropertyInfo.GBA);
                    cmd.Parameters.AddWithValue("@NoOfStories", oPropertyInfo.NoOfStories);
                    cmd.Parameters.AddWithValue("@NoOfBuildings", oPropertyInfo.NoOfBuildings);
                    cmd.Parameters.AddWithValue("@TaxCLS", oPropertyInfo.TaxCLS);
                    cmd.Parameters.AddWithValue("@TRNLND", oPropertyInfo.TRNLND);
                    cmd.Parameters.AddWithValue("@TRNTTL", oPropertyInfo.TRNTTL);
                    cmd.Parameters.AddWithValue("@ACTLND", oPropertyInfo.ACTLND);
                    cmd.Parameters.AddWithValue("@ACTTTL", oPropertyInfo.ACTTTL);
                    cmd.Parameters.AddWithValue("@CurrActAssdTotal", oPropertyInfo.CurrActAssdTotal);
                    cmd.Parameters.AddWithValue("@FeeType", oPropertyInfo.FeeType);
                    cmd.Parameters.AddWithValue("@TaxRate", oPropertyInfo.TaxRate);
                    cmd.Parameters.AddWithValue("@FeeAmount", oPropertyInfo.FeeAmount);
                    cmd.Parameters.AddWithValue("@TotalEstimatedTaxSavings", oPropertyInfo.TotalEstimatedTaxSavings);
                    cmd.Parameters.AddWithValue("@FinalAssessedValue", oPropertyInfo.FinalAssessedValue);

                    if (oPropertyInfo.PCode.Length > 0 &&
                        oPropertyInfo.ClientCode.Length > 0 &&
                        oPropertyInfo.TaxYear.Length > 0) { }
                    else { return false; }

                    try
                    {
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
                    }
                    catch (Exception error)
                    {
                        System.Diagnostics.Debug.WriteLine(error.Message.ToString());
                        bSuccess = false;
                    }

                    CONN.Close();

                }
            }
            return bSuccess;
        }
        [Route("api/PropertyInfo/deletedPropertyRecordByID/{id}")]
        [HttpGet]
        public int deletedPropertyRecordByID(int id)
        {
            int nRecordsEffected = 0;
            string SQLCommandText = $"DELETE PropertyInformation WHERE id = {id}";

            string connectionString = s_ConnectionString;

            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLCommandText, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();

                    da.SelectCommand = cmd;

                    CONN.Open();

                    nRecordsEffected = cmd.ExecuteNonQuery();


                    CONN.Close();
                }
            }

            return nRecordsEffected;
        }
    }
}
