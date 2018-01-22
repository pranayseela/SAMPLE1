using TEER.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Sockets;

namespace TEER.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index(HomeViewModel vm, IPrincipal user)
        {
            return View(vm);
        }
        
    }
}