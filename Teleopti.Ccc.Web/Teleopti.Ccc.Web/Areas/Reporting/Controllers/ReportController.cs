using System;
using System.Threading;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Ccc.Web.Areas.Reporting.Models;

namespace Teleopti.Ccc.Web.Areas.Reporting.Controllers
{
    public class ReportController : Controller
    {
        // GET: Reporting/Selection
        public ActionResult Index(Guid id)
        {
			  var commonReports =
		        new CommonReports(
			        ((TeleoptiIdentity) Thread.CurrentPrincipal.Identity).DataSource.Statistic.ConnectionString, id);
			  commonReports.LoadReportInfo();
			  var name = Resources.ResourceManager.GetString(commonReports.ResourceKey);
			  if (string.IsNullOrEmpty(name))
				  name = commonReports.Name;
			  if (id.Equals(new Guid("D1ADE4AC-284C-4925-AEDD-A193676DBD2F")) ||
	            id.Equals(new Guid("6A3EB69B-690E-4605-B80E-46D5710B28AF")))
		        return View("Adherence", new ReportModel {Id = id, Name = name});

            return View(new ReportModel{Id = id, Name = name});
        }
    }
}