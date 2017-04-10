using System.Linq;
using System.ServiceModel;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
	public class CurrentPersonContainer : ICurrentPersonContainer
	{
		public PersonContainer Current()
		{
			return OperationContext.Current.ServiceSecurityContext.AuthorizationPolicies.OfType<TeleoptiPrincipalAuthorizationPolicy>()
				.Single()
				.PersonContainer;
		}
	}
}