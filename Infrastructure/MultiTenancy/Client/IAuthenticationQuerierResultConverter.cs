using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IAuthenticationQuerierResultConverter
	{
		AuthenticationQuerierResult Convert(AuthenticationInternalQuerierResult tenantServerResult);
	}
}