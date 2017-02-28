﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider
{
	public class ReportsNavigationProvider: IReportsNavigationProvider
	{
		private readonly IAuthorization _authorization;
		private readonly IReportsProvider _reportsProvider;
		private readonly IReportUrl _reportUrl;
		private readonly IToggleManager _toggleManager;

		public ReportsNavigationProvider(IAuthorization authorization, IReportsProvider reportsProvider, IReportUrl reportUrl, IToggleManager toggleManager)
		{
			_authorization = authorization;
			_reportsProvider = reportsProvider;
			_toggleManager = toggleManager;
			_reportUrl = reportUrl;
		}

		public IList<ReportNavigationItem> GetNavigationItems()
		{
			var reportsList = new List<ReportNavigationItem>();
			if (_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyReportWeb))
				reportsList.Add(new ReportNavigationItem
				{
					Action = "Index",
					Controller = "MyReport",
					Title = Resources.MyReport,
					IsWebReport = true
				});
			if (_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard))
			{
				reportsList.Add(new ReportNavigationItem
				{
					Action = "Index",
					Controller = "BadgeLeaderBoardReport",
					Title = Resources.BadgeLeaderBoardReport,
					Url = "/WFM/#/reports/leaderboard",
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
				reportsList.Add(new ReportNavigationItem
				{
					Url = _reportUrl.Build(applicationFunction.ForeignId),
					Title = applicationFunction.LocalizedFunctionDescription,
					Id = new Guid(applicationFunction.ForeignId)
				});
			}
			return reportsList;
		}
	}

}