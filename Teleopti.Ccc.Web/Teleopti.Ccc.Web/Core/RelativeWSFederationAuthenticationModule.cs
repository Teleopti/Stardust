using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.Web;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Core
{
	public class RelativeWSFederationAuthenticationModule : WSFederationAuthenticationModule
	{
		protected override void InitializePropertiesFromConfiguration(string serviceName)
		{
			base.InitializePropertiesFromConfiguration(serviceName);

			if (Issuer.StartsWith("http://dummy/"))
			{
				var field = typeof(WSFederationAuthenticationModule).GetField("_issuer",
					BindingFlags.NonPublic | BindingFlags.Instance);
				field.SetValue(this, Issuer.Replace("http://dummy",""));
			}
		}

		protected override string GetSessionTokenContext()
		{
			var authenticationModule = new AuthenticationModule();
			string issuer = authenticationModule.Issuer(new HttpContextWrapper(HttpContext.Current)).ToString();
			
			return ReplicaSessionTokenContextPrefix + getCustomFederationPassiveSignOutUrl(issuer, SignOutReply, SignOutQueryString);
		}

		private string getCustomFederationPassiveSignOutUrl(string issuer, string signOutReply, string signOutQueryString)
		{
			SignOutRequestMessage signOutRequestMessage = new SignOutRequestMessage(new Uri(issuer));
			if (!string.IsNullOrEmpty(signOutReply))
			{
				signOutRequestMessage.Reply = signOutReply;
			}
			if (!string.IsNullOrEmpty(signOutQueryString))
			{
				NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(signOutQueryString);
				foreach (string text in nameValueCollection.Keys)
				{
					signOutRequestMessage.Parameters.Add(text, nameValueCollection[text]);
				}
			}
			return getCustomPathAndQuery(signOutRequestMessage);
		}

		private static string getCustomPathAndQuery(WSFederationMessage request)
		{
			StringBuilder stringBuilder = new StringBuilder(128);
			string result;
			using (StringWriter stringWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
			{
				request.Write(stringWriter);
				result = stringBuilder.ToString();
			}
			return result;
		}

		public string ReplicaSessionTokenContextPrefix
		{
			get { return "(" + typeof (WSFederationAuthenticationModule).Name + ")"; }
		}
	}
}