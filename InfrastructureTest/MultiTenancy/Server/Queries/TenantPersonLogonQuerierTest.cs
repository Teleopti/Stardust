using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	public class TenantPersonLogonQuerierTest
	{
		private Tenant tenantPresentInDatabase;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		private readonly string _tenantName = RandomName.Make();

		[SetUp]
		public void InsertPreState()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			_tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();

			tenantPresentInDatabase = new Tenant(_tenantName);
			_tenantUnitOfWorkManager.CurrentSession().Save(tenantPresentInDatabase);
		}

		[TearDown]
		public void RollbackTransaction()
		{
			_tenantUnitOfWorkManager.Dispose();
		}

		[Test]
		public void ShouldGetPersonInfoModel()
		{
			var infoModel = new PersonInfoModel()
			{
				ApplicationLogonName = "Agent1",
				Identity = RandomName.Make(),
				Password = RandomName.Make(),
				PersonId = Guid.NewGuid()
			};
			var info = new PersonInfo(tenantPresentInDatabase, infoModel.PersonId);
			info.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), infoModel.ApplicationLogonName, infoModel.Password, new OneWayEncryption());
			info.SetIdentity(infoModel.Identity);
			_tenantUnitOfWorkManager.CurrentSession().Save(info);
			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(tenantPresentInDatabase);

			var target = new TenantPersonLogonQuerier(new FakeCurrentDatasource(_tenantName), new FindTenantByName(_tenantUnitOfWorkManager), _tenantUnitOfWorkManager);

			var result = target.FindApplicationLogonUsers(new[] { infoModel.ApplicationLogonName }).ToList();
			result.Count.Should().Be.EqualTo(1);
			result.First().ApplicationLogonName.Should().Be.EqualTo(infoModel.ApplicationLogonName);
			result.First().Identity.Should().Be.EqualTo(infoModel.Identity);
			result.First().PersonId.Should().Be.EqualTo(infoModel.PersonId);
		}
	}
}
