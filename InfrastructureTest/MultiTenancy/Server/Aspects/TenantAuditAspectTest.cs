using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Aspects;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Aspects
{
	[TestFixture]
	[UnitOfWorkTest]
	public class TenantAuditAspectTest : IIsolateSystem, IExtendSystem
	{
		public TestAuditService Target;
		public ICurrentTenantSession CurrentTenantSession;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentHttpContext CurrentHttpContext;

		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<TestAuditService>();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			isolate.UseTestDouble<FakeCurrentHttpContext>().For<ICurrentHttpContext>();
		}
		
		[Test]
		public void TenantAuditAspectShouldSavesAuditInformation()
		{
			var personInfo = new PersonInfo(new Tenant("_"), Guid.NewGuid());
			CurrentHttpContext.Current().Items[WebTenantAuthenticationConfiguration.PersonInfoKey] = personInfo;

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				Target.DoSomething(new AppLogonChangeActionObj(){PersonInfo = personInfo});
				var session = CurrentTenantSession.CurrentSession();
				session.FlushAndClear();
				session.Query<TenantAudit>().ToList().Count.Should().Be.EqualTo(1);
			}
		}

		public class TestAuditService
		{
			[TenantAudit(PersistActionIntent.AppLogonChange)]
			public virtual void DoSomething(AppLogonChangeActionObj personInfo)
			{
			}
		}
	}
}
