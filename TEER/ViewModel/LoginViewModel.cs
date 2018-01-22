using TEER.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace TEER.ViewModel
{
    public class LoginViewModel : BaseUIViewModel
    {
        [StoreValueInCookie]
        [Required(ErrorMessage = "Enter your employee number")]
        public string EmployeeNumber { get; set; }

        [Required(ErrorMessage = "Enter your password")]
        public string Password { get; set; }

        public bool KeepMeSignedIn { get; set; }

        public bool Authenticated { get; set; }

        public string LoginErrorMessage { get; set; }

        public bool LoginCertified { get; set; }

        public string PasswordResetPage
        {
            get
            {
                return ConfigurationManager.AppSettings["passwordChangeUrl"];
            }
        }

        public string FAQPage
        {
            get
            {
                return ConfigurationManager.AppSettings["faqUrl"];
            }
        }

        public string Date { get; set; }
        public string Time { get; set; }

    }
}