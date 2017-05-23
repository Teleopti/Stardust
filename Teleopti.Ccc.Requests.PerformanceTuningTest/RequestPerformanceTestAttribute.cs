﻿using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Anywhere.Core.IoC;
using Teleopti.Ccc.Web.Areas.TeamSchedule.IoC;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{
	public class RequestPerformanceTuningTestAttribute : IoCTestAttribute
	{
		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			return config;
		}

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			var intervalFetcher = new FakeIntervalLengthFetcher();
			intervalFetcher.Has(15);
			system.UseTestDouble(intervalFetcher).For<IIntervalLengthFetcher>();

			system.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			system.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			system.UseTestDouble<UpdateStaffingLevelReadModel>().For<UpdateStaffingLevelReadModel>();
			system.UseTestDouble<MultiAbsenceRequestsHandler>().For<MultiAbsenceRequestsHandler>();
			system.UseTestDouble<MutableNow>().For<INow>();
			system.AddService<Database>();
			system.AddModule(new TenantServerModule(configuration));
			system.AddModule(new AnywhereAreaModule(configuration));
			system.AddModule(new TeamScheduleAreaModule());
			system.AddModule(new CommonModule(configuration));
			system.AddModule(new RuleSetModule(configuration,true));
		}

		protected override void Startup(IComponentContext container)
		{
			base.Startup(container);
			container.Resolve<HangfireClientStarter>().Start();
		}
	}
}