using System;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Ccc.Web.Areas.Reporting.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Reporting.Controllers
{
    public class SelectionController : Controller
    {
        // GET: Reporting/Selection
        public ActionResult Report(Guid id)
        {
			  var commonReports =
		        new CommonReports(
			        ((TeleoptiIdentity) Thread.CurrentPrincipal.Identity).DataSource.Statistic.ConnectionString, id);
			  commonReports.LoadReportInfo();
			  var name = Resources.ResourceManager.GetString(commonReports.ResourceKey);
			  if (string.IsNullOrEmpty(name))
				  name = commonReports.Name;
            return View(new ReportModel{Id = id, Name = name});
        }
    }
}