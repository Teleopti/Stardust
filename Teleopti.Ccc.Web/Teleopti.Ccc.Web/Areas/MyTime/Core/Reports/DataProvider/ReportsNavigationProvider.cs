using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider
{
	public class ReportsNavigationProvider: IReportsNavigationProvider
	{
		private readonly IPrincipalAuthorization _principalAuthorization;
		private readonly IReportsProvider _reportsProvider;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IToggleManager _toggleManager;

		public ReportsNavigationProvider(IPrincipalAuthorization principalAuthorization, IReportsProvider reportsProvider, ISessionSpecificDataProvider sessionSpecificDataProvider, IToggleManager toggleManager)
		{
			_principalAuthorization = principalAuthorization;
			_reportsProvider = reportsProvider;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_toggleManager = toggleManager;
		}

		public IList<ReportNavigationItem> GetNavigationItems()
		{
			var data = _sessionSpecificDataProvider.GrabFromCookie();
			var matrixWebsiteUrl = ConfigurationManager.AppSettings["MatrixWebSiteUrl"];
			if (!string.IsNullOrEmpty(matrixWebsiteUrl) && !matrixWebsiteUrl.EndsWith("/"))
				matrixWebsiteUrl += "/";
			var realBu = data.BusinessUnitId;
			var reportsList = new List<ReportNavigationItem>();
			if (_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb))
				reportsList.Add(new ReportNavigationItem
				{
					Action = "Index",
					Controller = "MyReport",
					Title = Resources.MyReport,
					IsWebReport = true
				});
			if (_toggleManager.IsEnabled(Toggles.Badge_Leaderboard_30584) && _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard))
			{
				reportsList.Add(new ReportNavigationItem
				{
					Action = "Index",
					Controller = "BadgeLeaderBoardReport",
					Title = Resources.BadgeLeaderBoardReport,
					IsWebReport = true
				});
			}
			var otherReports = _reportsProvider.GetReports().OrderBy(x => x.LocalizedFunctionDescription);
			if (otherReports.Any()&& reportsList.Any())
				reportsList.Add(new ReportNavigationItem
				{
					IsDivider = true
				});
			foreach (var applicationFunction in otherReports)
			{
				var url = string.Format(CultureInfo.CurrentCulture, "{0}Selection.aspx?ReportId={1}&BuId={2}",
					matrixWebsiteUrl, applicationFunction.ForeignId, realBu);

				reportsList.Add(new ReportNavigationItem
				{
					Url = url,
					Title = applicationFunction.LocalizedFunctionDescription,
				});
			}
			return reportsList;
		}
	}

}