using System.Web.Mvc;
using TEER.ViewModel;

namespace TEER.Controllers
{
    public class ReportsController : BaseController
    {
        // GET: Reports
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Reports(ReportsViewModel vm)
        {
            vm.viewReports = "This is the Reports Page.";
            return View(vm);
        }
    }
}