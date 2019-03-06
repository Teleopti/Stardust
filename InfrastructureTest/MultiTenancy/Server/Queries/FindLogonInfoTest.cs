using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	public class FindLogonInfoTest
	{
		private Tenant tenantPresentInDatabase;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		[Test]
		public void ShouldGetLogonInfo()
		{
			var info = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			info.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make(), new OneWayEncryption());
			info.SetIdentity(RandomName.Make());
			_tenantUnitOfWorkManager.CurrentSession().Save(info);
			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(tenantPresentInDatabase);

			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentTenant);

			var result = target.GetForIds(new[] {info.Id}).Single();
			result.LogonName.Should().Be.EqualTo(info.ApplicationLogonInfo.LogonName);
			result.Identity.Should().Be.EqualTo(info.Identity);
			result.PersonId.Should().Be.EqualTo(info.Id);
		}

		[Test]
		public void ShouldNotGetPersonInfoFromWrongTenant()
		{
			var info = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			info.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make(), new OneWayEncryption());
			info.SetIdentity(RandomName.Make());
			var loggedOnTenant = new Tenant("_");
			_tenantUnitOfWorkManager.CurrentSession().Save(info);
			_tenantUnitOfWorkManager.CurrentSession().Save(loggedOnTenant);
			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(loggedOnTenant);

			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentTenant);

			target.GetForIds(new[] {info.Id})
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldSilentlyIgnoreNonExistingIds()
		{
			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(tenantPresentInDatabase);

			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentTenant);

			target.GetForIds(new[] {Guid.NewGuid(), Guid.NewGuid()})
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnNullForNonExistingColumns()
		{
			var info = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			_tenantUnitOfWorkManager.CurrentSession().Save(info);
			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(tenantPresentInDatabase);
			
			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentTenant);

			var result = target.GetForIds(new[] { info.Id }).Single();
			result.LogonName.Should().Be.Null();
			result.Identity.Should().Be.Null();
			result.PersonId.Should().Be.EqualTo(info.Id);
		}


		[Test]
		public void ShouldGetLogonInfoByName()
		{
			var info = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			info.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make(), new OneWayEncryption());
			info.SetIdentity(RandomName.Make());
			_tenantUnitOfWorkManager.CurrentSession().Save(info);
			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(tenantPresentInDatabase);

			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentTenant);

			var result = target.GetForLogonName(info.ApplicationLogonInfo.LogonName);
			result.LogonName.Should().Be.EqualTo(info.ApplicationLogonInfo.LogonName);
			result.Identity.Should().Be.EqualTo(info.Identity);
			result.PersonId.Should().Be.EqualTo(info.Id);
		}

		[Test]
		public void ShouldGetLogonInfosByIdentities()
		{
			var info = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			info.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make(), new OneWayEncryption());
			info.SetIdentity(RandomName.Make());
			_tenantUnitOfWorkManager.CurrentSession().Save(info);
			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(tenantPresentInDatabase);

			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentTenant);

			var result = target.GetForIdentities(new []{ info.Identity }).ToList();
			result.Count.Should().Be.EqualTo(1);
			result.First().LogonName.Should().Be.EqualTo(info.ApplicationLogonInfo.LogonName);
			result.First().Identity.Should().Be.EqualTo(info.Identity);
			result.First().PersonId.Should().Be.EqualTo(info.Id);
		}

		[Test]
		public void ShouldNotGetPersonInfoFromWrongTenantByLogonName()
		{
			var info = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			info.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make(), new OneWayEncryption());
			info.SetIdentity(RandomName.Make());
			var loggedOnTenant = new Tenant("_");
			_tenantUnitOfWorkManager.CurrentSession().Save(info);
			_tenantUnitOfWorkManager.CurrentSession().Save(loggedOnTenant);
			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(loggedOnTenant);

			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentTenant);

			target.GetForLogonName(info.ApplicationLogonInfo.LogonName)
				.Should().Be.Null();
		}

		[Test]
		public void ShouldSilentlyIgnoreNonExistingLogonName()
		{
			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(tenantPresentInDatabase);

			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentTenant);

			target.GetForLogonName("nonExists")
				.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullForNonExistingColumnsByLogonName()
		{
			var info = new PersonInfo(tenantPresentInDatabase, Guid.NewGuid());
			info.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make(), new OneWayEncryption());
			_tenantUnitOfWorkManager.CurrentSession().Save(info);
			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(tenantPresentInDatabase);

			var target = new FindLogonInfo(_tenantUnitOfWorkManager, currentTenant);

			var result = target.GetForLogonName(info.ApplicationLogonInfo.LogonName);
			result.LogonName.Should().Be.EqualTo(info.ApplicationLogonInfo.LogonName);
			result.Identity.Should().Be.Null();
			result.PersonId.Should().Be.EqualTo(info.Id);
		}


		[SetUp]
		public void InsertPreState()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ApplicationConnectionString());
			_tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
			
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