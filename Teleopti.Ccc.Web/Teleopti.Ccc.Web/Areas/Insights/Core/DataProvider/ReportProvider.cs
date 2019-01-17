using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Insights.Models;

namespace Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider
{
	public class ReportProvider : IReportProvider
	{
		public const string TemplateReportName = "__WFM_Insights_Report_Template__";
		public const string UsageReportName = "Report Usage Metrics Report";
		private readonly IApplicationConfigurationDbProvider _appConfig;
		private readonly IPowerBiClientFactory _powerBiClientFactory;
		private static readonly ILog logger = LogManager.GetLogger(typeof(ReportProvider));

		public ReportProvider(IApplicationConfigurationDbProvider appConfig, IPowerBiClientFactory powerBiClientFactory)
		{
			_appConfig = appConfig;
			_powerBiClientFactory = powerBiClientFactory;
		}

		public async Task<ReportModel[]> GetReports()
		{
			var excludedReports = new[] {UsageReportName, TemplateReportName};
			using (var client = await _powerBiClientFactory.CreatePowerBiClient())
			{
				var groupId = getPowerBiGroupId();
				var reports = await client.Reports.GetReportsInGroupAsync(groupId);

				return reports.Value.Where(r => !excludedReports.Contains(r.Name))
					.OrderBy(r => r.Name)
					.Select(x => new ReportModel
					{
						Id = x.Id,
						Name = x.Name,
						EmbedUrl = x.EmbedUrl
						// WebUrl = x.WebUrl,
						// DatasetId = x.DatasetId
					})
					.ToArray();
			}
		}

		public async Task<EmbedReportConfig> GetReportConfig(string reportId)
		{
			var result = new EmbedReportConfig();

			// Create a Power BI Client object. It will be used to call Power BI APIs.
			using (var client = await _powerBiClientFactory.CreatePowerBiClient())
			{
				// Get a list of reports.
				var groupId = getPowerBiGroupId();
				var reports = await client.Reports.GetReportsInGroupAsync(groupId);

				var report = string.IsNullOrEmpty(reportId)
					? reports.Value.FirstOrDefault()
					: reports.Value.FirstOrDefault(r => r.Id == reportId);

				if (report == null)
				{
					logger.Error("Group has no reports.");
					return result;
				}

				//var datasets = await client.Datasets.GetDatasetByIdInGroupAsync(groupId, report.DatasetId);
				//result.IsEffectiveIdentityRequired = datasets.IsEffectiveIdentityRequired;
				//result.IsEffectiveIdentityRolesRequired = datasets.IsEffectiveIdentityRolesRequired;

				return await generateEmbedReportConfig(client, report);
			}
		}

		public async Task<EmbedReportConfig> CreateReport(string newReportName)
		{
			var result = new EmbedReportConfig();

			// Create a Power BI Client object. It will be used to call Power BI APIs.
			using (var client = await _powerBiClientFactory.CreatePowerBiClient())
			{
				// Get a list of reports.
				var groupId = getPowerBiGroupId();
				var reports = await client.Reports.GetReportsInGroupAsync(groupId);

				var templateReport = reports.Value.SingleOrDefault(r => r.Name == TemplateReportName);
				if (templateReport == null) {
					logger.Error($"Template report \"{TemplateReportName}\" not found.");
					return result;
				}
				
				var newReport = client.Reports.CloneReportInGroup(groupId, templateReport.Id,
					new CloneReportRequest(newReportName));

				return await generateEmbedReportConfig(client, newReport);
			}
		}

		public async Task<EmbedReportConfig> CloneReport(string reportId, string newReportName)
		{
			var result = new EmbedReportConfig();

			if (string.IsNullOrEmpty(reportId))
			{
				return result;
			}

			// Create a Power BI Client object. It will be used to call Power BI APIs.
			using (var client = await _powerBiClientFactory.CreatePowerBiClient())
			{
				// Get a list of reports.
				var groupId = getPowerBiGroupId();
				var reports = await client.Reports.GetReportsInGroupAsync(groupId);

				var report = reports.Value.FirstOrDefault(r => r.Id == reportId);

				if (report == null)
				{
					logger.Error("Group has no reports.");
					return result;
				}

				var targetReportName = string.IsNullOrEmpty(newReportName)
					? report.Name + " - Copy"
					: newReportName;
				var newReport = client.Reports.CloneReportInGroup(groupId, reportId,
					new CloneReportRequest(targetReportName));

				return await generateEmbedReportConfig(client, newReport);
			}
		}

		public async Task<bool> DeleteReport(string reportId)
		{
			if (string.IsNullOrEmpty(reportId))
			{
				return false;
			}

			// Create a Power BI Client object. It will be used to call Power BI APIs.
			using (var client = await _powerBiClientFactory.CreatePowerBiClient())
			{
				try
				{
					// Get a list of reports.
					var groupId = getPowerBiGroupId();
					client.Reports.DeleteReport(groupId, reportId);
				}
				catch (Exception ex)
				{
					logger.Error($"Failed to delete report with Id={reportId}", ex);
					return false;
				}

				return true;
			}
		}

		private async Task<EmbedReportConfig> generateEmbedReportConfig(IPowerBIClient client, Report report, string userName = null,
			string roles = null)
		{
			GenerateTokenRequest generateTokenRequestParameters;
			// This is how you create embed token with effective identities
			if (!string.IsNullOrEmpty(userName))
			{
				var rls = new EffectiveIdentity(userName, new List<string> {report.DatasetId});
				if (!string.IsNullOrWhiteSpace(roles))
				{
					rls.Roles = roles.Split(',').ToList();
				}

				// Generate Embed Token with effective identities.
				// Possible values for access level: 'View', 'Edit', 'Create'
				// Refer to https://github.com/Microsoft/PowerBI-CSharp/blob/master/sdk/PowerBI.Api/Source/V2/Models/GenerateTokenRequest.cs
				generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "Edit",
					identities: new List<EffectiveIdentity> {rls});
			}
			else
			{
				// Generate Embed Token for reports without effective identities.
				generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "Edit");
			}

			var groupId = getPowerBiGroupId();
			var token = await client.Reports.GenerateTokenInGroupAsync(groupId, report.Id,
				generateTokenRequestParameters);

			if (token == null)
			{
				logger.Error("Failed to generate embed token.");
				return new EmbedReportConfig();
			}

			// The expiration got from PowerBI is earlier than now, set to Minimum Access Token Lifetime(10 minutes)
			// Refer to https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-configurable-token-lifetimes#configurable-token-lifetime-properties
			var tenMinutesLater = DateTime.Now.AddMinutes(10);
			var expiration = token.Expiration < tenMinutesLater
				? tenMinutesLater
				: token.Expiration;

			return new EmbedReportConfig
			{
				TokenType = "Embed",
				AccessToken = token.Token,
				Expiration = expiration,
				ReportId = report.Id,
				ReportName = report.Name,
				ReportUrl = report.EmbedUrl
			};
		}

		private string getPowerBiGroupId()
		{
			return _appConfig.TryGetTenantValue(TenantApplicationConfigKey.InsightsPowerBIGroupId);
		}
	}
}