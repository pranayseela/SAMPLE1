using System.Collections.Generic;
using System;
using TEER.Model;
using TEER.Helpers;
using System.Configuration;

namespace TEER.Model
{
    public class AppUser:Employee
    {
        public string Username { get; set; }
        public DateTime LoggedInTime { get; set; }
        public string UserTimeFormat
        {
            get
            {
                return ConfigurationManager.AppSettings[AgencyCode + "TimeFormat"];

            }
        }
        public string LoggedInTime_DateString
        {
            get
            {
                if (LoggedInTime != null)
                {
                    return LoggedInTime.Date.ToString("d");
                }
                return null;
            }
        }
        public string LoggedInTime_TimeUserFormat
        {
            get
            {
                //return time format base on user agency
                if (LoggedInTime != null)
                {
                    return Helper.TimeUserFormat(LoggedInTime);
                }
                return null;
            }
        }
    }
}