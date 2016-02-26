using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public IPerson Person { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var repository = new PersonRepository(currentUnitOfWork);
			Person = PersonFactory.CreatePerson();
			Person.Name = ParseName(Name);
			repository.Add(Person);
		}

		public static Name ParseName(string name)
		{
			if (string.IsNullOrEmpty(name))
				return new Name();
			if (!name.Contains(" "))
				return new Name("", name);
			var splitted = name.Split(' ');
			return new Name(splitted[0], splitted[1]);
		}
	}

	public class PersonUserConfigurable : IUserSetup, ITenantUserSetup
	{
		public const string DefaultTenantName = "TestData";

		public string Name { get; set; }

		public DateTime? TerminalDate { get; set; }

		public string UserName { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }

		public bool WindowsAuthentication { get; set; }

		public string Tenant { get; set; }

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

			user.Name = PersonConfigurable.ParseName(Name);

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
			if (!WindowsAuthentication && !changedApplicationLogonCredentials() && Tenant==null) return;

			var personInfo = tenantSession.CurrentSession().Get<PersonInfo>(user.Id.Value);

			if (personInfo == null)
			{
				personInfo = new PersonInfo(defaultTenant(tenantSession), user.Id.Value);
				tenantSession.CurrentSession().Save(personInfo);
			}

			if (Tenant != null)
			{
				personInfo.ChangeTenant_OnlyUseInTest(new FindTenantByName(tenantSession).Find(Tenant));
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
				logonName.Set(UserName);
			}
		}

		private static Tenant defaultTenant(ICurrentTenantSession tenantSession)
		{
			return new LoadAllTenants(tenantSession).Tenants().Single(t => t.Name.Equals(DefaultTenantName));
		}
	}
}