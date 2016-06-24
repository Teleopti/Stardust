using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.Reports.Models;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Reports.Core
{
	public class ReportNavigationProvider : IReportNavigationProvider
	{
		private readonly IReportsProvider _reportsProvider;
		private readonly IReportUrl _reportUrl;
		private readonly IAuthorization _authorization;
		private readonly IToggleManager _toggleManager;

		public ReportNavigationProvider(IReportsProvider reportsProvider, IReportUrl reportUrl, IAuthorization authorization, IToggleManager toggleManager)
		{
			_reportsProvider = reportsProvider;
			_reportUrl = reportUrl;
			_authorization = authorization;
			_toggleManager = toggleManager;
		}

		public IList<ReportItem> GetNavigationItems()
		{
			var grantedFuncs = _reportsProvider.GetReports().OrderBy(x => x.LocalizedFunctionDescription);

			var reportList = grantedFuncs.Select(applicationFunction => new ReportItem
			{
				Url = _reportUrl.Build(applicationFunction.ForeignId),
				Name = applicationFunction.LocalizedFunctionDescription,
			}).ToList();

			if (_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboardUnderReports) && _toggleManager.IsEnabled(Toggles.WfmReportPortal_LeaderBoard_39440))
			{
				reportList.Add(new ReportItem
				{
					Url = "reports/leaderboard",
					Name = Resources.BadgeLeaderBoardReport,
					IsWebReport = true
				});
			}

			return reportList ;
		}
	}
}