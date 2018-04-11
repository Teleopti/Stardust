using NUnit.Framework;
using SharpTestsEx;
using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	public class TenantAuditTest
	{
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;
		private IAuditPersister target;

		[SetUp]
		public void Setup()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			_tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
			target = new TenantAuditPersister(_tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Cleanup()
		{
			_tenantUnitOfWorkManager.Dispose();
		}

		[Test]
		public void ShouldInsertTenantAudit()
		{
			var session = _tenantUnitOfWorkManager.CurrentSession(); 
			var tenantAudit = new TenantAudit(Guid.NewGuid(), Guid.NewGuid(), "action", "actionResult", "{ \"Data\" : \"SomeData\" }", Guid.NewGuid());
			target.Persist(tenantAudit);

			session.Flush();
			session.Clear();
			session.Get<TenantAudit>(tenantAudit.Id).Id.Should().Be.EqualTo(tenantAudit.Id);
		}
	}
}