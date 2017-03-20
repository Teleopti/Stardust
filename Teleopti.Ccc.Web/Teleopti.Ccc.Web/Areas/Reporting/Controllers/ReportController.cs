using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Ccc.Web.Areas.Reporting.Models;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Reporting.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.AccessToReports)]
	public class ReportController : Controller
	{
		private readonly IReportsNavigationProvider _reportsNavigationProvider;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IToggleManager _toggleManager;
		private readonly IAnalyticsPermissionsUpdater _analyticsPermissionsUpdater;
		private readonly ICommonReportsFactory _commonReportsFactory;

		private static readonly Guid[] adherenceReportIds = { new Guid("D1ADE4AC-284C-4925-AEDD-A193676DBD2F"), new Guid("6A3EB69B-690E-4605-B80E-46D5710B28AF") };

		public ReportController(IReportsNavigationProvider reportsNavigationProvider,
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
			_toggleManager = toggleManager;
			_analyticsPermissionsUpdater = analyticsPermissionsUpdater;
			_commonReportsFactory = commonReportsFactory;
		}

		[UnitOfWork]
		[AnalyticsUnitOfWork]
		public virtual ActionResult Index(Guid? id)
		{
			if (!id.HasValue)
				return View("Empty");
			var reportsItems = _reportsNavigationProvider.GetNavigationItems();

			var guids = reportsItems.Select(item => item.Id).ToArray();
			if(!id.Value.Equals(Guid.Empty) && !guids.Contains(id.Value))
				return View("NoPermission");
			var agentName = _personNameProvider.BuildNameFromSetting(_loggedOnUser.CurrentUser().Name);
			var buName = _currentBusinessUnit.Current().Name;

			_analyticsPermissionsUpdater.Handle(_loggedOnUser.CurrentUser().Id.GetValueOrDefault(), _currentBusinessUnit.Current().Id.GetValueOrDefault());

			using (var commonReports = _commonReportsFactory.CreateAndLoad(((TeleoptiIdentity)Thread.CurrentPrincipal.Identity).DataSource.Analytics.ConnectionString, id.Value))
			{
				var name = "";
				if (!string.IsNullOrEmpty(commonReports.ResourceKey))
					name = Resources.ResourceManager.GetString(commonReports.ResourceKey, CultureInfo.CurrentUICulture);
				if (string.IsNullOrEmpty(name))
					name = commonReports.Name;
				var helpUrl = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", ConfigurationManager.AppSettings["HelpUrlOnline"], commonReports.HelpKey);

				if (adherenceReportIds.Contains(id.Value))
					return View("Adherence",
						new ReportModel
						{
							Id = id.Value,
							Name = name,
							ReportNavigationItems = reportsItems,
							HelpUrl = helpUrl,
							CurrentLogonAgentName = agentName,
							CurrentBuName = buName
						});

				return
					View(new ReportModel
					{
						Id = id.Value,
						Name = name,
						ReportNavigationItems = reportsItems,
						HelpUrl = helpUrl,
						CurrentLogonAgentName = agentName,
						CurrentBuName = buName,
						UseOpenXml = _toggleManager.IsEnabled(Toggles.Report_UseOpenXmlFormat_35797)
					});
			}
		}
	}
}