using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AuthBridge.Configuration;
using AuthBridge.Model;

namespace Teleopti.Ccc.Web.AuthenticationBridge
{
	public class ProviderTest : ViewPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var results = provider();
			
			var isScopeAccessible = tryVisitUrl(DefaultConfigurationRepository.Instance.RetrieveDefaultScope().Uri);
			results.Add("Teleopti web site", isScopeAccessible);
			
			foreach (var providerResult in results)
			{
				var statusText = providerResult.Value ? "OK" : "FAILED";
				var textColor =  providerResult.Value ? "green" : "red";
				Response.Write($"<div style='color:{textColor}'>{providerResult.Key} - {statusText}</div>");
			}
			
			var allOk = results.All(pair => pair.Value != false);
			var systemTextColor =  allOk ? "green" : "red";
			var systemText = allOk ? "System is up!" : "Some parts of the system is not working as expected.";
			Response.Write($"<br><div style='color:{systemTextColor}'>{systemText}</div>");
			
			
			Response.StatusCode =  allOk ? 200: 500;
		}

		private static Dictionary<string, bool> provider()
		{
			var configurationRepository = DefaultConfigurationRepository.Instance;
			var issuers = configurationRepository.RetrieveIssuers();

			var result = tryVisitUrls(issuers);
			return result;
		}

		private static Dictionary<string, bool> tryVisitUrls(IEnumerable<ClaimProvider> providers)
		{
			var result = new Dictionary<string, bool>();
			foreach (var provider in providers)
				result.Add(provider.DisplayName, tryVisitUrl(provider.Url.AbsoluteUri));

			return result;
		}

		private static bool tryVisitUrl(string url)
		{
			try
			{
				var web = WebRequest.Create(url);
				web.GetResponse();
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}