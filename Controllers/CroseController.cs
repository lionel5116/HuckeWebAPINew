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
using System.IO;
using System.Security.Cryptography;

//using System.Text.Json.Serialization;
//using System.Text.Json;
using Newtonsoft.Json;

using System.Web.Script.Serialization;

namespace HuckeWEBAPI.Controllers
{
    public class CroseController : ApiController

    {

        public string s_ConnectionStringCROSE = ConfigurationManager.AppSettings["CONN_STRING_CROSE"].ToString();
        public string s_ConnectionStringCROSE_PROD_DB = ConfigurationManager.AppSettings["CONN_STRING_CROSE_PROD_DB"].ToString();
        //
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

        public class VendorList {
            public string random { get; set; }
            public string Name { get; set; }
            public string VendorNumber { get; set; }
            public string CheckNumber { get; set; }
            public string CheckDate { get; set; }
            public  double Amount { get; set; }

        }

     

        [Route("api/crose/getAllCroseVendorData/")]
        [HttpGet]
        public List<VendorCROSE> getAllCroseVendorData()
        {
            VendorCROSE oVendorCROSE;
            List<VendorCROSE> lstCROSEData = new List<VendorCROSE>();
            var connectionString = s_ConnectionStringCROSE;
          
            var ConnectionStringPrivate = "server=DWQADB01;uid=app_daep;pwd=";
            ConnectionStringPrivate += ";database=EDB";

            string SQLCommandText = $"SELECT (VendorNumber + CONVERT(CHAR,[CheckDate]))as random,VendorNumber,Name,CheckNumber, CheckDate,Amount FROM [EDB].[EXT].[VendorCROSE] ";
          

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
        
        [Route("api/crose/getCroseVendorData/{croseArgs}")]
        [HttpGet]
        public List<VendorCROSE> getCroseVendorData(string croseArgs)
        {

            string[] _searchValues = croseArgs.Split('|');


            VendorCROSE oVendorCROSE;
            List<VendorCROSE> lstCROSEData = new List<VendorCROSE>();
            var connectionString = s_ConnectionStringCROSE;

            //string dbPassword = DecryptString(s_ConnectionStringCROSE, "CroseApp2024");


            var ConnectionStringPrivate = "server=DWQADB01;uid=app_daep;pwd=";
            //ConnectionStringPrivate += dbPassword;
            ConnectionStringPrivate += ";database=EDB";

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
            //using (SqlConnection CONN = new SqlConnection(ConnectionStringPrivate))
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

            //string dbPassword = DecryptString(s_ConnectionStringCROSE, "CroseApp2024");


            var ConnectionStringPrivate = "server=DWQADB01;uid=app_daep;pwd=";
            //ConnectionStringPrivate += dbPassword;
            ConnectionStringPrivate += ";database=EDB";

            string SQLCommandText = $"SELECT distinct(VendorNumber), Name FROM [EDB].[EXT].[VendorCROSE] ";
            SQLCommandText += " ORDER BY Name";

            //using (SqlConnection CONN = new SqlConnection(ConnectionStringPrivate))
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

        [Route("api/crose/getCroseVendorList_PROD_DB/")]
        [HttpGet]
        public List<VendorCROSE> getCroseVendorList_PROD_DB()
        {

            VendorCROSE oVendorCROSE;
            List<VendorCROSE> lstCROSEData = new List<VendorCROSE>();
            var connectionString = s_ConnectionStringCROSE_PROD_DB;

            string SQLCommandText = $"SELECT distinct(VendorNumber), Name FROM [EDB].[EXT].[VendorCROSE] ";
            SQLCommandText += " ORDER BY Name";

         
            using (SqlConnection CONN = new SqlConnection(s_ConnectionStringCROSE_PROD_DB))
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

        private string EncryptString(string plaintext, string password)
        {
            // Convert the plaintext string to a byte array
            byte[] plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);

            // Derive a new password using the PBKDF2 algorithm and a random salt
            Rfc2898DeriveBytes passwordBytes = new Rfc2898DeriveBytes(password, 20);

            // Use the password to encrypt the plaintext
            Aes encryptor = Aes.Create();
            encryptor.Key = passwordBytes.GetBytes(32);
            encryptor.IV = passwordBytes.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(plaintextBytes, 0, plaintextBytes.Length);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private string DecryptString(string encrypted, string password)
        {
            // Convert the encrypted string to a byte array
            byte[] encryptedBytes = Convert.FromBase64String(encrypted);

            // Derive the password using the PBKDF2 algorithm
            Rfc2898DeriveBytes passwordBytes = new Rfc2898DeriveBytes(password, 20);

            // Use the password to decrypt the encrypted string
            Aes encryptor = Aes.Create();
            encryptor.Key = passwordBytes.GetBytes(32);
            encryptor.IV = passwordBytes.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                }
                return System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        [Route("api/crose/GetCroseTransactions")]
        [HttpGet]
        public List<VendorList> GetCroseTransactions()
        {
            var _vendorListData = new List<VendorList>();

           /* var options = new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString |
                 JsonNumberHandling.WriteAsString
            };
           */



            string path = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6);

            path += "\\CPI_VENDOR_DATA.JSON.json";


            using (StreamReader r = new StreamReader(path))
            {
                 string json = r.ReadToEnd();
                // _vendorListData = System.Text.Json.JsonSerializer.Deserialize<List<VendorList>>(json, options);
                _vendorListData = JsonConvert.DeserializeObject<List<VendorList>>(json);

            }

            if (_vendorListData != null && _vendorListData.Count > 0)
            {
                Console.WriteLine("Number of records: " + _vendorListData.Count);


                //using a LINQ QUERY
                var jnames = _vendorListData.Where(p => p.Name.StartsWith("J"));
                foreach (var vendor in jnames)
                {
                    Console.WriteLine($"{vendor.Name}- {vendor.VendorNumber}");
                }

                return _vendorListData;
            }
            else
            {
                return null;
            }


        }

        #region FileBasedEndpoints
        [Route("api/crose/getCroseVendorDataFILE_BASED/{croseArgs}")]
        [HttpGet]
        public List<VendorCROSE> getCroseVendorDataFILE_BASED(string croseArgs)
        {

            var _vendorListData = new List<VendorList>();
            _vendorListData = GetCroseTransactions();

           string[] _searchValues = croseArgs.Split('|');


            VendorCROSE oVendorCROSE;
            List<VendorCROSE> lstCROSEData = new List<VendorCROSE>();

            switch (_searchValues[4])
            {
                case "VendorNumber":
                    //SQLCommandText += " WHERE VendorNumber LIKE " + "'%" + _searchValues[0] + "%'";
                    var dataByVendorNumber = _vendorListData.Where(v => v.VendorNumber.Contains(_searchValues[0]));
                    foreach (var _vendor in dataByVendorNumber)
                    {
                        oVendorCROSE = new VendorCROSE();
                        oVendorCROSE.random = _vendor.random;
                        oVendorCROSE.VendorNumber = _vendor.VendorNumber;
                        oVendorCROSE.Name = _vendor.Name;
                        oVendorCROSE.CheckNumber = _vendor.CheckNumber;
                        oVendorCROSE.CheckDate = DateTime.Parse(_vendor.CheckDate);
                        oVendorCROSE.Amount = _vendor.Amount;
                        lstCROSEData.Add(oVendorCROSE);
                        oVendorCROSE = null;
                    }
                    break;
                case "VendorName":
                    //SQLCommandText += " WHERE Name LIKE " + "'%" + _searchValues[1] + "%'";
                    var dataByVendorName = _vendorListData.Where(v => v.Name.ToLower().Contains(_searchValues[1].ToLower()));
                    foreach (var _vendor in dataByVendorName)
                    {
                        oVendorCROSE = new VendorCROSE();
                        oVendorCROSE.random = _vendor.random;
                        oVendorCROSE.VendorNumber = _vendor.VendorNumber;
                        oVendorCROSE.Name = _vendor.Name;
                        oVendorCROSE.CheckNumber = _vendor.CheckNumber;
                        oVendorCROSE.CheckDate = DateTime.Parse(_vendor.CheckDate);
                        oVendorCROSE.Amount = _vendor.Amount;
                        lstCROSEData.Add(oVendorCROSE);
                        oVendorCROSE = null;
                    }
                    break;
                case "VendorNameDate":
                    //SQLCommandText += " WHERE Name LIKE " + "'%" + _searchValues[1] + "%'" + " AND " + " CheckDate BETWEEN " + "'" + _searchValues[2] + "'" + "AND " + "'" + _searchValues[3] + "'";
                    var dataByVendorNameAndDateRange = _vendorListData.Where(v => v.Name.ToLower().Contains(_searchValues[1].ToLower()) && DateTime.Parse(v.CheckDate) > Convert.ToDateTime(_searchValues[2]) && DateTime.Parse(v.CheckDate) < Convert.ToDateTime(_searchValues[3]));
                    foreach (var _vendor in dataByVendorNameAndDateRange)
                    {
                        oVendorCROSE = new VendorCROSE();
                        oVendorCROSE.random = _vendor.random;
                        oVendorCROSE.VendorNumber = _vendor.VendorNumber;
                        oVendorCROSE.Name = _vendor.Name;
                        oVendorCROSE.CheckNumber = _vendor.CheckNumber;
                        oVendorCROSE.CheckDate = DateTime.Parse(_vendor.CheckDate);
                        oVendorCROSE.Amount = _vendor.Amount;
                        lstCROSEData.Add(oVendorCROSE);
                        oVendorCROSE = null;
                    }

                    break;
                case "VendorNumberDate":
                    //SQLCommandText += " WHERE VendorNumber LIKE " + "'%" + _searchValues[0] + "%'" + " AND " + " CheckDate BETWEEN " + "'" + _searchValues[2] + "'" + "AND " + "'" + _searchValues[3] + "'";
                    var dataByVVendorNumberDate = _vendorListData.Where(v => v.VendorNumber.Contains(_searchValues[0]) && DateTime.Parse(v.CheckDate) > Convert.ToDateTime(_searchValues[2]) && DateTime.Parse(v.CheckDate) < Convert.ToDateTime(_searchValues[3]));
                    foreach (var _vendor in dataByVVendorNumberDate)
                    {
                        oVendorCROSE = new VendorCROSE();
                        oVendorCROSE.random = _vendor.random;
                        oVendorCROSE.VendorNumber = _vendor.VendorNumber;
                        oVendorCROSE.Name = _vendor.Name;
                        oVendorCROSE.CheckNumber = _vendor.CheckNumber;
                        oVendorCROSE.CheckDate = DateTime.Parse(_vendor.CheckDate);
                        oVendorCROSE.Amount = _vendor.Amount;
                        lstCROSEData.Add(oVendorCROSE);
                        oVendorCROSE = null;
                    }

                    break;
                default:
                    break;
            }


            return lstCROSEData;

         
        }
        [Route("api/crose/getCroseVendorList_PROD_FILE_BASED/")]
        [HttpGet]
        public List<VendorCROSE> getCroseVendorList_PROD_FILE_BASED()
        {

            var _vendorListData = new List<VendorList>();
            _vendorListData = GetCroseTransactions();

            var __vendorListDataDistinct = _vendorListData.GroupBy(u => u.Name)
                                            .Select(grp => grp.ToList())
                                            .ToList();

            VendorCROSE oVendorCROSE;
            List<VendorCROSE> lstCROSEData = new List<VendorCROSE>();
            for (int i = 0; i< __vendorListDataDistinct.Count; i++)
            {
                oVendorCROSE = new VendorCROSE();
                oVendorCROSE.random = __vendorListDataDistinct[i][0].random;
                oVendorCROSE.VendorNumber = __vendorListDataDistinct[i][0].VendorNumber;
                oVendorCROSE.Name = __vendorListDataDistinct[i][0].Name;
                oVendorCROSE.CheckNumber = __vendorListDataDistinct[i][0].CheckNumber;
                oVendorCROSE.CheckDate = DateTime.Parse(__vendorListDataDistinct[i][0].CheckDate);
                oVendorCROSE.Amount = __vendorListDataDistinct[i][0].Amount;
                lstCROSEData.Add(oVendorCROSE);
                oVendorCROSE = null;
            }

            return lstCROSEData;
        }

        #endregion FileBasedEndpoints
    }
}
