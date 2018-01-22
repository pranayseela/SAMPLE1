using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace TEER.Helpers
{
    public class Config
    {
        public static string ConStr
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["EHOSSE01"].ConnectionString;
            }
        }
        public static int? CommandTimeout
        {
            get
            {
                return Helper.GetValue<int?>(ConfigurationManager.AppSettings["oracleCommandTimeoutSeconds"]);
            }
        }
    }
}