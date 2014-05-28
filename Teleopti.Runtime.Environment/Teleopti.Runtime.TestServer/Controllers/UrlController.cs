using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Teleopti.Runtime.TestServer.Controllers
{
    public class UrlController : Controller
    {
        //
        // GET: /Url/

        public ActionResult Index()
        {
            return Json(new {Url = "http://cccrc/TeleoptiCCC/"},JsonRequestBehavior.AllowGet);
        }

    }
}
