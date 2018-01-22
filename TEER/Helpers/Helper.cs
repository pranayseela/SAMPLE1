using TEER.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEER.Helpers
{
    public class Helper
    {
        public static readonly string DATE_FORMAT = "MM/dd/yyyy";
        public static readonly string DATE_TIME24_FORMAT = "M/d/yyyy H:m";
        //public static readonly string DATE_TIME24_FORMAT_display = "MM/dd/yyyy HH:mm";
        public static readonly string DATE_TIME24_FORMAT_SS = "MM/dd/yyyy HH:mm:ss";
        public static readonly string DATE_TIME12_FORMAT = "M/d/yyyy h:m tt";
        //public static readonly string DATE_TIME12_FORMAT_display = "MM/dd/yyyy hh:mm tt";
        public static readonly string DATE_TIME12_FORMAT_SS = "MM/dd/yyyy hh:mm:ss tt";
        public static readonly string TIME12_FORMAT = "hh:mm tt";
        public static readonly string TIME24_FORMAT = "HH:mm";
        //
        public static readonly string EFF_DATE_FORMAT = DATE_FORMAT;
        public static readonly string EFF_DATE_TIME_FORMAT = DATE_TIME12_FORMAT;
        public static readonly string EFF_DATE_TIME_FORMAT_SS = DATE_TIME12_FORMAT_SS;
        public static readonly string EFF_TIME_FORMAT = TIME12_FORMAT;
        //
        public static T GetValue<T>(object value)
        {
            Type type = typeof(T);

            if (value == DBNull.Value || value == null || string.IsNullOrEmpty(Convert.ToString(value)))
            {
                
                return default(T);
                               
            }

            if (IsNullableType(type))
            {
                // GET THE UNDERLYING TYPE
                //t = typeof(T).GetGenericArguments()[0];
                type = Nullable.GetUnderlyingType(typeof(T));
            }

            return (T)Convert.ChangeType(value, type);
        }
        public static string TimeUserFormat(DateTime dt)
        {
            AppUser user = (AppUser)System.Web.HttpContext.Current.Session["LoggedInUser"];
            
            if (user != null && dt != null)
            {
                if (user.UserTimeFormat == "TIME24_FORMAT")
                {
                    return dt.ToString(TIME24_FORMAT);
                }
                //else if (user.UserTimeFormat == null || user.UserTimeFormat == "TIME12_FORMAT")
                //prevent incorrect configuration to crash the system
                else
                {
                    return dt.ToString(TIME12_FORMAT);
                }
            }
            return null;
        }
        //
        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
        public static DateTime? ConvertToDateTime(string date, string time = "00:00")
        {
            if (string.IsNullOrEmpty(time))
            {
                return null;
            }
            
            DateTime dateTime = DateTime.MinValue;
            bool valid = false;
            string dateTimeToParse = (date + " " + time).Trim();

            // try parsing with 24 format. if that fails then by 12 format
            valid = DateTime.TryParseExact(dateTimeToParse, Helper.DATE_TIME24_FORMAT, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out dateTime);
            if (!valid)
            {
                valid = DateTime.TryParseExact(dateTimeToParse, Helper.DATE_TIME12_FORMAT, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out dateTime);
            }

            if (valid)
            {
                return dateTime;
            }
            else
            {
                return null;
            }
        }
        //public static DateTime MinAllowedFirstTime1From(DateTime date)
        //{
        //    return date.Date.AddDays(-1);
        //}
    }
}