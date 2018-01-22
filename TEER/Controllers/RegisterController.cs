using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.Mvc;
using TEER.ViewModel;

namespace TEER.Controllers
{
    public class RegisterController : BaseController
    {
        // GET: Register
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult RegisterSheet(RegisterViewModel vm)
        {
            vm.ipAddress = GetLocalIPAddress();
            return View(vm);
        }
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}