using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Aspects;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Aspects
{
	[TestFixture]
	[UnitOfWorkTest]
	public class TenantAuditAspectTest : ISetup
	{
		public TestAuditService Target;
		public TenantUnitOfWorkManager _tenantUnitOfWorkManager;
		public ITenantUnitOfWork TenantUnitOfWork;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TestAuditService>();
			system.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
		}
		
		[Test]
		public void TenantAuditAspectShouldSavesAuditInformation()
		{
			using (_tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted())
			{
				var personInfo = new PersonInfo(new Tenant("_"), Guid.NewGuid());
				Target.DoSomething(personInfo, PersistActionIntent.AppLogonChange);

				var session = _tenantUnitOfWorkManager.CurrentSession();
				session.FlushAndClear();
				session.Query<TenantAudit>().ToList().Count.Should().Be.EqualTo(1);
			}
		}

		public class TestAuditService
		{
			[TenantAudit]
			public virtual void DoSomething(PersonInfo personInfo, PersistActionIntent actionIntent)
			{
			}
		}
	}
}
