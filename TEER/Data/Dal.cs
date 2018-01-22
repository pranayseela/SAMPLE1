using TEER.Model;
using TEER.Helpers;
using TEER.Security;
using NLog;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Diagnostics;


namespace TEER.Data
{
    public class Dal : BaseDal
    {
        private static readonly Logger _logger = LogManager.GetLogger("Dal");
        static readonly MemoryCache autoLogoutParametersCache = MemoryCache.Default;
        public void SaveLoginInfo(string employeeNumber, string hostName)
        {
            OracleConnection con = new OracleConnection(ConStr);
            OracleCommand com = con.CreateCommand(SCHEMA_EHOS + HOS_PKG + "save_login_info");

            com.Parameters.Add("i_bsc_employee_number", OracleDbType.Varchar2, string.IsNullOrEmpty(employeeNumber) ? null : employeeNumber.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_employee_number", OracleDbType.Varchar2, null, ParameterDirection.Input);
            com.Parameters.Add("i_remote_host", OracleDbType.Varchar2, hostName, ParameterDirection.Input);
            com.Parameters.Add("i_app_module_name", OracleDbType.Varchar2, "EHOS-SE", ParameterDirection.Input);

            try
            {
                con.Open();

                com.ExecuteNonQuery();

                con.Close();
            }
            catch (Exception ex)
            {
                // log and rethrow
                _logger.Fatal(ex.Message);
                throw;
            }
            finally
            {
                Cleanup(con, com);

                con = null;
                com = null;
            }
        }
        public Employee GetEmployeeByBsc(string bscEmployeeNumber)
        {
            try
            {
                return AspNetRoleProvider.One.GetAppUser(null, bscEmployeeNumber);
            }
            catch (OracleException ex)
            {
                // throw a custom exception
                throw new DalException(ExtractOracleMessage(ex.Message), ex);
            }
        }
    }
}
