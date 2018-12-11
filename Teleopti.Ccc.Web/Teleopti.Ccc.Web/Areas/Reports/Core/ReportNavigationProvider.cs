using System;
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
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;

namespace Teleopti.Ccc.Web.Areas.Reports.Core
{
	public class ReportNavigationProvider : IReportNavigationProvider
	{
		private readonly IReportsProvider _reportsProvider;
		private readonly IReportUrl _reportUrl;
		private readonly IAuthorization _authorization;
		private readonly IReportNavigationModel _reportNavigationModel;
		private readonly IToggleManager _toggleManager;

		public ReportNavigationProvider(IReportsProvider reportsProvider, IReportUrl reportUrl, IAuthorization authorization, 
			IReportNavigationModel reportNavigationModel, IToggleManager toggleManager)
		{
			_reportsProvider = reportsProvider;
			_reportUrl = reportUrl;
			_authorization = authorization;
			_reportNavigationModel = reportNavigationModel;
			_toggleManager = toggleManager;
		}

		public IList<ReportItemViewModel> GetNavigationItemViewModels()
		{
			var grantedFuncs = _reportsProvider.GetReports().OrderBy(x => x.LocalizedFunctionDescription);

			var reportList = grantedFuncs.Select(applicationFunction => new ReportItemViewModel
			{
				Url = _reportUrl.Build(applicationFunction),
				Name = applicationFunction.LocalizedFunctionDescription,
				ForeignId = applicationFunction.ForeignId
			}).ToList();

			if (isPermittedLeaderBoardUnderReports())
			{
				reportList.Add(createLeaderBoardReportItem());
			}

			return reportList ;
		}

		public IList<CategorizedReportItemViewModel> GetCategorizedNavigationsItemViewModels()
		{
			var reportItems = getCategorizedReports()
				.SelectMany(reportCollection => reportCollection.ApplicationFunctions.Select(report =>
					new CategorizedReportItemViewModel
					{
						Url = _reportUrl.Build(report),
						Name = report.LocalizedFunctionDescription,
						Category = reportCollection.LocalizedDescription,
						IsWebReport = report.IsWebReport,
						ForeignId = report.ForeignId
					}))
				.ToList();

			if (isPermittedLeaderBoardUnderReports())
			{
				reportItems.Add(createLeaderBoardReportItem());
			}

			if (!_toggleManager.IsEnabled(Toggles.Wfm_AuditTrail_GenericAuditTrail_74938))
				reportItems = reportItems.Where(i => i.ForeignId != DefinedRaptorApplicationFunctionForeignIds.GeneralAuditTrailWebReport).ToList();

			return reportItems;
		}

		public IList<ReportNavigationItem> GetNavigationItems()
		{
			var reports = _reportsProvider.GetReports().OrderBy(x => x.LocalizedFunctionDescription);
			return reports.Select(applicationFunction => new ReportNavigationItem
			{
				Url = _reportUrl.Build(applicationFunction),
				Title = applicationFunction.LocalizedFunctionDescription,
				Id = new Guid(applicationFunction.ForeignId),
			}).ToList();
		}

		private bool isPermittedLeaderBoardUnderReports()
		{
			return _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboardUnderReports);
		}

		private CategorizedReportItemViewModel createLeaderBoardReportItem()
		{
			return new CategorizedReportItemViewModel
			{
				Url = "reports/leaderboard",
				Name = Resources.BadgeLeaderBoardReport,
				IsWebReport = true,
				Category = Resources.AgentPerformance
			};
		}

		private IEnumerable<IMatrixFunctionGroup> getCategorizedReports()
		{
			var reports = _reportNavigationModel.PermittedCategorizedReportFunctions.ToList();
			var customReports = _reportNavigationModel.PermittedCustomReportFunctions;
			reports.Add(new MatrixFunctionGroup
			{
				LocalizedDescription = Resources.CustomReports,
				ApplicationFunctions = customReports
			});

			return reports;
		}
	}
}