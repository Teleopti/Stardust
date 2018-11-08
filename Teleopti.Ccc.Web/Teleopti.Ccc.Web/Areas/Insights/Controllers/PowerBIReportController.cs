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
		private static readonly string authorityUrl = ConfigurationManager.AppSettings["PowerBIAuthorityUrl"];
		private static readonly string resourceUrl = ConfigurationManager.AppSettings["PowerBIResourceUrl"];
		private static readonly string clientId = ConfigurationManager.AppSettings["PowerBIClientId"];
		private static readonly string apiUrl = ConfigurationManager.AppSettings["PowerBIApiUrl"];
		private static readonly string groupId = ConfigurationManager.AppSettings["PowerBIGroupId"];

		[HttpGet, Route("api/Insights/ReportConfig")]
		public virtual async Task<EmbedReportConfig> GetReportConfig()
		{
			var reportId = "";
			var userName = "";
			var roles = "";

			var result = new EmbedReportConfig();

			// Create a user password credentials.
			var credential = new UserPasswordCredential(powerBiUsername, powerBiPassword);

			// Authenticate using created credentials
			var authenticationContext = new AuthenticationContext(authorityUrl);
			var authenticationResult = await authenticationContext.AcquireTokenAsync(resourceUrl, clientId, credential);

			if (authenticationResult == null)
			{
				logger.Error("Authentication Failed.");
				return result;
			}

			var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");

			// Create a Power BI Client object. It will be used to call Power BI APIs.
			using (var client = new PowerBIClient(new Uri(apiUrl), tokenCredentials))
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
				GenerateTokenRequest generateTokenRequestParameters;
				// This is how you create embed token with effective identities
				if (!string.IsNullOrEmpty(userName))
				{
					var rls = new EffectiveIdentity(userName, new List<string> { report.DatasetId });
					if (!string.IsNullOrWhiteSpace(roles))
					{
						var rolesList = new List<string>();
						rolesList.AddRange(roles.Split(','));
						rls.Roles = rolesList;
					}
					// Generate Embed Token with effective identities.
					generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view", identities: new List<EffectiveIdentity> { rls });
				}
				else
				{
					// Generate Embed Token for reports without effective identities.
					generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
				}

				var tokenResponse = await client.Reports.GenerateTokenInGroupAsync(groupId, report.Id, generateTokenRequestParameters);

				if (tokenResponse == null)
				{
					logger.Error("Failed to generate embed token.");
					return result;
				}

				// Generate Embed Configuration.
				result.TokenType = "Embed";
				result.AccessToken = tokenResponse.Token;
				result.ReportUrl = report.EmbedUrl;
				result.ReportId = report.Id;

				return result;
			}
		}
	}
}
