using TEER.Model;
using NLog;
using TEER.Security;

namespace TEER.Data
{

    public class APIClient
     {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public Employee GetEmployeeByBsc(string bscEmployeeNumber)
        {
            return AspNetRoleProvider.One.GetAppUser(null, bscEmployeeNumber);
        }
    }
}