using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class BadgeCalculationModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AgentBadgeCalculator>().As<IAgentBadgeCalculator>();
			builder.RegisterType<AgentBadgeWithRankCalculator>().As<IAgentBadgeWithRankCalculator>();
			builder.RegisterType<LogObjectDateChecker>().As<ILogObjectDateChecker>();
			builder.RegisterType<PerformAllBadgeCalculation>().As<IPerformBadgeCalculation>();
			builder.RegisterType<CalculateBadges>();
			builder.RegisterType<IsTeamGamificationSettingsAvailable>().As<IIsTeamGamificationSettingsAvailable>();
			builder.RegisterType<PushMessageSender>().As<IPushMessageSender>();
		}
	}
}