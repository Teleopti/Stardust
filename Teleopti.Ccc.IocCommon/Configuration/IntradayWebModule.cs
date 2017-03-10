﻿using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
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
			builder.RegisterType<StaffingViewModelCreator>().As<IStaffingViewModelCreator>().SingleInstance();
			builder.RegisterType<CacheableStaffingViewModelCreator>().As<ICacheableStaffingViewModelCreator>().SingleInstance();
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
			if(_configuration.Toggle(Toggles.StaffingActions_RemoveScheduleForecastSkillChangeReadModel_43388))
				builder.RegisterType<ExtractSkillForecastIntervals>().As<IExtractSkillForecastIntervals>().SingleInstance();
			else
				builder.RegisterType<SkillForecastIntervalsFromReadModel>().As<IExtractSkillForecastIntervals>().SingleInstance();
			builder.RegisterType<IntradayQueueStatisticsLoader>().As<IIntradayQueueStatisticsLoader>().SingleInstance();
			builder.RegisterType<SplitSkillStaffInterval>().As<SplitSkillStaffInterval>().SingleInstance();
			builder.RegisterType<JobStartTimeRepository>().As<IJobStartTimeRepository>().SingleInstance();
			builder.RegisterType<SkillCombinationResourceRepository>().As<ISkillCombinationResourceRepository>().SingleInstance();

			if (_configuration.Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_42663))
				builder.RegisterType<SkillStaffingIntervalProvider>().As<ISkillStaffingIntervalProvider>().SingleInstance();
			else
				builder.RegisterType<SkillStaffingIntervalProviderOldReadModel>().As<ISkillStaffingIntervalProvider>().SingleInstance();
		}
	}


}