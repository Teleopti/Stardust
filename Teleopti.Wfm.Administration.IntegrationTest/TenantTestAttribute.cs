using System.Configuration;
using System.IO;
using NUnit.Framework;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.Hangfire;
using Teleopti.Wfm.Administration.Core.Modules;

namespace Teleopti.Wfm.Administration.IntegrationTest
{
	public class TenantTestAttribute : IoCTestAttribute
	{
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		protected override void Extend(IExtend extend, IIocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddModule(new WfmAdminModule2(configuration));
			extend.AddService(_tenantUnitOfWorkManager);
			extend.AddService<DbPathProviderFake>();
			extend.AddService<CheckPasswordStrengthFake>();
			extend.AddService<TestPollutionCleaner>();
			extend.AddService<LoadAllTenants>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);

			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);

			isolate.UseTestDouble<ConsoleLogger>().For<IUpgradeLog>();
			isolate.UseTestDouble<FakeHangfireCookie>().For<IHangfireCookie>();
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			isolate.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();

			_tenantUnitOfWorkManager = TenantUnitOfWorkForTest();
		}

		public static TenantUnitOfWorkManager TenantUnitOfWorkForTest()
		{
			return TenantUnitOfWorkManager.Create(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			_tenantUnitOfWorkManager.CancelAndDisposeCurrent();
		}
	}
}