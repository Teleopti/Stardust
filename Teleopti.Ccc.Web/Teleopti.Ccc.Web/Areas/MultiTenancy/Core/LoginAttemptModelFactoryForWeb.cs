using System;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class LoginAttemptModelFactoryForWeb : ILoginAttemptModelFactory
	{
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IIpAddressResolver _ipAddressResolver;
		private readonly IHttpRequestUserAgent _httpRequestUserAgent;

		public const string ApplicationProvider = "Application";
		public const string IdentityProvider = "Identity";
		public const string SuccessfulLogon = "LogonSuccess";
		public const string FailedLogon = "LogonFailed";

		public LoginAttemptModelFactoryForWeb(ITokenIdentityProvider tokenIdentityProvider,
																		IIpAddressResolver ipAddressResolver,
																		IHttpRequestUserAgent httpRequestUserAgent)
		{
			_tokenIdentityProvider = tokenIdentityProvider;
			_ipAddressResolver = ipAddressResolver;
			_httpRequestUserAgent = httpRequestUserAgent;
		}

		public LoginAttemptModel Create(string userName, Guid? personId, bool wasSuccesful)
		{
			var provider = ApplicationProvider;
			if (string.IsNullOrEmpty(userName))
			{
				var winAccount = _tokenIdentityProvider.RetrieveToken();
				userName = winAccount==null ? "N/A" : winAccount.UserIdentifier;
				provider = IdentityProvider;
			}
			return new LoginAttemptModel
			{
				ClientIp = _ipAddressResolver.GetIpAddress(),
				Provider = provider,
				Client = _httpRequestUserAgent.Fetch(),
				UserCredentials = userName,
				Result = wasSuccesful ? SuccessfulLogon : FailedLogon,
				PersonId = personId
			};
		}
	}
}