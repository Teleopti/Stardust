using System;
using System.Reflection;
using Microsoft.IdentityModel.Configuration;
using Microsoft.IdentityModel.Web;
using Teleopti.Ccc.Infrastructure.Foundation;

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
	}

	public class IdentityProviderProvider : IIdentityProviderProvider
	{
		private readonly IConfigurationWrapper _configurationWrapper;
	    private string _overrideProvider;

	    public IdentityProviderProvider(IConfigurationWrapper configurationWrapper)
		{
			_configurationWrapper = configurationWrapper;
		}

		public string DefaultProvider()
		{
            return "urn:" + (_overrideProvider ?? _configurationWrapper.AppSettings["DefaultIdentityProvider"]);
		}

	    internal void SetDefaultProvider(string provider)
	    {
	        _overrideProvider = provider;
	    }
	}
}