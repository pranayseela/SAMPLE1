using TEER.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace TEER.Model
{
    public class Employee
    {
        public string EmployeeNumber { get; set; }
        public string BscEmployeeNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Suffix { get; set; }
        public string FullName
        {
            get
            {
                return string.Format("{0}, {1}", LastName, FirstName);
            }
        }
        public string JobDescription { get; set; }
        public bool Active { get; set; }
        public DateTime StatusValidity { get; set; }
        public string Roles { get; set; }
        public string AgencyCode { get; set; }
        public string CraftCode { get; set;  }
    }
}