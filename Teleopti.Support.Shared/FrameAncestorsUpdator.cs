using System.Linq;
using Microsoft.Web.Administration;

namespace Teleopti.Support.Shared
{
	public class FrameAncestorsUpdator : IFrameAncestorsUpdator
	{
		public void Update(string url)
		{
			using (var serverManager = new ServerManager())
			{
				var webConfiguration = serverManager.GetWebConfiguration("Default Web Site/TeleoptiWFM/Web");
				var authenticationBridgeConfiguration = serverManager.GetWebConfiguration("Default Web Site/TeleoptiWFM/AuthenticationBridge");
				var windowsIdentityProviderConfiguration = serverManager.GetWebConfiguration("Default Web Site/TeleoptiWFM/WindowsIdentityProvider");

				updateFrameAncestors(webConfiguration, url);
				updateFrameAncestors(authenticationBridgeConfiguration, url);
				updateFrameAncestors(windowsIdentityProviderConfiguration, url);

				serverManager.CommitChanges();
			}
		}

		private void updateFrameAncestors(Configuration configuration, string url)
		{
			var httpProtocolSection = configuration.GetSection("system.webServer/httpProtocol");
			var customHeadersCollection = httpProtocolSection.GetCollection("customHeaders");
			var contentSecurityPolicy = customHeadersCollection.Single(x => (string)x["name"] == @"Content-Security-Policy");
			contentSecurityPolicy["value"] = "script-src 'self' 'unsafe-inline' 'unsafe-eval'; frame-ancestors 'self' " + url + ";";
		}
	}
}
