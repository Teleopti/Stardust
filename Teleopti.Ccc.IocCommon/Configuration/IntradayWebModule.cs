using System;
using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Intraday;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class IntradayWebModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public IntradayWebModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<StaffingViewModelCreator>().SingleInstance();
			builder.RegisterType<ForecastedCallsProvider>().SingleInstance();
			builder.RegisterType<RequiredStaffingProvider>().SingleInstance();
			builder.RegisterType<ScheduledStaffingProvider>().SingleInstance();
			builder.RegisterType<TimeSeriesProvider>().SingleInstance();
			builder.RegisterType<ForecastedStaffingProvider>().SingleInstance();
			builder.RegisterType<ReforecastedStaffingProvider>().SingleInstance();
			builder.RegisterType<SupportedSkillsInIntradayProvider>().SingleInstance();
			builder.RegisterType<TaskPeriodsProvider>().SingleInstance();
			builder.RegisterType<FetchSkillInIntraday>().SingleInstance();
			builder.RegisterType<FetchSkillArea>().SingleInstance();
			builder.RegisterType<CreateSkillArea>().SingleInstance();
			builder.RegisterType<DeleteSkillArea>().SingleInstance();
			builder.RegisterType<LoadSkillInIntradays>().As<ILoadAllSkillInIntradays>().SingleInstance();
			builder.RegisterType<IntradayMonitorDataLoader>().As<IIntradayMonitorDataLoader>().SingleInstance();
			builder.RegisterType<LatestStatisticsIntervalIdLoader>().As<ILatestStatisticsIntervalIdLoader>().SingleInstance();
			builder.RegisterType<IncomingTrafficViewModelCreator>().SingleInstance();
			builder.RegisterType<PerformanceViewModelCreator>().SingleInstance();
			builder.RegisterType<EstimatedServiceLevelProvider>().SingleInstance();
			builder.RegisterType<LatestStatisticsTimeProvider>().SingleInstance();
			builder.RegisterType<ScheduleForecastSkillReadModelRepository>().As<IScheduleForecastSkillReadModelRepository>().SingleInstance();
			builder.RegisterType<IntradayQueueStatisticsLoader>().As<IIntradayQueueStatisticsLoader>().SingleInstance();
			builder.RegisterType<SplitSkillStaffInterval>().As<SplitSkillStaffInterval>().SingleInstance();
			builder.RegisterType<SkillStaffingIntervalProvider>().As<SkillStaffingIntervalProvider>().SingleInstance();
			builder.RegisterType<JobStartTimeRepository>().As<IJobStartTimeRepository>().SingleInstance();
			if (_configuration.Toggle(Toggles.AbsenceRequests_Intraday_UseCascading_41969))
				builder.RegisterType<SkillCombinationResourceRepository>().As<ISkillCombinationResourceRepository>().SingleInstance();
			else
				builder.RegisterType<SkillCombinationResourceRepositoryEmpty>().As<ISkillCombinationResourceRepository>().SingleInstance();
		}
	}


}