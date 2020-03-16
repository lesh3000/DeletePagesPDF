using System.Drawing;
using System.IO;
using System.Web;
using System.Web.Mvc;
using GdPicture14;
using GdPicture14.WEB;


namespace aspnet_mvc_razor_app.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Version = DocuViewareManager.GetVersion();
            return View();
        }

       
    }
}