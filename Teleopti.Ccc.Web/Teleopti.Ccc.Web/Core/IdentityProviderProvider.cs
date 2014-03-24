using System.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Web.Core
{
	public class IdentityProviderProvider : IIdentityProviderProvider
	{
		private readonly IConfigurationWrapper _configurationWrapper;

		public IdentityProviderProvider(IConfigurationWrapper configurationWrapper)
		{
			_configurationWrapper = configurationWrapper;
		}

		public string DefaultProvider()
		{
			return _configurationWrapper.AppSettings["IdentityProviders"].Split('|').FirstOrDefault();
		}
	}
}