using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.Hangfire;
using Teleopti.Wfm.Administration.Core.Modules;

namespace Teleopti.Wfm.Administration.IntegrationTest
{
	public class WfmAdminTestAttribute : IoCTestAttribute
	{
		public IHangfireClientStarter HangfireClientStarter;

		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("MessageBroker", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("RtaTracer", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("Toggle", InfraTestConfigReader.ConnectionString);
			return config;
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();
			HangfireClientStarter.Start();
		}

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddModule(new WfmAdminModule(configuration));
			extend.AddService<DbPathProviderFake>();
			extend.AddService<CheckPasswordStrengthFake>();
			extend.AddService<TestPollutionCleaner>();
			extend.AddService<LoadAllTenants>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);
			isolate.UseTestDouble<ConsoleLogger>().For<IUpgradeLog>();
			isolate.UseTestDouble<FakeHangfireCookie>().For<IHangfireCookie>();
			isolate.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			isolate.UseTestDouble<PersistedTypeMapperForTest>().For<PersistedTypeMapper>();
		}
	}
}