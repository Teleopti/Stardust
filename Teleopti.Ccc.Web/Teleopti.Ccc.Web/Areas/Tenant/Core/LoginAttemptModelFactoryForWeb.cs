using System;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class LoginAttemptModelFactoryForWeb : ILoginAttemptModelFactory
	{
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IIpAddressResolver _ipAddressResolver;
		private readonly IHttpRequestUserAgent _httpRequestUserAgent;

		public const string ApplicationProvider = "Application";
		public const string WindowsProvider = "Windows";
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
				userName = winAccount.UserIdentifier;
				provider = WindowsProvider;
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




	public class LoginAttemptModelFactoryForWeb_OLD_JustKeepUntilOldStuffIsGone : ILoginAttemptModelFactory
	{
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IIpAddressResolver _ipAddressResolver;

		public const string Client = "WEB";
		public const string ApplicationProvider = "Application";
		public const string WindowsProvider = "Windows";
		public const string SuccessfulLogon = "LogonSuccess";
		public const string FailedLogon = "LogonFailed";

		public LoginAttemptModelFactoryForWeb_OLD_JustKeepUntilOldStuffIsGone(ITokenIdentityProvider tokenIdentityProvider,
																		IIpAddressResolver ipAddressResolver)
		{
			_tokenIdentityProvider = tokenIdentityProvider;
			_ipAddressResolver = ipAddressResolver;
		}

		public LoginAttemptModel Create(string userName, Guid? personId, bool wasSuccesful)
		{
			var provider = ApplicationProvider;
			if (string.IsNullOrEmpty(userName))
			{
				var winAccount = _tokenIdentityProvider.RetrieveToken();
				userName = winAccount.UserIdentifier;
				provider = WindowsProvider;
			}
			return new LoginAttemptModel
			{
				ClientIp = _ipAddressResolver.GetIpAddress(),
				Provider = provider,
				Client = Client,
				UserCredentials = userName,
				Result = wasSuccesful ? SuccessfulLogon : FailedLogon,
				PersonId = personId
			};
		}
	}
}