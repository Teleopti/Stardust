using Autofac;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Staffing.PerformanceTest
{
	public class StaffingPerformanceTestAttribute : IoCTestAttribute
	{
		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			return config;
		}

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService<Database>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			var intervalFetcher = new FakeIntervalLengthFetcher();
			intervalFetcher.Has(15);  //because we don't restore Analytics
			base.Isolate(isolate);
			isolate.UseTestDouble<ScenarioRepository>().For<IScenarioRepository>();
			isolate.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			isolate.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			isolate.UseTestDouble<MutableNow>().For<INow>();
			isolate.UseTestDouble(intervalFetcher).For<IIntervalLengthFetcher>();
			isolate.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			
			isolate.UseTestDouble<UpdateSkillForecastReadModel>().For<UpdateSkillForecastReadModel>();
			isolate.UseTestDouble<SkillForecastIntervalCalculator>().For<SkillForecastIntervalCalculator>();
			isolate.UseTestDouble<SkillForecastReadModelPeriodBuilder>().For<SkillForecastReadModelPeriodBuilder>();
			isolate.UseTestDouble<SkillForecastSettingsReader>().For<SkillForecastSettingsReader>();
			isolate.UseTestDouble<StaffingSettingsReader49Days>().For<IStaffingSettingsReader>();
		}

		protected override void Startup(IComponentContext container)
		{
			base.Startup(container);
			container.Resolve<IHangfireClientStarter>().Start();
		}
	}
}