using System;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.Rest;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider
{
	public class PowerBiClientFactory : IPowerBiClientFactory
	{
		private readonly IApplicationConfigurationDbProvider _appConfig;
		private static readonly ILog logger = LogManager.GetLogger(typeof(PowerBiClientFactory));

		const string azureAuthorityUrl = "https://login.microsoftonline.com/{0}/";
		const string resourceUrl = "https://analysis.windows.net/powerbi/api";
		const string apiUrl = "https://api.powerbi.com/";

		public PowerBiClientFactory(IApplicationConfigurationDbProvider appConfig)
		{
			_appConfig = appConfig;
		}

		public async Task<IPowerBIClient> CreatePowerBiClient()
		{
			var powerBiUsername = _appConfig.TryGetTenantValue(TenantApplicationConfigKey.InsightsPowerBIUsername);
			var powerBiPassword = _appConfig.TryGetTenantValue(TenantApplicationConfigKey.InsightsPowerBIPassword);
			var clientId = _appConfig.TryGetTenantValue(TenantApplicationConfigKey.InsightsPowerBIClientId);
			var azureTenantId = _appConfig.TryGetTenantValue(TenantApplicationConfigKey.InsightsAzureTenantId);

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
	}
}