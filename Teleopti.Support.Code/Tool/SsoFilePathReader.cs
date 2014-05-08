using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Teleopti.Support.Code.Tool
{
	public class SsoFilePathReader
	{
		public SsoFilePath Read(ModeFile mode)
		{
			var result = new SsoFilePath();
			var file = mode.FileContents();
			var authenticationBridgeConfig = file.FirstOrDefault(f => f.Contains("AuthenticationBridge"));
			if (string.IsNullOrEmpty(authenticationBridgeConfig))
			{
				return result;
			}

			var webConfigForAuthBridge = authenticationBridgeConfig.Substring(0, authenticationBridgeConfig.IndexOf(','));
			result.AuthBridgeConfig = webConfigForAuthBridge;

			var directory = Path.GetDirectoryName(webConfigForAuthBridge);
			var claimsPoliciesXml = Path.Combine(directory, "App_Data\\claimsPolicies.xml");
			result.ClaimPolicies = claimsPoliciesXml;

			var webRootConfigLine = file.FirstOrDefault(f => f.Contains("Web.Root.Web.config"));
			if (string.IsNullOrEmpty(webRootConfigLine))
			{
				return result;
			}

			var webRootConfig = webRootConfigLine.Substring(0, webRootConfigLine.IndexOf(','));
			result.WebConfig = webRootConfig;

			return result;
		}
	}

	public class SsoFilePath
	{
		public string AuthBridgeConfig { get; set; }
		public string ClaimPolicies { get; set; }
		public string WebConfig { get; set; }

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(AuthBridgeConfig) && !string.IsNullOrEmpty(ClaimPolicies) && !string.IsNullOrEmpty(WebConfig);
		}
	}
}
