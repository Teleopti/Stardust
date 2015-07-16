using Teleopti.Ccc.Domain.MultipleConfig;

namespace Teleopti.Ccc.Web.Core
{
	public class IdentityProviderProvider : IIdentityProviderProvider
	{
		private readonly IConfigReader _configReader;
	    private string _overrideProvider;

	    public IdentityProviderProvider(IConfigReader configReader)
		{
			_configReader = configReader;
		}

		public string DefaultProvider()
		{
            return "urn:" + (_overrideProvider ?? _configReader.AppSettings["DefaultIdentityProvider"]);
		}

	    internal void SetDefaultProvider(string provider)
	    {
	        _overrideProvider = provider;
	    }
	}
}