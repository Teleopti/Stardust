using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class ApplicationUserQueryTest
	{
		private Guid personId;
		private string correctUserName;
		private string tenantName;
		private IApplicationUserQuery target;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;


		[Test]
		public void ShouldFindPersonId()
		{
			var result = target.Find(correctUserName);
			result.Id.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldFindTenant()
		{
			var result = target.Find(correctUserName);
			result.Tenant.Name.Should().Be.EqualTo(tenantName);
		}

		[Test]
		public void ShouldReturnNullIfNotFound()
		{
			target.Find("not existing")
				.Should().Be.Null();
		}


		[Test]
		public void ShouldFindPassword()
		{
			var result = target.Find(correctUserName);
			result.ApplicationLogonInfo.ApplicationLogonPassword.Should().Not.Be.Null();
		}


		[SetUp]
		public void Setup()
		{
			correctUserName = RandomName.Make();
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConnectionStringHelper.ConnectionStringUsedInTests);
			tenantName = RandomName.Make();
			var tenant = new Tenant(tenantName);
			_tenantUnitOfWorkManager.CurrentSession().Save(tenant);
			personId = Guid.NewGuid();
			var pInfo = new PersonInfo(tenant, personId);
			pInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), correctUserName, RandomName.Make());
			var personInfoPersister = new PersistPersonInfo(_tenantUnitOfWorkManager);
			personInfoPersister.Persist(pInfo);
			_tenantUnitOfWorkManager.CurrentSession().Flush();
			target = new ApplicationUserQuery(_tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Clean()
		{
			_tenantUnitOfWorkManager.Dispose();
		}
	}
}