using TEER.Helpers;
using TEER.Model;
using System;
using System.Configuration;
using System.Security.Principal;
using System.Web;


namespace TEER.ViewModel
{
    public class HeaderViewModel
    {
        public BaseUIViewModel BaseVM { get; private set; }

        public bool LoggedInUsernameVisible
        {
            get
            {
                return !string.IsNullOrEmpty(BaseVM.LoggedInUsername);
            }
        }

        public bool IsWindowsAuthentication
        {
            get
            {
                IPrincipal user = HttpContext.Current.User;

                if (user != null)
                {
                    if (string.IsNullOrEmpty(user.Identity.AuthenticationType)
                        || user.Identity.AuthenticationType.EqualsEx("ntlm")
                        || user.Identity.AuthenticationType.EqualsEx("negotiate"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public string EmployeeName
        {
            get
            {
                AppUser emp;
                // return if anonymous or not authenticated
                if (string.IsNullOrEmpty(BaseVM.LoggedInUsername))
                {
                    return null;
                }
                try
                {
                    emp = (AppUser)HttpContext.Current.Session["LoggedInUser"];
                }
                catch(Exception )
                {
                    return null;
                }

                if (emp == null)
                {

                    return null;
                }

                return string.Format("{0} {1}", emp.FirstName, emp.LastName);
            }
        }
        public string EmployeeInfo
        {
            get
            {
                AppUser emp;
                try
                {
                    emp = (AppUser)HttpContext.Current.Session["LoggedInUser"];
                }
                catch (Exception)
                {
                    return null;
                }

                if (emp == null)
                {
                    return null;
                }
                //return string.Format("{0} {1} ({2}) Logged in at {3:g}", emp.FirstName, emp.LastName, emp.BscEmployeeNumber, emp.LoggedInTime);
                return string.Format("{0} {1} ({2}) Logged in at {3:g}", emp.FirstName, emp.LastName, emp.BscEmployeeNumber, emp.LoggedInTime_DateString + " " + emp.LoggedInTime_TimeUserFormat);

            }
        }
        public string JobDescription
        {
            get
            {
                AppUser emp;
                if (HttpContext.Current.Session["LoggedInUser"] != null)
                {
                    try
                    {
                        emp = (AppUser)HttpContext.Current.Session["LoggedInUser"];
                    }
                    catch (Exception)
                    {
                        return EmployeeName;
                    }
                    if (emp.JobDescription == null)
                    {
                        return EmployeeName;
                    }
                    return emp.JobDescription;
                }
                else
                    return null;
            }
        }
        public string UserManual
        {
            get
            {
                return ConfigurationManager.AppSettings["userManualUrl"];
            }
        }
        public string YardCrewbookUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["roadYardCrewbookUrl"];
            }
        }
        public string PassangerCrewbookUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["passengerCrewbookUrl"];
            }
        }
        public string FAQPage
        {
            get
            {
                return ConfigurationManager.AppSettings["faqUrl"];
            }
        }
        public HeaderViewModel(BaseUIViewModel vm)
        {
            BaseVM = vm;
        }
    }
}