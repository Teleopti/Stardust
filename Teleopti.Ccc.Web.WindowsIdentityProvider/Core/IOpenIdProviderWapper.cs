using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
{
	public interface IOpenIdProviderWapper
	{
		IRequest GetRequest();
		OutgoingWebResponse PrepareResponse(IRequest request);
		void SendResponse(IRequest request);
	}
}