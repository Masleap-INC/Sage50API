using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

using SimplySDK;
using SimplySDK.Support;

namespace CustomerAPI.Controllers
{
    public class CustomersController : ApiController
    {

        private static short GetAccountNumberLen()
        {
            return (short)(new SDKDatabaseUtility()).RunScalerQuery("SELECT nActNumLen FROM tCompOth");
        }


        // GET: api/Customers
        [ResponseType(typeof(String))]
        public IHttpActionResult GetCustomers()
        {

            var fileName = "C:\\Users\\Public\\Documents\\Simply Accounting\\2023\\Samdata\\Premium\\Universl.SAI";
            var status = "lal vai mbbs";

            try
            {
                SDKInstanceManager.SDKResult sdkResult;
                if (SDKInstanceManager.Instance.OpenDatabase(fileName.ToUpper(), "sysadmin", "sysadmin", true, "Sage 50 SDK Sample Program", "SASDK", 1, out sdkResult))
                {
                    status = "Connected: " + GetAccountNumberLen();
                }
                else
                {
                    status = "Not Connected";
                }
            }
            catch (Exception ex)
            {
                status = ex.Message;
            }

            SDKInstanceManager.Instance.CloseDatabase();

            return Ok(status);
        }

    }
}