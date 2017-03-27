using System.Linq;
using Microsoft.Web.Administration;

namespace Teleopti.Support.Tool.Tool
{
	public class FrameAncestorsUpdator : IFrameAncestorsUpdator
	{
		public void Update(string url)
		{
			using (var serverManager = new ServerManager())
			{
				foreach (var site in serverManager.Sites)
				{
					foreach (var application in site.Applications)
					{
						if (application.Path.EndsWith("/Web") || 
							application.Path.EndsWith("/AuthenticationBridge") ||
							application.Path.EndsWith("/WindowsIdentityProvider"))
						{
							updateFrameAncestors(application.GetWebConfiguration(), url);
						}
					}
				}
				serverManager.CommitChanges();
			}
		}

		private void updateFrameAncestors(Configuration configuration, string url)
		{
			var httpProtocolSection = configuration.GetSection("system.webServer/httpProtocol");
			var customHeadersCollection = httpProtocolSection.GetCollection("customHeaders");
			var value = $"script-src \'self\' \'unsafe-inline\' \'unsafe-eval\'; frame-ancestors \'self\' {url};";
			createOrUpdate(customHeadersCollection, "Content-Security-Policy", value);
		}

		private void createOrUpdate(ConfigurationElementCollection customHeadersCollection, string name, string value)
		{
			var contentSecurityPolicy = customHeadersCollection.SingleOrDefault(x => (string)x["name"] == name);
			if (contentSecurityPolicy == null)
			{
				contentSecurityPolicy = customHeadersCollection.CreateElement("add");
				contentSecurityPolicy["name"] = name;
				customHeadersCollection.Add(contentSecurityPolicy);
			}
			contentSecurityPolicy["value"] = value;
		}
	}
}
