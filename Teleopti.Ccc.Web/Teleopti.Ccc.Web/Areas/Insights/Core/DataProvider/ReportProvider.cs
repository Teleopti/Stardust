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
			using (var client = await _powerBiClientFactory.CreatePowerBiClient())
			{
				var groupId = _configReader.AppConfig("PowerBIGroupId");
				var reports = await client.Reports.GetReportsInGroupAsync(groupId);

				return reports.Value.OrderBy(r => r.Name)
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

				var token = await generateAccessToken(client, report);

				// Generate Embed Configuration.
				result.TokenType = "Embed";
				result.AccessToken = token.Token;
				result.ReportUrl = report.EmbedUrl;
				result.ReportId = report.Id;

				return result;
			}
		}

		public async Task<EmbedReportConfig> CloneReport(string reportId)
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

				var newReport = client.Reports.CloneReportInGroup(groupId, reportId,
					new CloneReportRequest(report.Name + " - Copy"));

				var accessToken = await generateAccessToken(client, newReport);

				result.TokenType = "Embed";
				result.AccessToken = accessToken.Token;
				result.ReportUrl = newReport.EmbedUrl;
				result.ReportId = newReport.Id;

				return result;
			}
		}

		private async Task<EmbedToken> generateAccessToken(IPowerBIClient client, Report report, string userName = null,
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
			}

			return token;
		}
	}
}