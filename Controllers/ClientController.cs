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
    public class ClientController : ApiController
    {
        public string s_ConnectionString = ConfigurationManager.AppSettings["CONN_STRING"].ToString();
        public string HUCKE_WEB_CONN_STRING = ConfigurationManager.AppSettings["HUCKE_WEB"].ToString();
        public string s_DBaseProvider = ConfigurationManager.AppSettings["currProvider"].ToString();
        public string s_Environment = ConfigurationManager.AppSettings["Environment"].ToString();


        [Route("api/Client/FetchClientInformation/{ClientCode}")]
        [HttpGet]
        public List<Client> FetchClientInformation(string ClientCode)
        {
            Client oClient;
            List<Client> lstClientData = new List<Client>();
            var connectionString = s_ConnectionString;

            string SQLCommandText = $"SELECT * from  Client WHERE ClientCode = '{ClientCode}'";

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
                        oClient = new Client();
                        oClient.id = int.Parse(row["id"].ToString());
                        oClient.ClientCode = row["ClientCode"].ToString();
                        oClient.LastName = row["LastName"].ToString();
                        oClient.FirstName = row["FirstName"].ToString();
                        oClient.Address = row["Address"].ToString();
                        oClient.City = row["City"].ToString();
                        oClient.State = row["State"].ToString();
                        oClient.Zip = row["Zip"].ToString();
                        oClient.Phone = row["Phone"].ToString();
                        oClient.Phone2 = row["Phone2"].ToString();
                        oClient.Cell = row["Cell"].ToString();
                        oClient.Notes = row["Notes"].ToString();
                        oClient.Email = row["Email"].ToString();

                        lstClientData.Add(oClient);
                        oClient = null;


                    }

                }
            }

            return lstClientData;
        }
        [Route("api/Client/FetchAllClientsForBTTable/")]
        [HttpGet]
        public List<BTClientData> FetchAllClientsForBTTable()
        {
            BTClientData oClient;
            List<BTClientData> lstClientData = new List<BTClientData>();
            var connectionString = s_ConnectionString;

            string SQLCommandText = $"SELECT id,ClientCode,LastName,FirstName,Email,Address,City,State,Cell from  Client";

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
                        oClient = new BTClientData();
                        oClient.id = int.Parse(row["id"].ToString());
                        oClient.ClientCode = row["ClientCode"].ToString();
                        oClient.LastName = row["LastName"].ToString();
                        oClient.FirstName = row["FirstName"].ToString();
                        oClient.Address = row["Address"].ToString();
                        oClient.City = row["City"].ToString();
                        oClient.State = row["State"].ToString();
                        oClient.Cell = row["Cell"].ToString();
                        oClient.Email = row["Email"].ToString();

                        lstClientData.Add(oClient);
                        oClient = null;


                    }

                }
            }

            return lstClientData;
        }
        [Route("api/Client/fetchSingleClientRecord/{_SEARCH_STRING_}")]
        [HttpGet]
        public List<BTClientData> fetchSingleClientRecord(string _SEARCH_STRING_)
        {
            BTClientData oClient;
            List<BTClientData> lstClientData = new List<BTClientData>();

            string[] searchCriteria = _SEARCH_STRING_.Split('|');
            //0 = Type
            //1 = Value


            var connectionString = s_ConnectionString;

            string SQLCommandText = "";

            switch (searchCriteria[0])
            {
                case "ClientCode":
                    SQLCommandText = $"SELECT id,ClientCode,LastName,FirstName,Email,Address,City,State,Cell from  Client WHERE ClientCode = '{searchCriteria[1]}'";
                    break;
                case "LastName":
                    SQLCommandText = $"SELECT id,ClientCode,LastName,FirstName,Email,Address,City,State,Cell from  Client WHERE LastName LIKE '%{searchCriteria[1]}%'";
                    break;
                case "ID":
                    SQLCommandText = $"SELECT id,ClientCode,LastName,FirstName,Email,Address,City,State,Cell from  Client WHERE id = {searchCriteria[1]}";
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
                        oClient = new BTClientData();
                        oClient.id = int.Parse(row["id"].ToString());
                        oClient.ClientCode = row["ClientCode"].ToString();
                        oClient.LastName = row["LastName"].ToString();
                        oClient.FirstName = row["FirstName"].ToString();
                        oClient.Address = row["Address"].ToString();
                        oClient.City = row["City"].ToString();
                        oClient.State = row["State"].ToString();
                        oClient.Cell = row["Cell"].ToString();
                        oClient.Email = row["Email"].ToString();

                        lstClientData.Add(oClient);
                        oClient = null;
                    }

                }
            }

            return lstClientData;
        }
        [Route("api/Client/FetchClientInformationFromRedirect/{id}")]
        [HttpGet]
        public List<Client> FetchClientInformationFromRedirect(int id)
        {
            Client oClient;
            List<Client> lstClientData = new List<Client>();
            var connectionString = s_ConnectionString;

            string SQLCommandText = $"SELECT * from  Client WHERE id = {id}";

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
                        oClient = new Client();
                        oClient.id = int.Parse(row["id"].ToString());
                        oClient.ClientCode = row["ClientCode"].ToString();
                        oClient.LastName = row["LastName"].ToString();
                        oClient.FirstName = row["FirstName"].ToString();
                        oClient.Address = row["Address"].ToString();
                        oClient.City = row["City"].ToString();
                        oClient.State = row["State"].ToString();
                        oClient.Zip = row["Zip"].ToString();
                        oClient.Phone = row["Phone"].ToString();
                        oClient.Phone2 = row["Phone2"].ToString();
                        oClient.Cell = row["Cell"].ToString();
                        oClient.Notes = row["Notes"].ToString();
                        oClient.Email = row["Email"].ToString();

                        lstClientData.Add(oClient);
                        oClient = null;


                    }

                }
            }

            return lstClientData;
        }
        [HttpPost]
        [Route("api/Client/AddOrUpdatClientRecord")]
        public bool AddOrUpdatClientRecord([FromBody] Client oClient)
        {
            bool bSuccess = false;
            var connectionString = "";
            connectionString = s_ConnectionString;

            const string sql = @"
                                IF NOT EXISTS(SELECT 1 FROM Client WHERE ClientCode = @ClientCode)
                                    BEGIN
                                        INSERT INTO Client(
											ClientCode,
											LastName ,
											FirstName,
											Address,
											City ,
											State,
											Zip,
											Phone,
											Phone2 ,
											Cell,
											Notes,
                                            Email)
                                        VALUES(								 
										    @ClientCode,
											@LastName ,
											@FirstName,
											@Address,
											@City ,
											@State,
											@Zip,
											@Phone,
											@Phone2 ,
											@Cell,
											@Notes,
                                            @Email)
                                    END
                                ELSE
                                    BEGIN
                                       UPDATE Client SET 
                                        ClientCode= @ClientCode,
										LastName = @LastName,
										FirstName= @FirstName,
										Address= @Address,
										City = @City,
										State= @State,
										Zip= @Zip,
										Phone= @Phone,
										Phone2 = @Phone2,
										Cell= @Cell,
										Notes= @Notes,
                                        Email = @Email
                                        WHERE ClientCode = @ClientCode
                                    END";
            using (SqlConnection CONN = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, CONN))
                {
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Parameters.AddWithValue("@ClientCode", oClient.ClientCode);
                    cmd.Parameters.AddWithValue("@LastName", oClient.LastName);
                    cmd.Parameters.AddWithValue("@FirstName", oClient.FirstName);
                    cmd.Parameters.AddWithValue("@Address", oClient.Address);
                    cmd.Parameters.AddWithValue("@City", oClient.City);
                    cmd.Parameters.AddWithValue("@State", oClient.State);
                    cmd.Parameters.AddWithValue("@Zip", oClient.Zip);
                    cmd.Parameters.AddWithValue("@Phone", oClient.Phone);
                    cmd.Parameters.AddWithValue("@Phone2", oClient.Phone2);
                    cmd.Parameters.AddWithValue("@Cell", oClient.Cell);
                    cmd.Parameters.AddWithValue("@Notes", oClient.Notes);
                    cmd.Parameters.AddWithValue("@Email", oClient.Email);

                    da.SelectCommand = cmd;

                    if(oClient.ClientCode.Length > 0 && 
                        oClient.LastName.Length > 0 &&
                        oClient.FirstName.Length > 0){}else { return false; }

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
                    catch(Exception error)
                    {
                        System.Diagnostics.Debug.WriteLine(error.Message.ToString());
                        bSuccess = false;
                    }
                   
                    CONN.Close();


                }

            }

            return bSuccess;
        }

        [Route("api/Client/deleteClientRecord/{ClientCode}")]
        [HttpGet]
        public bool deleteClientRecord(string ClientCode)
        {
            int nRecordsEffected = 0;
            int nRecordsEffectedProperty = 0;
            bool bRecordsDeleted = false;

            string SQLCommandText = $"DELETE Client WHERE ClientCode = '{ClientCode}'";

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
                    if(nRecordsEffected > 0)
                    {
                        nRecordsEffectedProperty = deleteAsscociatedPropertyRecords(ClientCode);
                    }

                    CONN.Close();
                }
            }

            if(nRecordsEffected > 0 || nRecordsEffectedProperty > 0)
            {
                bRecordsDeleted = true;
            }

            return bRecordsDeleted;
        }

        public int deleteAsscociatedPropertyRecords(string ClientCode)
        {
            int nRecordsEffected = 0;
            string SQLCommandText = $"DELETE PropertyInformation WHERE ClientCode = '{ClientCode}'";

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
    }
}
