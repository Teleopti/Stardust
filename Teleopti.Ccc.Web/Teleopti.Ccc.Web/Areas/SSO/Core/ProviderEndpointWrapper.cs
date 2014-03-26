using DotNetOpenAuth.OpenId.Provider;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public class ProviderEndpointWrapper : IProviderEndpointWrapper
	{
		public IHostProcessedRequest PendingRequest {
			get { return ProviderEndpoint.PendingRequest; }
			set { ProviderEndpoint.PendingRequest = value; }
		}

		public IAuthenticationRequest PendingAuthenticationRequest {get
		{
			return ProviderEndpoint.PendingAuthenticationRequest;
		}}

		public IAnonymousRequest PendingAnonymousRequest
		{
			get
			{
				return ProviderEndpoint.PendingAnonymousRequest;
			}
		}
	}
}