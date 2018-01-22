using System;
using System.Collections.Generic;
using System.Configuration;
using TEER.Model;

namespace TEER.Authentication
{
    public abstract class LdapHelper : IDisposable
    {
        bool? _validateUsernamePassword = null, _checkLdapUserProperties = null;


        public bool ValidateUsernamePassword
        {
            get
            {
                if (_validateUsernamePassword == null)
                {
                    return ConfigurationManager.AppSettings["validateUsernamePassword"] == "1";
                }
                else
                {
                    return _validateUsernamePassword.Value;
                }
            }
        }

        public bool CheckLdapUserProperties
        {
            get
            {
                if (_checkLdapUserProperties == null)
                {
                    return ConfigurationManager.AppSettings["checkLDAPUserProperties"] == "1";

                }
                else
                {
                    return _checkLdapUserProperties.Value;
                }
            }
        }


        public LdapHelper()
        {

        }

        public LdapHelper(bool authenticatePassword)
        {
            _validateUsernamePassword = authenticatePassword;
            _checkLdapUserProperties = authenticatePassword;
        }


        public abstract LdapAuthResult Authenticate(
            string username,
            string password,
            Func<Employee> getUser);

        public abstract Dictionary<string, string> GetLdapProperties(string username);

        public abstract bool IsUserValidInLdap(string username, out string error);

        public abstract PasswordExpiryInfo GetPasswordExpiryInfo(string username);

        protected LdapAuthResult GetAuthResult(
            string username,
            bool result = false,
            string message = null,
            Employee employee = null)
        {
            // return ldap properties only on fail
            var ldapProperties = new Dictionary<string, string>();
            if (!result)
            {
                ldapProperties = GetLdapProperties(username);
            }

            var authResult = new LdapAuthResult
            {
                Valid = result,
                Message = message,
                LdapProperties = ldapProperties,
                User = employee
            };

            if (CheckLdapUserProperties)
            {
                var pei = GetPasswordExpiryInfo(username);
                if (pei != null)
                {
                    authResult.PasswordTimeToExpire = pei.TimeToExpire;
                }
                else
                {
                    // assume the password expires in the future
                    authResult.PasswordTimeToExpire = TimeSpan.MaxValue;
                }
            }
            else
            {
                // assume the password expires in the future
                authResult.PasswordTimeToExpire = TimeSpan.MaxValue;
            }

            return authResult;
        }

        public virtual void Dispose()
        {

        }


        public class LdapAuthResult
        {
            public bool Valid { get; set; }

            public string Message { get; set; }

            public Employee User { get; set; }

            public Dictionary<string, string> LdapProperties { get; set; }

            public TimeSpan PasswordTimeToExpire { get; set; }


            public LdapAuthResult()
            {
                LdapProperties = new Dictionary<string, string>();
            }
        }

        public class PasswordExpiryInfo
        {
            public DateTime PasswordExpiration { get; private set; }

            public TimeSpan TimeToExpire
            {
                get
                {
                    return PasswordExpiration - DateTime.Now;
                }
            }
            
            public PasswordExpiryInfo(DateTime passwordExpiration)
            {
                PasswordExpiration = passwordExpiration;
            }
        }
    }
}
