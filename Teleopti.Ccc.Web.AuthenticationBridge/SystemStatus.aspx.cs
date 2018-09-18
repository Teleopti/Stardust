using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using AuthBridge.Configuration;
using AuthBridge.Model;
using AuthBridge.Utilities;
using Microsoft.Practices.Unity;

namespace Teleopti.Ccc.Web.AuthenticationBridge
{
	public class SystemStatus : ViewPage
	{
		private readonly StringBuilder sbTriedVisitByIdentityUrls = new StringBuilder();
		protected void Page_Load(object sender, EventArgs e)
		{
			var results = visitProviderUrls();
			
			var isScopeAccessible = tryVisitUrl(ServiceLocator.Container.Value.Resolve<IConfigurationRepository>().RetrieveDefaultScope(Request.UrlConsideringLoadBalancerHeaders()).Url.ToString());
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

			Response.Write($"<input type=\"hidden\" value=\"{sbTriedVisitByIdentityUrls}\">");
			Response.StatusCode =  allOk ? 200: 202;
		}

		private Dictionary<string, bool> visitProviderUrls()
		{
			var configurationRepository = ServiceLocator.Container.Value.Resolve<IConfigurationRepository>();
			var issuers = configurationRepository.RetrieveIssuers(Request.UrlConsideringLoadBalancerHeaders()).Where(provider => provider.DisplayName.ToLower().Contains("teleopti"));
			var result = tryVisitUrlsByIdentity(issuers);
			return result;
		}

		private Dictionary<string, bool> tryVisitUrlsByIdentity(IEnumerable<ClaimProvider> providers)
		{
			var result = new Dictionary<string, bool>();
			foreach (var provider in providers)
			{
				var uriLoadBalancer = Request.UrlConsideringLoadBalancerHeaders();

				var baseUrl = uriLoadBalancer.Scheme + "://" + uriLoadBalancer.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
				var authenticateUrl = baseUrl + "authenticate?whr=" + provider.Identifier;

				sbTriedVisitByIdentityUrls.Append($"{authenticateUrl};");

				result.Add(provider.DisplayName, tryVisitUrl(authenticateUrl));
			}
			return result;
		}

		private static bool tryVisitUrl(string url)
		{
			
			try
			{
				var httpWebRequest = WebRequest.CreateHttp(url);
				var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				return httpWebResponse.StatusCode == HttpStatusCode.OK;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}