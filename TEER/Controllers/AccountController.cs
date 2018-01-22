using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TEER.ViewModel;
using System.Configuration;
using System.Web.Security;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using NLog;
using TEER.Model;
using TEER.Authentication;
using TEER.Helpers;

namespace TEER.Controllers
{
    public class AccountController : BaseController
    {
        private static readonly Logger _logger = LogManager.GetLogger("AccountController");
        PrincipalContext pc = null;
        private string DomainOrServerName
        {
            get
            {
                return ConfigurationManager.AppSettings["domainOrServerName"];
            }
        }
        // GET: Account
        public ActionResult Index()
        {
            LoginViewModel vm = new LoginViewModel();
            System.Web.HttpContext.Current.Session["UserData"] = null;
            System.Web.HttpContext.Current.Session["AcceptedDisclaimer"] = null;
            System.Web.HttpContext.Current.Session["ConfirmedLoginContinue"] = null;

            if (Request.IsAjaxRequest())
                return View(vm);
            else
                return View(vm);
        }

        [HttpGet]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            //clear Session cache
            Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Index(LoginViewModel vm, string returnUrl)
        {
            LoggedInUser = null;
            DateTime? loginDateTime = null;
            if (ModelState.IsValid)
            {
                string username = vm.EmployeeNumber.Trim().ToUpper();
                string password = vm.Password.Trim();
               

                LdapHelper.LdapAuthResult ldapResult = null;

                //bypass user authentication in programming mode
                using (LdapHelper ldap = new LirrLdapHelper(false))
                //using (LdapHelper ldap = new LirrLdapHelper(true))
                {
                    ldapResult = ldap.Authenticate(
                        username: username,
                        password: password,
                        getUser: () =>
                        {
                            return Dal.GetEmployeeByBsc(username);
                        });
                }


                if (!ldapResult.Valid)
                {
                    if (ldapResult.PasswordTimeToExpire.TotalSeconds < 0)
                    {
                        //Access BaseUI and show the reset password popup
                        vm.LoginErrorMessage = ldapResult.Message;
                        return View(vm);
                    }
                    else
                    {
                        vm.LoginErrorMessage = ldapResult.Message;
                        return View(vm);
                    }
                }

                if (vm.Date != null && vm.Time != null)
                {
                    loginDateTime = Helper.ConvertToDateTime(vm.Date, vm.Time);
                    
                }
                else
                {
                    loginDateTime = DateTime.Now;
                }

                if (loginDateTime > DateTime.Now)
                {
                    vm.LoginErrorMessage = ("Login time cannot be in the future");
                    return View(vm);
                }


                //// this is used to detect a session timeout and redirect the user to the login page
                //// See Global.asax.cs > Application_PreRequestHandlerExecute
                Session[SESSION_KEY_USED_TO_CHECK_SESSION_TIMEOUT] = DateTime.Now;

                FormsAuthentication.SetAuthCookie(vm.EmployeeNumber.ToUpper(), vm.KeepMeSignedIn);

                PasswordTimeToExpire = ldapResult.PasswordTimeToExpire;
                AppUser User = new AppUser()
                {
                    Username = ldapResult.User.FullName,
                    EmployeeNumber = ldapResult.User.EmployeeNumber,
                    //LoggedInTime = DateTime.Now,
                    LoggedInTime = (DateTime)loginDateTime,
                    BscEmployeeNumber = ldapResult.User.BscEmployeeNumber,
                    FirstName = ldapResult.User.FirstName,
                    LastName = ldapResult.User.LastName,
                    //JobDescription = ldapResult.User.JobCodeDescr,
                    JobDescription = ldapResult.User.JobDescription,
                    AgencyCode = ldapResult.User.AgencyCode,
                    //below for testing only
                    //AgencyCode = "LIRR",
                    CraftCode = ldapResult.User.CraftCode
                };
                //Set the Logged in user session to Employee object in BaseController          
                LoggedInUser = User;

                //string identityUsername = ldapResult.User.EmployeeNumber;
                string identityUsername = ldapResult.User.BscEmployeeNumber;

                //// log the user login event for auditing purposes
                Dal.SaveLoginInfo(username, Request.ServerVariables["REMOTE_HOST"]);

                DateTime systemTimeUtc = DateTime.Now.ToUniversalTime();
                LoginUserData userData = new LoginUserData
                {
                    SystemTimeUtcAtLogin = systemTimeUtc,
                    //LoginTimeUtc = systemTimeUtc,
                    LoginTimeUtc = loginDateTime.Value.ToUniversalTime(),
                };
                System.Web.HttpContext.Current.Session["UserLoginData"] = userData;

                HttpCookie cookie = CreateNewFormsAuthCookie(identityUsername, null);

                //// add cookie to response and redirect
                Response.Cookies.Add(cookie);

                _logger.Info("{0}: User Logged in Successfully", identityUsername);

                return RedirectToAction("Index", "Home");
            }
            return View(vm);
        }
        private void LogFailError(string message, LdapHelper.LdapAuthResult ldapResult = null)
        {
            string username = ldapResult.User.FirstName.Trim().ToUpper() + " " + ldapResult.User.LastName.Trim().ToUpper();
            _logger.Warn("Login failed for {0}. {1}. {2}", username, message);
        }
    }
}