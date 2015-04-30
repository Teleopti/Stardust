using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class UserConfigurable : IUserSetup, ITenantUserSetup
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

			if (changedApplicationLogonCredentials())
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

		private bool changedApplicationLogonCredentials()
		{
			return !string.IsNullOrEmpty(UserName);
		}

		public void Apply(Tenant tenant, ICurrentTenantSession tenantSession, IPerson user)
		{
			if (!WindowsAuthentication && !changedApplicationLogonCredentials()) return;

			var personInfo = tenantSession.CurrentSession().Get<PersonInfo>(user.Id.Value);
			if (personInfo == null)
			{
				personInfo = new PersonInfo(tenant, user.Id.Value);
				tenantSession.CurrentSession().Save(personInfo);
			}

			if (WindowsAuthentication)
			{
				personInfo.SetIdentity(IdentityHelper.Merge(Environment.UserDomainName, Environment.UserName));
			}
			if (changedApplicationLogonCredentials())
			{
				var wasLocked = personInfo.ApplicationLogonInfo.IsLocked;
				var lastPasswordChangeBefore = personInfo.ApplicationLogonInfo.LastPasswordChange;
				personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), UserName, Password);
				if (wasLocked)
				{
					personInfo.ApplicationLogonInfo.Lock();
				}
				personInfo.ApplicationLogonInfo.SetLastPasswordChange_OnlyUseFromTests(lastPasswordChangeBefore);
			}
		}
	}
}