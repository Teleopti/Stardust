using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Web.Core
{
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