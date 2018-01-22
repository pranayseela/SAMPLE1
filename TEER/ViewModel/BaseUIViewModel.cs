using TEER.Model;
using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using TEER.Validation;

namespace TEER.ViewModel
{

    public class BaseUIViewModel
    {
        public ValidationState ValidationState { get; private set; }
        public string AutoLogoutIntervalSeconds { get; private set; }
        public string AutoLogoutEnabled { get; private set; }
        public string AutoLogoutTimeoutSeconds { get; private set; }
        public string RunningEnv { get; private set; }
        public string LoggedInUsername
        {
            get
            {
                return HttpContext.Current.User.Identity.Name;
            }
        }
        //public INotificationViewModel NotificationVM { get; private set; }
        public HeaderViewModel HeaderVM { get; private set; }
        public MenuViewModel MenuVM { get; private set; }
        public string PageTitle { get; set; }
        public bool UseValuesFromCookie { get; set; }
        public bool IsRedirectedToConfirmOnDutyPage { get; set; }
        public bool IsUserOffDuty { get; set; }
        public BaseUIViewModel()
        {

            HeaderVM = new HeaderViewModel(this);
            //NotificationVM = new NotificationViewModel();
            MenuVM = new MenuViewModel();
            ValidationState = new ValidationState();
            UseValuesFromCookie = true;

            AutoLogoutTimeoutSeconds = ConfigurationManager.AppSettings["AutoLogoutTimeoutSeconds"];
            AutoLogoutEnabled = ConfigurationManager.AppSettings["AutoLogoutEnabled"];
            AutoLogoutIntervalSeconds = ConfigurationManager.AppSettings["AutoLogoutIntervalSeconds"];
            RunningEnv = ConfigurationManager.AppSettings["Environment"];

            IsUserOffDuty = false;
        }

        public void ConfrimedonDuty(bool confirmed)
        {
            HttpContext.Current.Session["AcceptedDisclaimer"] = confirmed;
            IsRedirectedToConfirmOnDutyPage = confirmed;
        }

        public DateTime DateTimeNow
        {
            get
            {
                //if user set the test login date and time, return calculated system time
                if (HttpContext.Current.Session["UserLoginData"] != null)
                {
                    LoginUserData userLoginData = (LoginUserData)HttpContext.Current.Session["UserLoginData"];
                    return userLoginData.SystemTimeUtc.ToLocalTime();
                }
                else
                {
                    return DateTime.Now;
                }
            }
        }
        
        public void ShowErrorMessage(IEnumerable<string> messages, bool? showDialog = null, string dialogTitle = null, int? dialogWidth = null, int? dialogHeight = null, bool dialogResizable = false)
        {
            //NotificationVM.ShowMessages(messages, NotificationType.Error, showDialog, dialogTitle, dialogWidth, dialogHeight, dialogResizable);
        }
       
    }
}