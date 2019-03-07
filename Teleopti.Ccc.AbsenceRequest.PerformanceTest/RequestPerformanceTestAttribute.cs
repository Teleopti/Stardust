using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Requests.Core.IOC;
using Teleopti.Ccc.Web.Areas.TeamSchedule.IoC;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.AbsenceRequest.PerformanceTest
{
	public class RequestPerformanceTuningTestAttribute : IoCTestAttribute
	{
		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeInfraTestConfig();
			return config;
		}

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService<Database>();
			extend.AddModule(new LegacyRegistrationsFromAnywhere(configuration));
			extend.AddModule(new RequestsAreaModule());
			extend.AddModule(new TeamScheduleAreaModule());
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);

			var intervalFetcher = new FakeIntervalLengthFetcher();
			intervalFetcher.Has(15);
			isolate.UseTestDouble(intervalFetcher).For<IIntervalLengthFetcher>();

			isolate.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			isolate.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			isolate.UseTestDouble<MutableNow>().For<INow>();
		}

		protected override void BeforeInject(IComponentContext container)
		{
			base.BeforeInject(container);
			container.Resolve<IHangfireClientStarter>().Start();
		}
	}
}