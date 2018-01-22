using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using TEER.Model;
using NLog;
using TEER.Data;
using TEER.Helpers;
using System.DirectoryServices.Protocols;
namespace TEER.Authentication
{
    public class LirrLdapHelper : LdapHelper
    {
        private static readonly Logger _logger = LogManager.GetLogger("LdapHelper");


        public string LdapServer
        {
            get
            {
                return ConfigurationManager.AppSettings["ldapServer"];
            }
        }

        public int LdapPortNumber
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["ldapPortNumber"]);
            }
        }

        public string LdapUsername
        {
            get
            {
                return ConfigurationManager.AppSettings["ldapUsername"];
            }
        }

        public string LdapPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["ldapPassword"];
            }
        }

        public string LdapBaseDn
        {
            get
            {
                return ConfigurationManager.AppSettings["ldapBaseDn"];
            }
        }

        public static int? LdapBindTimeoutSeconds
        {
            get
            {
                int? num = null;
                int raw;
                if(Int32.TryParse(ConfigurationManager.AppSettings["ldapBindTimeoutSeconds"], out raw))
                {
                    num = raw;
                }
                return num;
            }
        }


        public LirrLdapHelper()
            : base()
        {

        }

        public LirrLdapHelper(bool authenticatePassword)
            : base(authenticatePassword)
        {

        }


        public override LdapAuthResult Authenticate(
            string username,
            string password,
            Func<Employee> getUser)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return GetAuthResult(username, false, Msg.ERROR_LOGIN_USER_NAME_REQUIRED);
            }

            string userDn = null;

            if (ValidateUsernamePassword || CheckLdapUserProperties)
            {
                try
                {
                    userDn = GetUserDistinguishedName(username);
                }
                catch (Exception ex)
                {
                    _logger.Error("Error binding to ldap server {0}:{1}. {2}", LdapServer, LdapPortNumber, ex);

                    return GetAuthResult(username, false, Msg.ERROR_LOGIN_LDAP_SERVER_INVALID);
                }

                if (string.IsNullOrEmpty(userDn))
                {
                    return GetAuthResult(username, false, Msg.ERROR_LOGIN_USER_ACCOUNT_NOT_FOUND);
                }
            }

            if (CheckLdapUserProperties)
            {
                string error = null;
                if (!IsUserValidInLdap(username, out error))
                {
                    return GetAuthResult(username, false, error);
                }
            }

            // validate the credentials 
            bool isvalid = false;

            if (ValidateUsernamePassword)
            {
                isvalid = AuthenticateUser(userDn, password);

                if (isvalid)
                {
                    _logger.Debug(string.Format("Login succeeded for {0}", username));
                }
            }
            else
            {
                isvalid = true;
            }

            if (!isvalid)
            {
                return GetAuthResult(username, false, Msg.ERROR_LOGIN_FAIL_INVALID_CREDENTIALS);
            }

            try
            {
                Employee employee = getUser.Invoke();

                if (employee == null)
                {
                    _logger.Warn("employee {0} not found in database.", username);
                    return GetAuthResult(username, false, Msg.ERROR_EMPLOYEE_NOT_FOUND_IN_APPLICATION);
                }

                if (!employee.Active)
                {
                    _logger.Warn("employee {0} not active in application.", username);
                    return GetAuthResult(username, false, Msg.ERROR_EMPLOYEE_NOT_ACTIVE_IN_APPLICATION);
                }

                return GetAuthResult(username: username,
                           message: null,
                           result: true,
                           employee: employee);
            }
            catch (Exception ex)
            {
                if (ex is DalException)
                {
                    _logger.Error("database error occurred. {0}", ex, null);
                    return GetAuthResult(username, false, Msg.ERROR_UNABLE_TO_ACCESS_APPLICATION_DATABASE);
                }
                else
                {
                    _logger.Error(ex);
                    return GetAuthResult(username, false, Msg.ERROR_OCCURRED_ON_SERVER);
                }
            }

        }

        public override bool IsUserValidInLdap(string username, out string error)
        {
            LdapConnection ldap = null;

            error = null;
            bool valid = true;

            try
            {
                ldap = GetLdapConnection(LdapUsername, LdapPassword);

                // assume valid
                if (ldap == null)
                {
                    return valid;
                }

                SearchResultEntry sre = GetUserSearchResultEntry(ldap, username);
                if (sre == null)
                {
                    error = Msg.ERROR_LOGIN_LDAP_USER_PRINCIPAL_INVALID;
                    valid = false;
                }
                else
                {
                    string employeeStatus = GetSearchResultEntryValue(sre, "employeeStatus");
                    if (!string.IsNullOrEmpty(employeeStatus) && !employeeStatus.EqualsEx("Active"))
                    {
                        error = Msg.ERROR_LOGIN_USER_ACCOUNT_IS_NOT_ACTIVE;
                        valid = false;
                    }

                    string loginDisabled = GetSearchResultEntryValue(sre, "loginDisabled");
                    if (!string.IsNullOrEmpty(loginDisabled) && loginDisabled.EqualsEx("TRUE"))
                    {
                        error = Msg.ERROR_LOGIN_DISABLED;
                        valid = false;
                    }

                    var pei = GetPasswordExpiryInfo(username);
                    if (pei != null && pei.TimeToExpire.TotalMinutes <= 0)
                    {
                        error = Msg.ERROR_LOGIN_PASSWORD_EXPIRED;
                        valid = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
            }
            finally
            {
                if (ldap != null)
                {
                    ldap.Dispose();
                    ldap = null;
                }
            }

            return valid;
        }

        public override PasswordExpiryInfo GetPasswordExpiryInfo(string username)
        {
            // at any point if we cannot determine if the password has expired or not
            // we assume the password has not expired
            PasswordExpiryInfo pei = null;

            LdapConnection ldap = null;

            try
            {
                ldap = GetLdapConnection(LdapUsername, LdapPassword);

                // assume not expired
                if (ldap == null)
                {
                    return null;
                }

                SearchResultEntry sre = GetUserSearchResultEntry(ldap, username);
                if (sre == null)
                {
                    // assume not expired
                    return null;
                }

                string passwordExpirationTime = GetSearchResultEntryValue(sre, "passwordExpirationTime");
                if (!string.IsNullOrEmpty(passwordExpirationTime))
                {
                    DateTime passwordExpiration = DateTime.MinValue;
                    if (DateTime.TryParseExact(passwordExpirationTime, "yyyyMMddHHmmssZ", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None, out passwordExpiration))
                    {
                        pei = new PasswordExpiryInfo(passwordExpiration);

                        return pei;
                    }
                }

                // assume not expired
                return null;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Password expiration check failed. Assume password has not expired and continue authentication. {0}");

                // assume not expired
                return null;
            }
            finally
            {
                if (ldap != null)
                {
                    ldap.Dispose();
                    ldap = null;
                }
            }
        }

        public override Dictionary<string, string> GetLdapProperties(string username)
        {
            Dictionary<string, string> props = new Dictionary<string, string>();

            LdapConnection ldap = null;
            try
            {
                ldap = GetLdapConnection(LdapUsername, LdapPassword);

                if (ldap == null)
                {
                    return props;
                }

                props["Ldap.HostName"] = ldap.SessionOptions.HostName;
                props["Ldap.DomainName"] = ldap.SessionOptions.DomainName;
                props["Ldap.ProtocolVersion"] = ldap.SessionOptions.ProtocolVersion.ToString();
                props["Ldap.SecureSocketLayer"] = ldap.SessionOptions.SecureSocketLayer.ToString();

                SearchResultEntry sre = GetUserSearchResultEntry(ldap, username);
                if (sre != null)
                {
                    props["DistinguishedName"] = sre.DistinguishedName;
                    props["cn"] = GetSearchResultEntryValue(sre, "cn");
                    props["sn"] = GetSearchResultEntryValue(sre, "sn");
                    props["givenName"] = GetSearchResultEntryValue(sre, "givenName");
                    props["employeeID"] = GetSearchResultEntryValue(sre, "employeeID");
                    props["employeeStatus"] = GetSearchResultEntryValue(sre, "employeeStatus");
                    props["loginDisabled"] = GetSearchResultEntryValue(sre, "loginDisabled");
                    props["passwordExpirationTime"] = GetSearchResultEntryValue(sre, "passwordExpirationTime");
                }
            }
            finally
            {
                if (ldap != null)
                {
                    ldap.Dispose();
                    ldap = null;
                }
            }

            return props;
        }


        public LdapConnection GetLdapConnection(string username, string password)
        {
            LdapConnection ldap = null;

            try
            {
                ldap = new LdapConnection(new LdapDirectoryIdentifier(LdapServer, LdapPortNumber));

                ldap.SessionOptions.SecureSocketLayer = true;
                // always trust server certificate
                ldap.SessionOptions.VerifyServerCertificate = (con, cer) => true;

                // if user name is null try anonymous login
                if (string.IsNullOrEmpty(username))
                {
                    ldap.AuthType = AuthType.Anonymous;
                }
                else
                {
                    ldap.Credential = new NetworkCredential(username, password);
                    ldap.AuthType = AuthType.Basic;
                }

                if (LdapBindTimeoutSeconds != null)
                {
                    ldap.Timeout = TimeSpan.FromSeconds(LdapBindTimeoutSeconds.Value);
                }

                ldap.Bind();
            }
            catch (Exception ex)
            {
                _logger.Warn("LDAP bind error. LdapServer={0}, LdapPortNumber={1}, AuthType={2}, Username={3}, Error={4}",
                    LdapServer, LdapPortNumber, ldap.AuthType, username, ex);

                ldap = null;
            }

            return ldap;
        }

        private string GetUserDistinguishedName(string username)
        {
            LdapConnection ldap = null;

            try
            {
                ldap = GetLdapConnection(LdapUsername, LdapPassword);

                if (ldap == null)
                {
                    return null;
                }

                SearchResultEntry sre = GetUserSearchResultEntry(ldap, username);
                if (sre != null)
                {
                    return sre.DistinguishedName;
                }
            }
            finally
            {
                if (ldap != null)
                {
                    ldap.Dispose();
                    ldap = null;
                }
            }

            return null;
        }

        private bool AuthenticateUser(string userDistinguishedName, string password)
        {
            LdapConnection ldap = null;

            try
            {
                // authenticate user by binding to the userDistinguishedName with user provided password
                ldap = GetLdapConnection(userDistinguishedName, password);

                return ldap != null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                if (ldap != null)
                {
                    ldap.Dispose();
                    ldap = null;
                }
            }

            return false;
        }


        private SearchResultEntry GetUserSearchResultEntry(LdapConnection ldap, string username)
        {
            var sre = GetSearchResultEntryByFilter(ldap, username);

            if (sre == null)
            {
                sre = GetSearchResultEntryByDistinguishedName(ldap, username);
            }

            return sre;
        }

        private string GetSearchResultEntryValue(SearchResultEntry entry, string attributeName)
        {
            SearchResultAttributeCollection attributes = entry.Attributes;

            if (!attributes.Contains(attributeName))
            {
                return null;
            }

            try
            {
                return string.Join(",", attributes[attributeName].GetValues(typeof(string)));
            }
            catch
            {
                return null;
            }
        }
        

        private SearchResultEntry GetSearchResultEntryByFilter(LdapConnection ldap, string username)
        {
            string searchFilter = "cn=" + username;

            SearchRequest searchRequest = new SearchRequest(LdapBaseDn, searchFilter, SearchScope.Subtree, null);
            SearchResponse searchResponse = null;

            try
            {
                searchResponse = (SearchResponse)ldap.SendRequest(searchRequest);
            }
            catch (DirectoryOperationException ex)
            {
                _logger.Warn("LDAP SearchRequest. Filter:\"{0}\"; Error:{1}; Response:{2}; ResultCode:{3}",
                    searchFilter, ex.Message, ex.Response.ErrorMessage, ex.Response.ResultCode);
                return null;
            }

            // check if user entry exists
            if (searchResponse != null)
            {
                if (searchResponse.Entries != null)
                {
                    if (searchResponse.Entries.Count == 0)
                    {
                        _logger.Warn("LDAP SearchRequest. SearchResponse.Entries.Count = 0 for search filter: {0}", searchFilter);
                    }
                    else if (searchResponse.Entries.Count == 1)
                    {
                        return searchResponse.Entries[0];
                    }
                    else
                    {
                        _logger.Warn("LDAP SearchRequest. More than one entry found that matches search filter: {0}", searchFilter);
                    }
                }
                else
                {
                    _logger.Warn("LDAP SearchRequest. SearchResponse.Entries = null for search filter: {0}", searchFilter);
                }
            }
            else
            {
                _logger.Warn("LDAP SearchRequest. SearchResponse = null for search filter: {0}", searchFilter);
            }

            return null;
        }

        private SearchResultEntry GetSearchResultEntryByDistinguishedName(LdapConnection ldap, string username)
        {
            string distinguishedName = "cn=" + username + ",ou=Users,o=LIRR";
            string searchFilter = null;

            SearchRequest searchRequest = new SearchRequest(distinguishedName, searchFilter, SearchScope.Subtree, null);
            SearchResponse searchResponse = null;

            try
            {
                searchResponse = (SearchResponse)ldap.SendRequest(searchRequest);
            }
            catch (DirectoryOperationException ex)
            {
                _logger.Warn("LDAP SearchByDn. Dn:\"{0}\"; Error:{1}; Response:{2}; ResultCode:{3}",
                    distinguishedName, ex.Message, ex.Response.ErrorMessage, ex.Response.ResultCode);
                return null;
            }

            // check if user entry exists
            if (searchResponse != null)
            {
                if (searchResponse.Entries != null)
                {
                    if (searchResponse.Entries.Count == 0)
                    {
                        _logger.Warn("LDAP SearchByDn. SearchResponse.Entries.Count = 0 for Distinguished Name: {0}", distinguishedName);
                    }
                    else if (searchResponse.Entries.Count == 1)
                    {
                        return searchResponse.Entries[0];
                    }
                    else
                    {
                        _logger.Warn("LDAP SearchByDn. More than one entry found that matches Distinguished Name: {0}", distinguishedName);
                    }
                }
                else
                {
                    _logger.Warn("LDAP SearchByDn. SearchResponse.Entries = null for Distinguished Name: {0}", distinguishedName);
                }
            }
            else
            {
                _logger.Warn("LDAP SearchByDn. SearchResponse = null for Distinguished Name: {0}", distinguishedName);
            }

            return null;
        }
    }
}