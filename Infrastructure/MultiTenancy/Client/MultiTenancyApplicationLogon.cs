using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class MultiTenancyApplicationLogon : IMultiTenancyApplicationLogon
	{
		private readonly IAuthenticationQuerier _authenticationQuerier;

		public MultiTenancyApplicationLogon(IAuthenticationQuerier authenticationQuerier)
		{
			_authenticationQuerier = authenticationQuerier;
		}

		public AuthenticationResult Logon(string userName, string password, string userAgent)
		{
			var result = _authenticationQuerier.TryLogon(new ApplicationLogonClientModel{UserName = userName, Password = password}, userAgent);
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
	}
}