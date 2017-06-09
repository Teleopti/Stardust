using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;

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
			builder.RegisterType<ForecastedCallsProvider>().SingleInstance();
			builder.RegisterType<RequiredStaffingProvider>().SingleInstance();
			builder.RegisterType<ScheduledStaffingProvider>().SingleInstance();
			builder.RegisterType<ScheduledStaffingToDataSeries>().SingleInstance();
			builder.RegisterType<TimeSeriesProvider>().SingleInstance();
			builder.RegisterType<ForecastedStaffingProvider>().SingleInstance();
			builder.RegisterType<ForecastedStaffingToDataSeries>().SingleInstance();
			builder.RegisterType<ReforecastedStaffingProvider>().SingleInstance();
			builder.RegisterType<SupportedSkillsInIntradayProvider>().As<ISupportedSkillsInIntradayProvider>().SingleInstance();
			builder.RegisterType<InboundPhoneSkillSupported>().As<ISupportedSkillCheck>();
			if (_configuration.Toggle(Toggles.Wfm_Intraday_SupportSkillTypeWebChat_42591))
				builder.RegisterType<OtherSkillsLikePhoneSupported>().As<ISupportedSkillCheck>();
			if (_configuration.Toggle(Toggles.Wfm_Intraday_SupportSkillTypeEmail_44002))
				builder.RegisterType<EmailSkillSupported>().As<ISupportedSkillCheck>();
			if (_configuration.Toggle(Toggles.WFM_Intraday_SupportMultisiteSkill_43874))
				builder.RegisterType<MultisiteSkillSupportedCheckAlwaysTrue>().As<IMultisiteSkillSupportedCheck>();
			else
				builder.RegisterType<MultisiteSkillSupportedCheck>().As<IMultisiteSkillSupportedCheck>();

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

			if (_configuration.Toggle(Toggles.Staffing_ReadModel_Keep8DaysHistoricalData_44652))
				builder.RegisterType<SkillCombinationResourceRepository>().As<ISkillCombinationResourceRepository>().SingleInstance();
			else
				builder.RegisterType<SkillCombinationResourceRepositoryOld>().As<ISkillCombinationResourceRepository>().SingleInstance();

			if (_configuration.Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx))
				builder.RegisterType<SkillStaffingIntervalProvider>().As<ISkillStaffingIntervalProvider>().SingleInstance();
			else
				builder.RegisterType<SkillStaffingIntervalProviderOldReadModel>().As<ISkillStaffingIntervalProvider>().SingleInstance();
		}
	}


}