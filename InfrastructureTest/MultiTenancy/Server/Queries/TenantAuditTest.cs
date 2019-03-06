using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using NHibernate.Impl;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	public class TenantAuditTest
	{
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;
		private ITenantAuditPersister target;

		[SetUp]
		public void Setup()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ApplicationConnectionString());
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
			var tenantAudit = new TenantAudit(Guid.NewGuid(), Guid.NewGuid(), "action", "{ \"Data\" : \"SomeData\" }", Guid.NewGuid());
			target.Persist(tenantAudit);
			
			var theEntry = session.Get<TenantAudit>(tenantAudit.Id);
			session.Query<TenantAudit>().ToList().Count.Should().Be.EqualTo(1);

			theEntry.Action.Should().Be.EqualTo(tenantAudit.Action);
			theEntry.Correlation.Should().Be.EqualTo(((SessionImpl)session).SessionId);
			theEntry.ActionPerformedOn.Should().Be.EqualTo(tenantAudit.ActionPerformedOn);
			theEntry.ActionPerformedBy.Should().Be.EqualTo(tenantAudit.ActionPerformedBy);
		}
	}
}