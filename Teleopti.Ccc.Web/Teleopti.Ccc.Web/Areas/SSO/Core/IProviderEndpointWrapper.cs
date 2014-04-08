using DotNetOpenAuth.OpenId.Provider;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public interface IProviderEndpointWrapper
	{
		IHostProcessedRequest PendingRequest { get; set; }
		IAuthenticationRequest PendingAuthenticationRequest { get; }
		IAnonymousRequest PendingAnonymousRequest { get; }
	}
}