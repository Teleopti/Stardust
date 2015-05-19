using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
	public class SdkCurrentTenantCredentials : ICurrentTenantCredentials
	{
		public TenantCredentials TenantCredentials
		{
			get
			{
				var personContainer = OperationContext.Current.ServiceSecurityContext.AuthorizationPolicies.OfType<TeleoptiPrincipalAuthorizationPolicy>()
					.Single()
					.PersonContainer;
				return new TenantCredentials(personContainer.Person.Id.Value, personContainer.TenantPassword);
			}
		}
	}
}