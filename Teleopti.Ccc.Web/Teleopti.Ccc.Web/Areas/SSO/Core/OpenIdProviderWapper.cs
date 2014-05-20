using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public class OpenIdProviderWapper : IOpenIdProviderWapper
	{
		private readonly OpenIdProvider _openIdProvider;

		public OpenIdProviderWapper(OpenIdProvider openIdProvider)
		{
			_openIdProvider = openIdProvider;
			_openIdProvider.ErrorReporting = new CustomErrorReporting();
		}

		public IRequest GetRequest()
		{
			return _openIdProvider.GetRequest();
		}

		public OutgoingWebResponse PrepareResponse(IRequest request)
		{
			return _openIdProvider.PrepareResponse(request);
		}

		public void SendResponse(IRequest request)
		{
			_openIdProvider.SendResponse(request);
		}

		public Channel Channel()
		{
			return _openIdProvider.Channel;
		}
	}
}