﻿using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
{
	public class OpenIdProviderWapper : IOpenIdProviderWapper
	{
		private readonly OpenIdProvider _openIdProvider;

		public OpenIdProviderWapper(OpenIdProvider openIdProvider)
		{
			_openIdProvider = openIdProvider;
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
	}
}