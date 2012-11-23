using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class UserConfigurable : IUserSetup
	{
		public string UserName { get; set; }
		public string Password { get; set; }

		public bool WindowsAuthentication { get; set; }

		public UserConfigurable()
		{
			WindowsAuthentication = false;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			if (!string.IsNullOrEmpty(UserName))
			{
				var authenticationInfo = new ApplicationAuthenticationInfo
				{
					ApplicationLogOnName = UserName,
					Password = Password
				};
				user.ApplicationAuthenticationInfo = authenticationInfo;
			}
			if (WindowsAuthentication)
			{
				var authenticationInfo = new WindowsAuthenticationInfo
					{
						WindowsLogOnName = Environment.UserName, 
						DomainName = Environment.UserDomainName
					};
				user.WindowsAuthenticationInfo = authenticationInfo;
			}
		}
	}
}