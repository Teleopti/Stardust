using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public class OpenIdProviderWapper : IOpenIdProviderWapper
	{
		public OpenIdProvider OpenIdProvider { get; set; }

		public IRequest GetRequest()
		{
			return OpenIdProvider.GetRequest();
		}

		public OutgoingWebResponse PrepareResponse(IRequest request)
		{
			return OpenIdProvider.PrepareResponse(request);
		}

		public void SendResponse(IRequest request)
		{
			OpenIdProvider.SendResponse(request);
		}
	}
}