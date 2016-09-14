using Autofac;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class SchedulingContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SeatImpactOnPeriodForProjection>().As<ISeatImpactOnPeriodForProjection>();
			builder.RegisterType<NonBlendSkillImpactOnPeriodForProjection>().As<INonBlendSkillImpactOnPeriodForProjection>();
			builder.RegisterType<SignificantChangeChecker>().As<ISignificantChangeChecker>();		
			builder.RegisterType<IntraIntervalFinderService>().As<IIntraIntervalFinderService>();
			builder.RegisterType<BusinessRulesForPersonalAccountUpdate>().As<IBusinessRulesForPersonalAccountUpdate>();
		}
	}
}