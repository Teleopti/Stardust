using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class IdentityUserQueryTest : DatabaseTestWithoutTransaction
	{
		private Guid personId;
		private string correctIdentity;
		private IIdentityUserQuery target;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;
		private string tenantName;

		[Test]
		public void ShouldFindPersonId()
		{
			var result = target.FindUserData(correctIdentity);
			result.Id.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldFindTenant()
		{
			var result = target.FindUserData(correctIdentity);
			result.Tenant.Name.Should().Be.EqualTo(tenantName);
		}

		[Test]
		public void NonExistingUserShouldFail()
		{
			var result = target.FindUserData("incorrectUserName");
			result.Should().Be.Null();
		}


		[SetUp]
		public void Setup_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			correctIdentity = RandomName.Make();
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConnectionStringHelper.ConnectionStringUsedInTests);
			tenantName = RandomName.Make();
			var tenant = new Tenant(tenantName);
			_tenantUnitOfWorkManager.CurrentSession().Save(tenant);
			personId = Guid.NewGuid();
			var pInfo = new PersonInfo(tenant, personId);
			pInfo.SetIdentity(correctIdentity);
			var personInfoPersister = new PersistPersonInfo(_tenantUnitOfWorkManager);
			personInfoPersister.Persist(pInfo);
			_tenantUnitOfWorkManager.CurrentSession().Flush();
			target = new IdentityUserQuery(_tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Teardown_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			_tenantUnitOfWorkManager.Dispose();
		}
	}
}