using System;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.Rest;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider
{
	public class PowerBiClientFactory : IPowerBiClientFactory
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(PowerBiClientFactory));
		private readonly IConfigReader _configReader;

		const string azureAuthorityUrl = "https://login.microsoftonline.com/{0}/";
		const string resourceUrl = "https://analysis.windows.net/powerbi/api";
		const string apiUrl = "https://api.powerbi.com/";

		public PowerBiClientFactory(IConfigReader configReader)
		{
			_configReader = configReader;
		}

		public async Task<IPowerBIClient> CreatePowerBiClient()
		{
			// TODO: Should move these configurations into database (Tenant related)
			var powerBiUsername = _configReader.AppConfig("PowerBIUsername");
			var powerBiPassword = _configReader.AppConfig("PowerBIPassword");
			var clientId = _configReader.AppConfig("PowerBIClientId");
			var azureTenantId = _configReader.AppConfig("AzureTenantId");

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