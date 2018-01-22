using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using TEER.Data;
using TEER.Model;
using TEER.Helpers;
using Oracle.ManagedDataAccess.Client;
using System.Diagnostics;

namespace TEER.Security
{
    public class AspNetRoleProvider : RoleProvider
    {
        readonly static object _locker = new object();

        static List<AppRole> _rolesCache = new List<AppRole>();
        //List<AppRole> userRolesCache;
        static bool _refreshRolesCache = false;

        public static AspNetRoleProvider One { get; private set; }


        string _connectionString;

        string _schemaOwner;
        private string SchemaOwner
        {
            get
            {
                if (string.IsNullOrEmpty(_schemaOwner))
                {
                    return string.Empty;
                }
                else
                {
                    return _schemaOwner + ".";
                }
            }
        }

        string _packageName;
        private string PackageName
        {
            get
            {
                if (string.IsNullOrEmpty(_packageName))
                {
                    return string.Empty;
                }
                else
                {
                    return _packageName + ".";
                }
            }
        }


        private List<AppRole> RolesCache
        {
            get
            {
                if (_rolesCache.Count == 0 || _refreshRolesCache)
                {
                    lock (_locker)
                    {
                        if (_rolesCache.Count == 0 || _refreshRolesCache)
                        {
                            _rolesCache = GetAllRolesForCache();
                            _refreshRolesCache = false;
                        }
                    }
                }

                return _rolesCache;
            }
        }


        public override string ApplicationName { get; set; }


        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (string.IsNullOrEmpty(name))
            {
                name = GetType().FullName;
            }

            ApplicationName = config["applicationName"].Trim();
            if (string.IsNullOrEmpty(ApplicationName))
            {
                throw new ProviderException("The attribute 'applicationName' is missing or empty.");
            }

            string connectionStringName = config["connectionStringName"].Trim();
            if (string.IsNullOrEmpty(connectionStringName))
            {
                throw new ProviderException("The attribute 'connectionStringName' is missing or empty.");
            }
            else
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringName];
                if (settings == null)
                {
                    throw new ProviderException(string.Format("The connection string name {0} specified by the attribute 'connectionStringName' was not found in the <connectionStrings> element.", connectionStringName));
                }
                else
                {
                    _connectionString = settings.ConnectionString;
                }
            }

            _schemaOwner = config["schemaOwner"].Trim();
            _packageName = config["packageName"].Trim();

            base.Initialize(name, config);

            // assign instance to static property
            One = this;
        }


        public override void CreateRole(string roleName)
        {
            if (RolesCache.Exists(r => r.RoleName.EqualsEx(roleName)))
            {
                return;
            }

            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "create_role", CommandType.StoredProcedure);

            com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("i_role_name", OracleDbType.Varchar2, roleName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_audit_user", OracleDbType.Varchar2, HttpContext.Current.User.Identity.Name, ParameterDirection.Input);

            try
            {
                con.Open();

                com.ExecuteNonQuery();

                con.Close();

                // reset cache
                _refreshRolesCache = true;
            }
            catch (OracleException oex)
            {
                // check if its a custom oracle exception
                if (oex.Number >= 20000 && oex.Number <= 20999)
                {
                    // throw a custom exception
                    throw new DalException(Dal.ExtractOracleMessage(oex.Message), oex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                // cleanup
                Dal.Cleanup(con, com);

                con = null;
                com = null;
            }
        }

        public override bool DeleteRole(string roleName, bool throwIfUsersAssigned)
        {
            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "delete_role", CommandType.StoredProcedure);

            com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("i_role_name", OracleDbType.Varchar2, roleName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_remove_users", OracleDbType.Int32, throwIfUsersAssigned ? 0 : 1, ParameterDirection.Input);

            bool deleted = false;
            try
            {
                con.Open();

                com.ExecuteNonQuery();

                con.Close();

                // reset cache
                _refreshRolesCache = true;

                deleted = true;
            }
            catch (OracleException oex)
            {
                // check if its a custom oracle exception
                if (oex.Number >= 20000 && oex.Number <= 20999)
                {
                    // throw a custom exception
                    throw new DalException(Dal.ExtractOracleMessage(oex.Message), oex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                // cleanup
                Dal.Cleanup(con, com);

                con = null;
                com = null;
            }

            return deleted;
        }


        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "find_employees", CommandType.StoredProcedure);

            com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("i_role_name", OracleDbType.Varchar2, roleName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("o_data", OracleDbType.RefCursor, ParameterDirection.Output);

            OracleDataReader dr = null;
            List<string> items = new List<string>();
            try
            {
                con.Open();

                dr = com.ExecuteReader();

                while (dr.Read())
                {
                    string userName = Convert.ToString(dr["employee_number"]);

                    // include if username matches partially
                    if (userName.IndexOf(usernameToMatch.Trim().ToUpper()) != -1)
                    {
                        items.Add(userName);
                    }
                }

                con.Close();
            }
            catch (OracleException oex)
            {
                // check if its a custom oracle exception
                if (oex.Number >= 20000 && oex.Number <= 20999)
                {
                    // throw a custom exception
                    throw new DalException(Dal.ExtractOracleMessage(oex.Message), oex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                // cleanup
                Dal.Cleanup(con, com);

                con = null;
                com = null;
            }

            return items.ToArray();
        }

        public override string[] GetAllRoles()
        {
            return RolesCache.Select(r => r.RoleName).ToArray();
        }

        public override string[] GetRolesForUser(string username)
        {
            return GetRoles(username).Select(r => r.RoleName).ToArray();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "find_employees", CommandType.StoredProcedure);

            com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("i_role_name", OracleDbType.Varchar2, roleName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("o_data", OracleDbType.RefCursor, ParameterDirection.Output);

            OracleDataReader dr = null;
            List<string> items = new List<string>();
            try
            {
                con.Open();

                dr = com.ExecuteReader();

                while (dr.Read())
                {
                    items.Add(Convert.ToString(dr["employee_number"]));
                }

                con.Close();
            }
            catch (OracleException oex)
            {
                // check if its a custom oracle exception
                if (oex.Number >= 20000 && oex.Number <= 20999)
                {
                    // throw a custom exception
                    throw new DalException(Dal.ExtractOracleMessage(oex.Message), oex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                // cleanup
                Dal.Cleanup(con, com);

                con = null;
                com = null;
            }

            return items.ToArray();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return GetRoles(username).Exists(r => r.RoleName.EqualsEx(roleName));
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null || roleNames == null)
            {
                return;
            }

            for (int i = 0; i < usernames.Length; i++)
            {
                for (int j = 0; j < roleNames.Length; j++)
                {
                    AddUserToRole(usernames[i].Trim().ToUpper(), roleNames[j].Trim().ToUpper());
                }
            }
        }

        public override bool RoleExists(string roleName)
        {
            return RolesCache.Exists(r => r.RoleName.EqualsEx(roleName.Trim()));
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null || roleNames == null)
            {
                return;
            }

            for (int i = 0; i < usernames.Length; i++)
            {
                for (int j = 0; j < roleNames.Length; j++)
                {
                    RemoveUserFromRole(usernames[i].Trim().ToUpper(), roleNames[j].Trim().ToUpper());
                }
            }
        }


        public Employee GetAppUser(string username, string bscEmployeeNumber)
        {
            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "get_employee_details");

            //com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("i_employee_number", OracleDbType.Varchar2, string.IsNullOrEmpty(username) ? null : username.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_bsc_employee_number", OracleDbType.Varchar2, string.IsNullOrEmpty(bscEmployeeNumber) ? null : bscEmployeeNumber.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("o_data", OracleDbType.RefCursor, ParameterDirection.Output);

            OracleDataReader dr = null;
            Employee employee = null;
            try
            {
                con.Open();

                dr = com.ExecuteReader();

                if (dr.Read())
                {
                    employee = new Employee();

                    PopulateEmployee(employee, dr);
                }

                con.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                BaseDal.Cleanup(con, com, dr);

                con = null;
                com = null;
                dr = null;
            }

            return employee;
        }

        public List<Employee> FindAppUsers(string username,
            string firstName,
            string lastName,
            string jobDescription,
            string roleName,
            bool? active)
        {
            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "find_employees");

            com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("i_employee_number", OracleDbType.Varchar2, string.IsNullOrEmpty(username) ? null : username.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_first_name", OracleDbType.Varchar2, string.IsNullOrEmpty(firstName) ? null : firstName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_last_name", OracleDbType.Varchar2, string.IsNullOrEmpty(lastName) ? null : lastName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_job_descr", OracleDbType.Varchar2, string.IsNullOrEmpty(jobDescription) ? null : jobDescription.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_role_name", OracleDbType.Varchar2, string.IsNullOrEmpty(roleName) ? null : roleName, ParameterDirection.Input);

            if (active != null)
            {
                com.Parameters.Add("i_employee_status", OracleDbType.Int32, active.Value ? 1 : 0, ParameterDirection.Input);
            }

            com.Parameters.Add("o_data", OracleDbType.RefCursor, ParameterDirection.Output);

            OracleDataReader dr = null;
            List<Employee> items = new List<Employee>();
            try
            {
                con.Open();

                dr = com.ExecuteReader();

                while (dr.Read())
                {
                    Employee employee = new Employee();

                    PopulateEmployee(employee, dr);

                    items.Add(employee);
                }

                con.Close();
            }
            finally
            {
                BaseDal.Cleanup(con, com, dr);

                con = null;
                com = null;
                dr = null;
            }

            return items;
        }

        public void CreateAppUser(string username,
            string bscEmployeeNumber,
            string lastName,
            string firstName,
            string middleName,
            string suffix,
            bool active,
            DateTime statusValidity,
            IEnumerable<string> roleName,
            string auditUser)
        {
            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "create_user");

            com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("i_employee_number", OracleDbType.Varchar2, username.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_bsc_employee_number", OracleDbType.Varchar2, bscEmployeeNumber.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_last_name", OracleDbType.Varchar2, lastName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_first_name", OracleDbType.Varchar2, firstName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_middle_name", OracleDbType.Varchar2, middleName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_suffix", OracleDbType.Varchar2, suffix.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_employee_status", OracleDbType.Int32, (active ? 1 : 0), ParameterDirection.Input);
            com.Parameters.Add("i_status_eff_end_date", OracleDbType.Date, statusValidity, ParameterDirection.Input);
            com.Parameters.Add("i_role_list", OracleDbType.Varchar2, string.Join(",", roleName), ParameterDirection.Input);
            com.Parameters.Add("i_audit_user", OracleDbType.Varchar2, auditUser, ParameterDirection.Input);

            try
            {
                con.Open();

                com.ExecuteNonQuery();

                con.Close();
            }
            catch (OracleException oex)
            {
                // check if its a custom oracle exception
                if (oex.Number >= 20000 && oex.Number <= 20999)
                {
                    // throw a custom exception
                    throw new DalException(BaseDal.ExtractOracleMessage(oex.Message), oex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                BaseDal.Cleanup(con, com);

                con = null;
                com = null;
            }
        }

        public void UpdateAppUser(string username,
            string bscEmployeeNumber,
            string lastName,
            string firstName,
            string middleName,
            string suffix,
            bool active,
            DateTime statusValidity,
            IEnumerable<string> roleName,
            string auditUser)
        {
            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "update_user");

            com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("i_employee_number", OracleDbType.Varchar2, username.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_bsc_employee_number", OracleDbType.Varchar2, bscEmployeeNumber.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_last_name", OracleDbType.Varchar2, lastName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_first_name", OracleDbType.Varchar2, firstName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_middle_name", OracleDbType.Varchar2, middleName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_suffix", OracleDbType.Varchar2, suffix.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_employee_status", OracleDbType.Int32, (active ? 1 : 0), ParameterDirection.Input);
            com.Parameters.Add("i_status_eff_end_date", OracleDbType.Date, statusValidity, ParameterDirection.Input);
            com.Parameters.Add("i_role_list", OracleDbType.Varchar2, string.Join(",", roleName), ParameterDirection.Input);
            com.Parameters.Add("i_audit_user", OracleDbType.Varchar2, auditUser, ParameterDirection.Input);

            try
            {
                con.Open();

                com.ExecuteNonQuery();

                con.Close();
            }
            catch (OracleException oex)
            {
                // check if its a custom oracle exception
                if (oex.Number >= 20000 && oex.Number <= 20999)
                {
                    // throw a custom exception
                    throw new DalException(BaseDal.ExtractOracleMessage(oex.Message), oex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                BaseDal.Cleanup(con, com);

                con = null;
                com = null;
            }
        }

        public void DeleteAppUser(string username, string auditUser)
        {
            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "delete_user");

            com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("i_employee_number", OracleDbType.Varchar2, username, ParameterDirection.Input);
            com.Parameters.Add("i_audit_user", OracleDbType.Varchar2, auditUser, ParameterDirection.Input);

            try
            {
                con.Open();

                com.ExecuteNonQueryEx();

                con.Close();
            }
            catch (OracleException oex)
            {
                // check if its a custom oracle exception
                if (oex.Number >= 20000 && oex.Number <= 20999)
                {
                    // throw a custom exception
                    throw new DalException(Dal.ExtractOracleMessage(oex.Message), oex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                Dal.Cleanup(con, com);

                con = null;
                com = null;
            }
        }


        public List<AppRole> GetRoles(string username)
        {
            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "get_roles_for_user");

            //com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("i_bsc_employee_number", OracleDbType.Varchar2, username.Trim(), ParameterDirection.Input);
            //com.Parameters.Add("i_employee_number", OracleDbType.Varchar2, username.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_employee_number", OracleDbType.Varchar2, null, ParameterDirection.Input);
            com.Parameters.Add("o_data", OracleDbType.RefCursor, ParameterDirection.Output);

            OracleDataReader dr = null;
            List<AppRole> items;
            
            
                items = new List<AppRole>();
                

                try
                {
                    con.Open();

                    dr = com.ExecuteReaderEx();

                    while (dr.Read())
                    {
                        int roleId = dr.GetValue<int>("role_id");

                        AppRole item = items.Find(r => r.RoleId == roleId);
                       

                        if (item == null)
                        {
                            item = new AppRole();

                            item.RoleId = roleId;
                            item.RoleName = dr.GetValue<string>("role_name");

                            items.Add(item);
                            
                        }
                    }

                    con.Close();
                }
                catch (OracleException oex)
                {
                    // check if its a custom oracle exception
                    if (oex.Number >= 20000 && oex.Number <= 20999)
                    {
                        // throw a custom exception
                        throw new DalException(Dal.ExtractOracleMessage(oex.Message), oex);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    Dal.Cleanup(con, com, dr);

                    con = null;
                    com = null;
                    dr = null;
                }
                
                return items;
                                                
        }

        private void AddUserToRole(string username, string roleName)
        {
            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "assign_role_to_employee", CommandType.StoredProcedure);

            com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("i_employee_number", OracleDbType.Varchar2, username.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_role_name", OracleDbType.Varchar2, roleName.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_audit_user", OracleDbType.Varchar2, HttpContext.Current.User.Identity.Name, ParameterDirection.Input);

            try
            {
                con.Open();

                com.ExecuteNonQuery();

                con.Close();
            }
            catch (OracleException oex)
            {
                // check if its a custom oracle exception
                if (oex.Number >= 20000 && oex.Number <= 20999)
                {
                    // throw a custom exception
                    throw new DalException(Dal.ExtractOracleMessage(oex.Message), oex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                // cleanup
                Dal.Cleanup(con, com);

                con = null;
                com = null;
            }
        }

        private void RemoveUserFromRole(string username, string roleName)
        {
            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "delete_role_from_employee", CommandType.StoredProcedure);

            com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("i_employee_number", OracleDbType.Varchar2, username.Trim().ToUpper(), ParameterDirection.Input);
            com.Parameters.Add("i_role_name", OracleDbType.Varchar2, roleName.Trim().ToUpper(), ParameterDirection.Input);

            try
            {
                con.Open();

                com.ExecuteNonQuery();

                con.Close();
            }
            catch (OracleException oex)
            {
                // check if its a custom oracle exception
                if (oex.Number >= 20000 && oex.Number <= 20999)
                {
                    // throw a custom exception
                    throw new DalException(Dal.ExtractOracleMessage(oex.Message), oex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                // cleanup
                Dal.Cleanup(con, com);

                con = null;
                com = null;
            }
        }

        private void PopulateEmployee(Employee employee, OracleDataReader dr)
        {
            employee.EmployeeNumber = dr.GetValue<string>("employee_number");
            employee.BscEmployeeNumber = dr.GetValue<string>("bsc_employee_number");
            employee.FirstName = dr.GetValue<string>("first_name");
            employee.LastName = dr.GetValue<string>("last_name");
            employee.MiddleName = dr.GetValue<string>("middle_name");
            employee.Suffix = dr.GetValue<string>("suffix");
            employee.JobDescription = dr.GetValue<string>("job_description");
            employee.Roles = dr.GetValue<string>("role_list");
            employee.AgencyCode = dr.GetValue<string>("agency_code");
            employee.CraftCode = dr.GetValue<string>("craft_code");
            employee.Active = dr.GetValue<int>("employee_status") == 1;
            employee.StatusValidity = dr.GetValue<DateTime>("status_eff_end_date");

            
            
            

        }


        private List<AppRole> GetAllRolesForCache()
        {
            OracleConnection con = new OracleConnection(_connectionString);
            OracleCommand com = con.CreateCommand(SchemaOwner + PackageName + "get_all_roles");

            com.Parameters.Add("i_app_name", OracleDbType.Varchar2, ApplicationName, ParameterDirection.Input);
            com.Parameters.Add("o_data", OracleDbType.RefCursor, ParameterDirection.Output);

            OracleDataReader dr = null;
            List<AppRole> items = new List<AppRole>();
            try
            {
                con.Open();

                dr = com.ExecuteReaderEx();

                while (dr.Read())
                {
                    AppRole item = new AppRole();

                    item.RoleId = dr.GetValue<int>("role_id");
                    item.RoleName = dr.GetValue<string>("role_name");

                    items.Add(item);
                }

                con.Close();
            }
            catch (OracleException oex)
            {
                // check if its a custom oracle exception
                if (oex.Number >= 20000 && oex.Number <= 20999)
                {
                    // throw a custom exception
                    throw new DalException(Dal.ExtractOracleMessage(oex.Message), oex);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                Dal.Cleanup(con, com, dr);

                con = null;
                com = null;
                dr = null;
            }

            return items;
        }
    }
}
