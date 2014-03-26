using DotNetOpenAuth.OpenId.Provider;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
{
	public interface IProviderEndpointWrapper
	{
		IHostProcessedRequest PendingRequest { get; set; }
	}
}