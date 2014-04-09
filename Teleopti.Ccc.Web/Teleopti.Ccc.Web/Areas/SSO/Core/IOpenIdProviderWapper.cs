using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public interface IOpenIdProviderWapper
	{
		IRequest GetRequest();
		OutgoingWebResponse PrepareResponse(IRequest request);
		void SendResponse(IRequest request);
		Channel Channel();
	}
}