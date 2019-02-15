using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;
using AuthBridge.Configuration;
using AuthBridge.Utilities;
using Microsoft.Practices.Unity;

namespace Teleopti.Ccc.Web.AuthenticationBridge
{
	public class SystemStatus : ViewPage
	{
		private static readonly HttpClient client = new HttpClient();
		private readonly StringBuilder sbTriedVisitByIdentityUrls = new StringBuilder();
		
		protected void Page_Load(object sender, EventArgs e)
		{
			var urls = providerUrls();
			urls = urls.Append(() => ("Teleopti web site",
				tryVisitUrl(ServiceLocator.Container.Value.Resolve<IConfigurationRepository>()
					.RetrieveDefaultScope(Request.UrlConsideringLoadBalancerHeaders()).Url.ToString())));

			var results = urls.AsParallel().Select(u => u()).ToDictionary(k => k.Item1, v => v.Item2);
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
			Response.StatusCode =  allOk ? 200 : 500;
		}

		private IEnumerable<Func<ValueTuple<string, bool>>> providerUrls()
		{
			var uriLoadBalancer = Request.UrlConsideringLoadBalancerHeaders();
			var configurationRepository = ServiceLocator.Container.Value.Resolve<IConfigurationRepository>();
			var issuers = configurationRepository.RetrieveIssuers(uriLoadBalancer).Where(provider => provider.DisplayName.ToLower().Contains("teleopti"));

			string baseUrl = uriLoadBalancer.Scheme + "://" + uriLoadBalancer.Authority +
							 Request.ApplicationPath.TrimEnd('/') + "/";
			return issuers.Select(provider =>
			{
				return new Func<(string, bool)>(() =>
				{
					var authenticateUrl = baseUrl + "authenticate?whr=" + provider.Identifier;

					sbTriedVisitByIdentityUrls.Append($"{authenticateUrl};");

					return (provider.DisplayName, tryVisitUrl(authenticateUrl));
				});
			});
		}
		
		private static bool tryVisitUrl(string url)
		{
			try
			{
				return client.GetAsync(url).Result.IsSuccessStatusCode;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}