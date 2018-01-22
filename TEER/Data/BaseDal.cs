using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using TEER.Helpers;

namespace TEER.Data
{
    public class BaseDal
    {
        //internal readonly static string SCHEMA_EHOS = "EHOSL01.";
        internal readonly static string SCHEMA_EHOS = "ehosse01.";
        internal readonly static string HOS_MAIN_PKG = "hos_main_pkg.";
        internal readonly static string HOS_REPORTS_PKG = "hos_reports_pkg.";
        internal readonly static string HOS_PKG = "hours_of_service_pkg.";
        internal readonly static int OTHER_LOCATION_ID = 10000;
        internal readonly static int BRANCH_ID_OTHER = 10000;


        public string ConStr
        {
            get
            {
                return Config.ConStr;
            }
        }


        internal static string ExtractOracleMessage(string oraMessage)
        {
            //ORA-20101: More than one record exist for the train and date provided. Try providing ...
            string message;
            int pos = oraMessage.IndexOf("\n"); //line break

            if (pos != -1)
            {
                message = oraMessage.Substring(0, pos).Trim();
                pos = message.IndexOf(":");
                if (pos != -1)
                {
                    //CASES WITH JUST A ERR NUMBER AND NO MESSAGE LIKE: ORA-20101:
                    if (message.Length > pos + 1)
                    {
                        message = message.Substring(pos + 1).Trim();
                    }
                }
            }
            else
            {
                message = oraMessage;
            }

            return message;
        }

        internal static void Cleanup(
            OracleConnection con,
            OracleCommand com,
            OracleDataReader dr = null,
            OracleDataAdapter dap = null)
        {
            if (con != null)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }

                con.Dispose();
                con = null;
            }

            if (com != null)
            {
                foreach (OracleParameter parameter in com.Parameters)
                {
                    if (parameter != null)
                    {
                        parameter.Dispose();
                    }
                }

                com.Parameters.Clear();
                com.Dispose();
                com = null;
            }

            if (dr != null)
            {
                dr.Dispose();
                dr = null;
            }

            if (dap != null)
            {
                dap.Dispose();
                dap = null;
            }
        }
    }
}
