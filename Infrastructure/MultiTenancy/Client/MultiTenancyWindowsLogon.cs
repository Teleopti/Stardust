using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class MultiTenancyWindowsLogon : IMultiTenancyWindowsLogon
	{
		private readonly IAuthenticationQuerier _authenticationQuerier;
		private readonly IWindowsUserProvider _windowsUserProvider;

		public MultiTenancyWindowsLogon(IAuthenticationQuerier authenticationQuerier, IWindowsUserProvider windowsUserProvider) 
		{
			_authenticationQuerier = authenticationQuerier;
			_windowsUserProvider = windowsUserProvider;
		}

		public AuthenticationResult Logon(string userAgent)
		{
			var identity = _windowsUserProvider.Identity();
			var result = _authenticationQuerier.TryLogon(new IdentityLogonClientModel{Identity = identity}, userAgent);
			if (!result.Success)
				return new AuthenticationResult
				{
					Successful = false,
					HasMessage = true,
					Message = result.FailReason
				};

			return new AuthenticationResult
			{
				Person = result.Person,
				Successful = true,
				DataSource = result.DataSource
			};
		}

		public bool CheckWindowsIsPossible()
		{
			return _authenticationQuerier.TryLogon(new IdentityLogonClientModel{Identity = _windowsUserProvider.Identity()}, "").Success;
		}
	}
}