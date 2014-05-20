using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
{
	public class OpenIdProviderWrapper : IOpenIdProviderWrapper
	{
		private readonly OpenIdProvider _openIdProvider;

		public OpenIdProviderWrapper(OpenIdProvider openIdProvider)
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

		public IDirectWebRequestHandler WebRequestHandler()
		{
			return _openIdProvider.Channel.WebRequestHandler;
		}
	}
}