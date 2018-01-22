using TEER.Controllers;
using TEER.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace TEER
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            BundleTable.EnableOptimizations = false;

            ModelBinders.Binders.Add(typeof(IPrincipal), new PrincipalModelBinder());

            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
        }
        protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            // only access session state if it is available
            if (Context.Handler is IRequiresSessionState || Context.Handler is IReadOnlySessionState)
            {
                // if we are authenticated AND we dont have a session here.. redirect to login page.
                HttpCookie authenticationCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authenticationCookie != null)
                {
                    FormsAuthenticationTicket authenticationTicket = FormsAuthentication.Decrypt(authenticationCookie.Value);
                    if (!authenticationTicket.Expired)
                    {
                        // check if the session value that was set when the user logged in still exists
                        if (Session[BaseController.SESSION_KEY_USED_TO_CHECK_SESSION_TIMEOUT] == null)
                        {
                            // this means for some reason the session expired before the authentication ticket expired. Force a login.
                            FormsAuthentication.SignOut();

                            Response.Redirect(FormsAuthentication.LoginUrl, false);
                        }
                    }
                }
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            
        }
    }
}
