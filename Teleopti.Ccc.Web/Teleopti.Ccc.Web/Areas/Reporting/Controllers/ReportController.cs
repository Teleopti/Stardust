﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Ccc.Web.Areas.Reporting.Models;

namespace Teleopti.Ccc.Web.Areas.Reporting.Controllers
{
    public class ReportController : Controller
    {
	    private readonly IReportsNavigationProvider _reportsNavigationProvider;

	    public ReportController(IReportsNavigationProvider reportsNavigationProvider)
	    {
		    _reportsNavigationProvider = reportsNavigationProvider;
	    }

	    // GET: Reporting/Selection
        public ActionResult Index(Guid id)
        {
			  var reportsItems = _reportsNavigationProvider.GetNavigationItems();
			  
			  var commonReports =
		        new CommonReports(
			        ((TeleoptiIdentity) Thread.CurrentPrincipal.Identity).DataSource.Statistic.ConnectionString, id);
			  commonReports.LoadReportInfo();
			  var name = Resources.ResourceManager.GetString(commonReports.ResourceKey);
			  if (string.IsNullOrEmpty(name))
				  name = commonReports.Name;
			  if (id.Equals(new Guid("D1ADE4AC-284C-4925-AEDD-A193676DBD2F")) ||
	            id.Equals(new Guid("6A3EB69B-690E-4605-B80E-46D5710B28AF")))
				  return View("Adherence", new ReportModel { Id = id, Name = name, ReportNavigationItems = reportsItems });

			  return View(new ReportModel { Id = id, Name = name, ReportNavigationItems = reportsItems });
        }
    }
}