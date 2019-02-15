using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonUserConfigurable : IUserSetup, ITenantUserSetup
	{
		public string Name { get; set; }

		public DateTime? TerminalDate { get; set; }

		public string UserName { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }

		public bool WindowsAuthentication { get; set; }

		public bool Delete { get; set; }

		public PersonUserConfigurable()
		{
			WindowsAuthentication = false;
			Delete = false;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			if (Delete)
			{
				var repository = new PersonRepository(new ThisUnitOfWork(uow));
				repository.Remove(user);
				return;
			}

			PersonConfigurable.SetName(user, Name);

			if (TerminalDate.HasValue)
				user.TerminatePerson(new DateOnly(TerminalDate.Value), new PersonAccountUpdaterDummy());

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

		public void Apply(ICurrentTenantSession tenantSession, IPerson user, ILogonName logonName)
		{
			if (!WindowsAuthentication && !changedApplicationLogonCredentials()) return;

			var personInfo = tenantSession.CurrentSession().Get<PersonInfo>(user.Id.Value);

			if (personInfo == null)
			{
				personInfo = new PersonInfo(defaultTenant(tenantSession), user.Id.Value);
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
				personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), UserName, Password, new OneWayEncryption());
				if (wasLocked)
				{
					personInfo.ApplicationLogonInfo.Lock();
				}
				personInfo.ApplicationLogonInfo.SetLastPasswordChange_OnlyUseFromTests(lastPasswordChangeBefore);
				logonName.Set(UserName);
			}
		}

		private static Tenant defaultTenant(ICurrentTenantSession tenantSession)
		{
			return new LoadAllTenants(tenantSession).Tenants().Single(t => t.Name.Equals(DataSourceHelper.TenantName));
		}
	}
}