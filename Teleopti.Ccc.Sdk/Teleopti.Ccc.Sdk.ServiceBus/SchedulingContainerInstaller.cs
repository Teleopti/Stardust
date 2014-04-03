using System;
using Autofac;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class SchedulingContainerInstaller : Module
    {
		[ThreadStatic]private static ISchedulingResultStateHolder schedulingResultStateHolder;

		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(getSchedulingResultStateHolder).As<ISchedulingResultStateHolder>().InstancePerDependency().ExternallyOwned();

			builder.RegisterType<SwapService>().As<ISwapService>();
			builder.RegisterType<SwapAndModifyService>().As<ISwapAndModifyService>();
			builder.RegisterType<ResourceCalculationOnlyScheduleDayChangeCallback>().As<IScheduleDayChangeCallback>();
			builder.RegisterType<OccupiedSeatCalculator>().As<IOccupiedSeatCalculator>();
			builder.RegisterType<SkillVisualLayerCollectionDictionaryCreator>().As<ISkillVisualLayerCollectionDictionaryCreator>();
			builder.RegisterType<SeatImpactOnPeriodForProjection>().As<ISeatImpactOnPeriodForProjection>();
			builder.RegisterType<NonBlendSkillCalculator>().As<INonBlendSkillCalculator>();
			builder.RegisterType<NonBlendSkillImpactOnPeriodForProjection>().As<INonBlendSkillImpactOnPeriodForProjection>();
			builder.RegisterType<PersonAbsenceAccountProvider>().As<IPersonAbsenceAccountProvider>();
			builder.RegisterType<ScheduleIsInvalidSpecification>().As<IScheduleIsInvalidSpecification>();
			builder.RegisterType<PeopleAndSkillLoaderDecider>().As<IPeopleAndSkillLoaderDecider>();
			builder.RegisterType<ResourceOptimizationHelper>().As<IResourceOptimizationHelper>();
			builder.RegisterType<PeriodDistributionService>().As<IPeriodDistributionService>();
			builder.RegisterType<LoadSchedulingStateHolderForResourceCalculation>().As<ILoadSchedulingStateHolderForResourceCalculation>();
			builder.RegisterType<LoadSchedulesForRequestWithoutResourceCalculation>().As<ILoadSchedulesForRequestWithoutResourceCalculation>();
			builder.RegisterType<SignificantChangeChecker>().As<ISignificantChangeChecker>();
			builder.RegisterType<SmsLinkChecker>().As<ISmsLinkChecker>();
			builder.RegisterType<NotificationSenderFactory>().As<INotificationSenderFactory>();
			builder.RegisterType<NotificationConfigReader>().As<INotificationConfigReader>();
			builder.RegisterType<SingleSkillDictionary>().As<ISingleSkillDictionary>();
			builder.RegisterType<CurrentTeleoptiPrincipal>().As<ICurrentTeleoptiPrincipal>().SingleInstance();
			builder.RegisterType<PersonSkillProvider>().As<IPersonSkillProvider>();
			builder.RegisterType<ResourceCalculationPrerequisitesLoader>().As<IResourceCalculationPrerequisitesLoader>();
		}

		private static ISchedulingResultStateHolder getSchedulingResultStateHolder(IComponentContext componentContext)
		{
			return schedulingResultStateHolder ?? (schedulingResultStateHolder = new SchedulingResultStateHolder());
		}
    }
}