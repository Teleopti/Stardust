using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.Reports.Models;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Reports.Core
{
	public class ReportNavigationProvider : IReportNavigationProvider
	{
		private readonly IReportsProvider _reportsProvider;
		private readonly IReportUrl _reportUrl;
		private readonly IAuthorization _authorization;
		private readonly IToggleManager _toggleManager;
		private readonly IReportNavigationModel _reportNavigationModel;

		public ReportNavigationProvider(IReportsProvider reportsProvider, IReportUrl reportUrl, IAuthorization authorization,
			IToggleManager toggleManager, IReportNavigationModel reportNavigationModel)
		{
			_reportsProvider = reportsProvider;
			_reportUrl = reportUrl;
			_authorization = authorization;
			_toggleManager = toggleManager;
			_reportNavigationModel = reportNavigationModel;
		}

		public IList<ReportItem> GetNavigationItems()
		{
			var grantedFuncs = _reportsProvider.GetReports().OrderBy(x => x.LocalizedFunctionDescription);

			var reportList = grantedFuncs.Select(applicationFunction => new ReportItem
			{
				Url = _reportUrl.Build(applicationFunction.ForeignId),
				Name = applicationFunction.LocalizedFunctionDescription,
			}).ToList();

			if (isPermittedLeaderBoardUnderReports())
			{
				reportList.Add(createLeaderBoardReportItem());
			}

			return reportList ;
		}

		public IList<CategorizedReportItem> GetCategorizedNavigationsItems()
		{
			var reportItems = getCategorizedReports()
				.SelectMany(reportCollection => reportCollection.ApplicationFunctions.Select(report => new CategorizedReportItem
				{
					Url = _reportUrl.Build(report.ForeignId),
					Name = report.LocalizedFunctionDescription,
					Category = reportCollection.LocalizedDescription,
				}))
				.ToList();

			if (isPermittedLeaderBoardUnderReports())
			{
				var leaderboardReportItem = createLeaderBoardReportItem();
				reportItems.Add(new CategorizedReportItem
				{
					Url = leaderboardReportItem.Url,
					Name = leaderboardReportItem.Name,
					IsWebReport = leaderboardReportItem.IsWebReport
				});
			}

			return reportItems;
		}

		private bool isPermittedLeaderBoardUnderReports()
		{
			return _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboardUnderReports) &&
				   _toggleManager.IsEnabled(Toggles.WfmReportPortal_LeaderBoard_39440);
		}

		private ReportItem createLeaderBoardReportItem()
		{
			return new CategorizedReportItem
			{
				Url = "reports/leaderboard",
				Name = Resources.BadgeLeaderBoardReport,
				IsWebReport = true,
			};
		}

		private IEnumerable<IMatrixFunctionGroup> getCategorizedReports()
		{
			var reports = _reportNavigationModel.PermittedCategorizedReportFunctions.ToList();
			var realTimeReports = _reportNavigationModel.PermittedRealTimeReportFunctions;
			var customReports = _reportNavigationModel.PermittedCustomReportFunctions;

			reports.Add(new MatrixFunctionGroup() { LocalizedDescription = Resources.CustomReports, ApplicationFunctions = customReports });
			reports.Add(new MatrixFunctionGroup() { LocalizedDescription = Resources.RealTime, ApplicationFunctions = realTimeReports });

			return reports;
		}
	}

}