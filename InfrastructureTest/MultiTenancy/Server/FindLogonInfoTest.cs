using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class FindLogonInfoTest
	{
		private Tenant tenantPresentInDatabase;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		[Test]
		public void ShouldGetLogonInfo()
		{
			var info = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			info.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make());
			info.SetIdentity(RandomName.Make());
			_tenantUnitOfWorkManager.CurrentSession().Save(info);
			var loggedOnUser = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			var currentUser = new CurrentTenantUserFake();
			currentUser.Set(loggedOnUser);

			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentUser);

			var result = target.GetForIds(new[] {info.Id}).Single();
			result.LogonName.Should().Be.EqualTo(info.ApplicationLogonName);
			result.Identity.Should().Be.EqualTo(info.Identity);
			result.PersonId.Should().Be.EqualTo(info.Id);
		}

		[Test]
		public void ShouldNotGetPersonInfoFromWrongTenant()
		{
			var info = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			info.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make());
			info.SetIdentity(RandomName.Make());
			var loggedOnTenant = new Tenant("_");
			_tenantUnitOfWorkManager.CurrentSession().Save(info);
			_tenantUnitOfWorkManager.CurrentSession().Save(loggedOnTenant);

			var loggedOnUser = new PersonInfo(loggedOnTenant, Guid.NewGuid());
			var currentUser = new CurrentTenantUserFake();
			currentUser.Set(loggedOnUser);

			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentUser);

			target.GetForIds(new[] {info.Id})
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldSilentlyIgnoreNonExistingIds()
		{
			var loggedOnUser = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			var currentUser = new CurrentTenantUserFake();
			currentUser.Set(loggedOnUser);

			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentUser);

			target.GetForIds(new[] {Guid.NewGuid(), Guid.NewGuid()})
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnNullForNonExistingColumns()
		{
			var info = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			_tenantUnitOfWorkManager.CurrentSession().Save(info);
			var loggedOnUser = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			var currentUser = new CurrentTenantUserFake();
			currentUser.Set(loggedOnUser);
			
			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentUser);

			var result = target.GetForIds(new[] { info.Id }).Single();
			result.LogonName.Should().Be.Null();
			result.Identity.Should().Be.Null();
			result.PersonId.Should().Be.EqualTo(info.Id);
		}

		[SetUp]
		public void InsertPreState()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConnectionStringHelper.ConnectionStringUsedInTests);
			
			tenantPresentInDatabase = new Tenant(RandomName.Make());
			_tenantUnitOfWorkManager.CurrentSession().Save(tenantPresentInDatabase);
		}

		[TearDown]
		public void RollbackTransaction()
		{
			_tenantUnitOfWorkManager.Dispose();
		}
	}
}