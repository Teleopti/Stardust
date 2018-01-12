﻿using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Ccc.Web.Areas.Reporting.Models;
using Teleopti.Ccc.Web.Areas.Reports.Core;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Reporting.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.AccessToReports)]
	public class ReportController : Controller
	{
		private readonly IReportNavigationProvider _reportsNavigationProvider;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IAnalyticsPermissionsUpdater _analyticsPermissionsUpdater;
		private readonly ICommonReportsFactory _commonReportsFactory;

		private static readonly Guid[] adherenceReportIds =
		{
			new Guid("D1ADE4AC-284C-4925-AEDD-A193676DBD2F"),
			new Guid("6A3EB69B-690E-4605-B80E-46D5710B28AF")
		};

		public ReportController(IReportNavigationProvider reportsNavigationProvider,
			IPersonNameProvider personNameProvider,
			ILoggedOnUser loggedOnUser,
			ICurrentBusinessUnit currentBusinessUnit,
			IToggleManager toggleManager,
			IAnalyticsPermissionsUpdater analyticsPermissionsUpdater,
			ICommonReportsFactory commonReportsFactory)
		{
			_reportsNavigationProvider = reportsNavigationProvider;
			_personNameProvider = personNameProvider;
			_loggedOnUser = loggedOnUser;
			_currentBusinessUnit = currentBusinessUnit;
			_analyticsPermissionsUpdater = analyticsPermissionsUpdater;
			_commonReportsFactory = commonReportsFactory;
		}

		public ActionResult Index(Guid? id)
		{
			if (!id.HasValue)
			{
				return View("Empty");
			}

			var reportsItems = _reportsNavigationProvider.GetNavigationItems();

			var guids = reportsItems.Select(item => item.Id).ToArray();
			if (!id.Value.Equals(Guid.Empty) && !guids.Contains(id.Value))
			{
				return View("NoPermission");
			}

			var reportContext = GetReportContext();
			_analyticsPermissionsUpdater.Handle(reportContext.PersonId, reportContext.BusinessUnitId);

			using (var commonReports = _commonReportsFactory.CreateAndLoad(
				((TeleoptiIdentity) Thread.CurrentPrincipal.Identity).DataSource.Analytics.ConnectionString, id.Value))
			{
				var name = "";
				if (!string.IsNullOrEmpty(commonReports.ResourceKey))
					name = Resources.ResourceManager.GetString(commonReports.ResourceKey, CultureInfo.CurrentUICulture);
				if (string.IsNullOrEmpty(name))
					name = commonReports.Name;
				var helpUrl = string.Format(CultureInfo.InvariantCulture, "{0}/{1}",
					ConfigurationManager.AppSettings["HelpUrlOnline"], commonReports.HelpKey);

				if (adherenceReportIds.Contains(id.Value))
				{
					return View("Adherence",
						new ReportModel
						{
							Id = id.Value,
							Name = name,
							ReportNavigationItems = reportsItems,
							HelpUrl = helpUrl,
							CurrentLogonAgentName = reportContext.PersonName,
							CurrentBuName = reportContext.BusinessUnitName
						});
				}

				return View(new ReportModel
					{
						Id = id.Value,
						Name = name,
						ReportNavigationItems = reportsItems,
						HelpUrl = helpUrl,
						CurrentLogonAgentName = reportContext.PersonName,
						CurrentBuName = reportContext.BusinessUnitName,
						UseOpenXml = true
					});
			}
		}

		[UnitOfWork]
		protected virtual ReportContext GetReportContext()
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var currentBusinessUnit = _currentBusinessUnit.Current();
			return new ReportContext
			{
				BusinessUnitId = currentBusinessUnit.Id.GetValueOrDefault(),
				PersonId = currentUser.Id.GetValueOrDefault(),
				BusinessUnitName = currentBusinessUnit.Name,
				PersonName = _personNameProvider.BuildNameFromSetting(currentUser.Name)
			};
		}

		protected class ReportContext
		{
			public Guid BusinessUnitId { get; set; }
			public Guid PersonId { get; set; }
			public string BusinessUnitName { get; set; }
			public string PersonName { get; set; }
		}
	}
}