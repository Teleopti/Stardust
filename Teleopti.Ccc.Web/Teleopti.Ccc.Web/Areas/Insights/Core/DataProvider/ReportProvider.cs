﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Web.Areas.Insights.Models;

namespace Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider
{
	public class ReportProvider : IReportProvider
	{
		private const string templateReportName = "__WFM_Insights_Report_Template__";
		private const string usageReportName = "Report Usage Metrics Report";
		private readonly IConfigReader _configReader;
		private readonly IPowerBiClientFactory _powerBiClientFactory;
		private static readonly ILog logger = LogManager.GetLogger(typeof(ReportProvider));

		public ReportProvider(IConfigReader configReader,
			IPowerBiClientFactory powerBiClientFactory)
		{
			_configReader = configReader;
			_powerBiClientFactory = powerBiClientFactory;
		}

		public async Task<ReportModel[]> GetReports()
		{
			var excludedReports = new[] {usageReportName, templateReportName};
			using (var client = await _powerBiClientFactory.CreatePowerBiClient())
			{
				var groupId = _configReader.AppConfig("PowerBIGroupId");
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
				var groupId = _configReader.AppConfig("PowerBIGroupId");
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
				var groupId = _configReader.AppConfig("PowerBIGroupId");
				var reports = await client.Reports.GetReportsInGroupAsync(groupId);

				var templateReport = reports.Value.SingleOrDefault(r => r.Name == templateReportName);
				if (templateReport == null) {
					logger.Error($"Template report \"{templateReportName}\" not found.");
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
				var groupId = _configReader.AppConfig("PowerBIGroupId");
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
					var groupId = _configReader.AppConfig("PowerBIGroupId");
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

			var groupId = _configReader.AppConfig("PowerBIGroupId");
			var token = await client.Reports.GenerateTokenInGroupAsync(groupId, report.Id,
				generateTokenRequestParameters);

			if (token == null)
			{
				logger.Error("Failed to generate embed token.");
				return new EmbedReportConfig();
			}

			var result = new EmbedReportConfig
			{
				TokenType = "Embed",
				AccessToken = token.Token,
				// The expiration got from PowerBI is earlier than now, set to Minimum Access Token Lifetime(10 minutes)
				// Refer to https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-configurable-token-lifetimes#configurable-token-lifetime-properties
				Expiration = token.Expiration < DateTime.Now
					? DateTime.Now.AddMinutes(10)
					: token.Expiration,
				ReportId = report.Id,
				ReportName = report.Name,
				ReportUrl = report.EmbedUrl
			};

			return result;
		}
	}
}