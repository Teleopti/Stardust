using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class BadgeCalculationModule : Module
	{
		private readonly IocConfiguration _config;

		public BadgeCalculationModule(IocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AgentBadgeCalculator>().As<IAgentBadgeCalculator>();
			builder.RegisterType<AgentBadgeWithRankCalculator>().As<IAgentBadgeWithRankCalculator>();
			builder.RegisterType<LogObjectDateChecker>().As<ILogObjectDateChecker>();
			if (_config.IsToggleEnabled(Toggles.WFM_Gamification_Calculate_Badges_47250))
			{
				builder.RegisterType<PerformAllBadgeCalculation>().As<IPerformBadgeCalculation>();
			}
			else
			{
				builder.RegisterType<PerformBadgeCalculation>().As<IPerformBadgeCalculation>();
			}
			builder.RegisterType<CalculateBadges>();
			builder.RegisterType<IsTeamGamificationSettingsAvailable>().As<IIsTeamGamificationSettingsAvailable>();
			builder.RegisterType<PushMessageSender>().As<IPushMessageSender>();
		}
	}
}