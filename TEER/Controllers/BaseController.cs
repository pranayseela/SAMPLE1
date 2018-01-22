using TEER.Data;
using TEER.Helpers;
using TEER.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace TEER.Controllers
{
    public class BaseController : Controller
    {
        public static readonly string SESSION_LAST_UNHANDLED_EXCEPTION = "SESSION_UNHANDLED_EXCEPTION";
        public static readonly string SESSION_KEY_USED_TO_CHECK_SESSION_TIMEOUT = "SESSION_KEY_USED_TO_CHECK_SESSION_TIMEOUT";
        public static readonly string SESSION_PASSWORD_TIME_TO_EXPIRE = "SESSION_PASSWORD_TIME_TO_EXPIRE";
        public static readonly string SESSION_EMPLOYEE_FLAGS = "EmployeeFlagsInSession";

        public Dal Dal { get; private set; }
        public BaseController()
        {
            Dal = new Dal();
        }
        public AppUser LoggedInUser
        {
            get
            {
                return HttpContext.Session["LoggedInUser"] as AppUser;
            }
            set
            {
                HttpContext.Session["LoggedInUser"] = value;
            }
        }
        public LoginUserData LoginUserData
        {
            get
            {
                return (LoginUserData)Session["UserLoginData"];

            }
        }
        public DateTime DateTimeNow
        {
            get
            {
                return LoginUserData.SystemTimeUtc.ToLocalTime();

            }
        }
        public TimeSpan PasswordTimeToExpire
        {
            get
            {
                return Helper.GetValue<TimeSpan>(Session[SESSION_PASSWORD_TIME_TO_EXPIRE]);
            }
            set
            {
                Session[SESSION_PASSWORD_TIME_TO_EXPIRE] = value;
            }
        }
        internal HttpCookie CreateNewFormsAuthCookie(string username, string userData)
        {
            // create the ticket, store the login time in the ticket
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(version: 2,
                                                   name: username,
                                                   issueDate: DateTime.Now,
                                                   expiration: DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
                                                   isPersistent: false,
                                                   // null seems to be causing a problem. use string.Empty instead
                                                   userData: string.IsNullOrEmpty(userData) ? string.Empty : userData);

            // create a cookie with the encrypted ticket
            string hash = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);

            // cookie is not accessible by client-side script.
            cookie.HttpOnly = true;

            return cookie;
        }
        public void StoreValuesInCookie(params object[] viewmodels)
        {
            string controllerName = ControllerContext.RouteData.Values["controller"].ToString();
            string actionName = ControllerContext.RouteData.Values["action"].ToString();

            foreach (var viewmodel in viewmodels)
            {
                Type type = viewmodel.GetType();

                // only take properties with [StoreValueInCookie] attribute defined
                var propsInfo = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(p => p.IsDefined(typeof(StoreValueInCookieAttribute), true));

                if (propsInfo.Count() == 0)
                {
                    continue;
                }

                string cookieName = string.Format("{0}-{1}-{2}-{3}-{4}.model-values",
                                        FormsAuthentication.FormsCookieName,
                                        User.Identity.Name,
                                        controllerName,
                                        actionName,
                                        type.FullName);

                HttpCookie cookie = new HttpCookie(cookieName);
                cookie.Expires = DateTime.Now.AddMonths(6);
                cookie.HttpOnly = true;

                foreach (PropertyInfo pi in propsInfo)
                {
                    var value = pi.GetValue(viewmodel, null);

                    Type propertyType = null;
                    if (Helper.IsNullableType(pi.PropertyType))
                    {
                        // GET THE UNDERLYING TYPE
                        //propertyType = pi.PropertyType.GetGenericArguments()[0];
                        propertyType = Nullable.GetUnderlyingType(pi.PropertyType);
                    }
                    else
                    {
                        propertyType = pi.PropertyType;
                    }

                    if (value == null)
                    {
                        cookie.Values.Add(pi.Name, null);
                    }
                    else
                    {
                        if (typeof(IEnumerable<object>).IsAssignableFrom(propertyType))
                        {
                            cookie.Values.Add(pi.Name, string.Join("|", (IEnumerable<object>)value));
                        }
                        else
                        {
                            cookie.Values.Add(pi.Name, value.ToString());
                        }
                    }
                }

                if (cookie.Values.Keys.Count == 0)
                {
                    cookie.Expires = DateTime.Today.AddDays(-1);
                }
                else
                {
                    Response.Cookies.Add(cookie);
                }
            }
        }
        public void RestoreValuesFromCookie(params object[] viewmodels)
        {
            string controllerName = ControllerContext.RouteData.Values["controller"].ToString();
            string actionName = ControllerContext.RouteData.Values["action"].ToString();

            foreach (var viewmodel in viewmodels)
            {
                Type type = viewmodel.GetType();

                string cookieName = string.Format("{0}-{1}-{2}-{3}-{4}.model-values",
                                        FormsAuthentication.FormsCookieName,
                                        User.Identity.Name,
                                        controllerName,
                                        actionName,
                                        type.FullName);

                HttpCookie cookie = Request.Cookies[cookieName];

                if (cookie == null || cookie.Values == null || cookie.Values.Count == 0)
                {
                    continue;
                }

                // only take properties with [StoreValueInCookie] attribute defined
                var propsInfo = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(p => p.IsDefined(typeof(StoreValueInCookieAttribute), true));

                if (propsInfo.Count() == 0)
                {
                    continue;
                }

                foreach (PropertyInfo pi in propsInfo)
                {
                    // no public set
                    if (pi.GetSetMethod() == null)
                    {
                        continue;
                    }

                    string value = cookie[pi.Name];
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    Type propertyType = null;
                    if (Helper.IsNullableType(pi.PropertyType))
                    {
                        // GET THE UNDERLYING TYPE
                        //propertyType = pi.PropertyType.GetGenericArguments()[0];
                        propertyType = Nullable.GetUnderlyingType(pi.PropertyType);
                    }
                    else
                    {
                        propertyType = pi.PropertyType;
                    }

                    try
                    {
                        if (typeof(IEnumerable<object>).IsAssignableFrom(propertyType))
                        {
                            // array type. todo
                        }
                        else if (propertyType == typeof(bool))
                        {
                            bool boolValue;
                            if (bool.TryParse(value, out boolValue))
                            {
                                pi.SetValue(viewmodel, boolValue, null);
                            }
                        }
                        else if (propertyType == typeof(int))
                        {
                            int intValue;
                            if (int.TryParse(value, out intValue))
                            {
                                pi.SetValue(viewmodel, intValue, null);
                            }
                        }
                        else if (propertyType == typeof(double))
                        {
                            double doubleValue;
                            if (double.TryParse(value, out doubleValue))
                            {
                                pi.SetValue(viewmodel, doubleValue, null);
                            }
                        }
                        else if (propertyType == typeof(decimal))
                        {
                            decimal decimalValue;
                            if (decimal.TryParse(value, out decimalValue))
                            {
                                pi.SetValue(viewmodel, decimalValue, null);
                            }
                        }
                        else if (typeof(string).IsAssignableFrom(propertyType))
                        {
                            pi.SetValue(viewmodel, value, null);
                        }
                        else
                        {
                            // not supported
                        }
                    }
                    catch
                    {
                        //ignore
                    }
                }
            }
        }
    }
}