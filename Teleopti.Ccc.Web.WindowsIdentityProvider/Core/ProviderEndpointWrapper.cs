using DotNetOpenAuth.OpenId.Provider;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
{
	public class ProviderEndpointWrapper : IProviderEndpointWrapper
	{
		public IHostProcessedRequest PendingRequest {
			get { return ProviderEndpoint.PendingRequest; }
			set { ProviderEndpoint.PendingRequest = value; }
		}
	}
}