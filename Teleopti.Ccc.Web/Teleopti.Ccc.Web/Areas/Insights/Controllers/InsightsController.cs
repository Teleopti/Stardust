﻿using System;
using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Insights.Models;

namespace Teleopti.Ccc.Web.Areas.Insights.Controllers
{
	public class InsightsController : ApiController
	{
		private readonly IReportProvider _reportProvider;
		private readonly Core.DataProvider.IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public InsightsController(IReportProvider reportProvider,
			Core.DataProvider.IPermissionProvider permissionProvider,
			ILoggedOnUser loggedOnUser)
		{
			_reportProvider = reportProvider;
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWork]
		[HttpGet, Route("api/Insights/Permission")]
		public virtual InsightsPermission GetPermission()
		{
			return _permissionProvider.GetInsightsPermission(_loggedOnUser.CurrentUser(), DateOnly.Today);
		}

		[UnitOfWork, TenantUnitOfWork]
		[HttpGet, Route("api/Insights/ReportConfig")]
		public virtual async Task<EmbedReportConfig> GetReportConfig(string reportId)
		{
			if (string.IsNullOrEmpty(reportId) || !Guid.TryParse(reportId, out var repId))
			{
				return new EmbedReportConfig();
			}

			return await _reportProvider.GetReportConfig(repId);
		}

		[UnitOfWork, TenantUnitOfWork]
		[HttpGet, Route("api/Insights/Reports")]
		public virtual async Task<ReportModel[]> GetReports()
		{
			return await _reportProvider.GetReports();
		}

		[TenantUnitOfWork]
		[HttpGet, Route("api/Insights/CreateReport")]
		public virtual async Task<EmbedReportConfig> CreateReport(string newReportName)
		{
			return await _reportProvider.CreateReport(newReportName);
		}

		[TenantUnitOfWork]
		[HttpGet, Route("api/Insights/UpdateReport")]
		public virtual async Task<bool> UpdateReportMetadata(string reportId, string reportName)
		{
			if (string.IsNullOrEmpty(reportId) || !Guid.TryParse(reportId, out var repId) ||
				string.IsNullOrWhiteSpace(reportName))
			{
				return false;
			}

			return await _reportProvider.UpdateReportMetadata(repId, reportName);
		}

		[TenantUnitOfWork]
		[HttpGet, Route("api/Insights/CloneReport")]
		public virtual async Task<EmbedReportConfig> CloneReport(string reportId, string newReportName)
		{
			if (string.IsNullOrEmpty(reportId) || !Guid.TryParse(reportId, out var repId))
			{
				return new EmbedReportConfig();
			}
			return await _reportProvider.CloneReport(repId, newReportName);
		}

		[TenantUnitOfWork]
		[HttpGet, Route("api/Insights/DeleteReport")]
		public virtual async Task<bool> DeleteReport(string reportId)
		{
			if (string.IsNullOrEmpty(reportId) || !Guid.TryParse(reportId, out var repId))
			{
				return false;
			}
			return await _reportProvider.DeleteReport(repId);
		}
	}
}
