using Autofac;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SkillGroupManagement;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class IntradayWebModule : Module
	{
		private readonly IocConfiguration _configuration;

		public IntradayWebModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ScheduledStaffingProvider>().As<IScheduledStaffingProvider>().SingleInstance();
			builder.RegisterType<ScheduledStaffingToDataSeries>().As<IScheduledStaffingToDataSeries>().SingleInstance();
			builder.RegisterType<ForecastedStaffingProvider>().As<IForecastedStaffingProvider>().SingleInstance();
			builder.RegisterType<ForecastedStaffingToDataSeries>().As<IForecastedStaffingToDataSeries>().SingleInstance();
			builder.RegisterType<SupportedSkillsInIntradayProvider>().As<ISupportedSkillsInIntradayProvider>().SingleInstance();
			builder.RegisterType<SkillTypeInfoProvider>().As<ISkillTypeInfoProvider>();
			builder.RegisterType<InboundPhoneSkillSupported>().As<ISupportedSkillCheck>();
			builder.RegisterType<OtherSkillsLikePhoneSupported>().As<ISupportedSkillCheck>();
			builder.RegisterType<EmailSkillSupported>().As<ISupportedSkillCheck>();
			builder.RegisterType<MultisiteSkillSupportedCheckAlwaysTrue>().As<IMultisiteSkillSupportedCheck>();
			builder.RegisterType<OtherSkillsLikeEmailSupported>().As<ISupportedSkillCheck>();
			builder.RegisterType<SkillTypeInfoTypesLikePhone>().As<ISkillTypeInfo>();
			builder.RegisterType<FetchSkillInIntraday>().SingleInstance();
			builder.RegisterType<SkillGroupViewModelBuilder>().SingleInstance();
			builder.RegisterType<AllSkillForSkillGroupProvider>().As<IAllSkillForSkillGroupProvider>();
			builder.RegisterType<CreateSkillGroup>().SingleInstance();
			builder.RegisterType<DeleteSkillGroup>().SingleInstance();
			builder.RegisterType<ModifySkillGroup>().SingleInstance();
			builder.RegisterType<LoadSkillInIntradays>().As<ILoadAllSkillInIntradays>().SingleInstance();
			builder.RegisterType<IntradayMonitorDataLoader>().As<IIntradayMonitorDataLoader>().SingleInstance();
			builder.RegisterType<LatestStatisticsIntervalIdLoader>().As<ILatestStatisticsIntervalIdLoader>().SingleInstance();

			builder.RegisterType<LatestStatisticsTimeProvider>().As<ILatestStatisticsTimeProvider>().SingleInstance();
			builder.RegisterType<ExtractSkillForecastIntervals>().SingleInstance();
			builder.RegisterType<IntradayQueueStatisticsLoader>().As<IIntradayQueueStatisticsLoader>().SingleInstance();
			builder.RegisterType<SplitSkillStaffInterval>().SingleInstance();
			builder.RegisterType<JobStartTimeRepository>().As<IJobStartTimeRepository>().SingleInstance();

			builder.RegisterType<SkillCombinationResourceRepository>().As<ISkillCombinationResourceRepository>()
					.SingleInstance();
			
			builder.RegisterType<EmailBacklogProvider>().As<IEmailBacklogProvider>().SingleInstance();
			builder.RegisterType<SkillStaffingIntervalProvider>().As<ISkillStaffingIntervalProvider>().SingleInstance();
			builder.RegisterType<IntradaySkillProvider>().As<IIntradaySkillProvider>().SingleInstance();

			// Intraday - application layer
			builder.RegisterType<IntradayStaffingApplicationService>();
			builder.RegisterType<IntradayPerformanceApplicationService>();
			builder.RegisterType<IntradayIncomingTrafficApplicationService>();

			// Intraday - domain layer
			builder.RegisterType<IntradayForecastingService>();
			builder.RegisterType<IntradayReforecastingService>();
			builder.RegisterType<IntradayStatisticsService>();
			builder.RegisterType<IntradayStaffingService>();
		}
	}
}