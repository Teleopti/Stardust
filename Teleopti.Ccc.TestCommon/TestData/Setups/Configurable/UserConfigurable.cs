using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class UserConfigurable : IUserSetup
	{
		public string Name { get; set; }

		public DateTime? TerminalDate { get; set; }

		public string UserName { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }

		public bool WindowsAuthentication { get; set; }

		public UserConfigurable()
		{
			WindowsAuthentication = false;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{

			if (!string.IsNullOrEmpty(Name))
			{
				if (Name.Contains(" "))
				{
					var splitted = Name.Split(' ');
					user.Name = new Name(splitted[0], splitted[1]);
				}
				else
					user.Name = new Name("", Name);
			}

			if (TerminalDate.HasValue)
				user.TerminatePerson(new DateOnly(TerminalDate.Value), new PersonAccountUpdaterDummy());

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
				var authenticationInfo = new AuthenticationInfo
				{
					Identity = IdentityHelper.Merge(Environment.UserDomainName, Environment.UserName)
				};
				user.AuthenticationInfo = authenticationInfo;
			}

			if (!string.IsNullOrEmpty(Role))
			{
				var roleRepository = new ApplicationRoleRepository(uow);
				var role = roleRepository.LoadAll().Single(b => b.Name ==  Role);
				user.PermissionInformation.AddApplicationRole(role);
			}
		}
	}
}