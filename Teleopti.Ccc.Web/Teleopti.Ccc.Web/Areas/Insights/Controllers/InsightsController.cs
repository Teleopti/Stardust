using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using Teleopti.Ccc.Web.Areas.Insights.Models;

namespace Teleopti.Ccc.Web.Areas.Insights.Controllers
{
	public class InsightsController : ApiController
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(InsightsController));

		private static readonly string powerBiUsername = ConfigurationManager.AppSettings["PowerBIUsername"];
		private static readonly string powerBiPassword = ConfigurationManager.AppSettings["PowerBIPassword"];
		private static readonly string clientId = ConfigurationManager.AppSettings["PowerBIClientId"];
		private static readonly string groupId = ConfigurationManager.AppSettings["PowerBIGroupId"];
		private static readonly string azureTenantId = ConfigurationManager.AppSettings["AzureTenantId"];

		const string azureAuthorityUrl = "https://login.microsoftonline.com/{0}/";
		const string resourceUrl = "https://analysis.windows.net/powerbi/api";
		const string apiUrl = "https://api.powerbi.com/";

		[HttpGet, Route("api/Insights/ReportConfig")]
		public virtual async Task<EmbedReportConfig> GetReportConfig(string reportId)
		{
			var result = new EmbedReportConfig();

			// Create a Power BI Client object. It will be used to call Power BI APIs.
			using (var client = await createBiClient())
			{
				// Get a list of reports.
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

		[HttpGet, Route("api/Insights/Reports")]
		public virtual async Task<ReportModel[]> GetReports()
		{
			using (var client = await createBiClient())
			{
				var reports = await client.Reports.GetReportsInGroupAsync(groupId);

				return reports.Value.Select(x => new ReportModel
				{
					Id = x.Id,
					Name = x.Name,
					EmbedUrl = x.EmbedUrl
					// WebUrl = x.WebUrl,
					// DatasetId = x.DatasetId
				}).ToArray();
			}
		}

		private async Task<IPowerBIClient> createBiClient()
		{
			// Create a user password credentials.
			var credential = new UserPasswordCredential(powerBiUsername, powerBiPassword);

			// Authenticate using created credentials
			var authorityUrl = string.Format(azureAuthorityUrl, azureTenantId);
			var authenticationContext = new AuthenticationContext(authorityUrl);
			var authenticationResult = await authenticationContext.AcquireTokenAsync(resourceUrl, clientId, credential);

			if (authenticationResult == null)
			{
				logger.Error("Authentication Failed.");
				return null;
			}

			var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");

			// Create a Power BI Client object. It will be used to call Power BI APIs.
			var client = new PowerBIClient(new Uri(apiUrl), tokenCredentials);
			return client;
		}

		private async Task<EmbedToken> generateAccessToken(IPowerBIClient client, Report report, string userName = null, string roles = null)
		{
			GenerateTokenRequest generateTokenRequestParameters;
			// This is how you create embed token with effective identities
			if (!string.IsNullOrEmpty(userName))
			{
				var rls = new EffectiveIdentity(userName, new List<string> { report.DatasetId });
				if (!string.IsNullOrWhiteSpace(roles))
				{
					rls.Roles = roles.Split(',').ToList();
				}

				// Generate Embed Token with effective identities.
				// Possible values for access level: 'View', 'Edit', 'Create'
				// Refer to https://github.com/Microsoft/PowerBI-CSharp/blob/master/sdk/PowerBI.Api/Source/V2/Models/GenerateTokenRequest.cs
				generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "Edit", identities: new List<EffectiveIdentity> { rls });
			}
			else
			{
				// Generate Embed Token for reports without effective identities.
				generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "Edit");
			}

			var token = await client.Reports.GenerateTokenInGroupAsync(groupId, report.Id, generateTokenRequestParameters);

			if (token == null)
			{
				logger.Error("Failed to generate embed token.");
			}

			return token;
		}
	}
}
