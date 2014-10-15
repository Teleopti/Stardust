using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class BadgeSettingsConfigurable : IDataSetup
	{
		public bool BadgeEnabled { get; set; }

		public int SilverToBronzeRate { get; set; }
		public int GoldToSilverRate { get; set; }
		public bool AnsweredCallsUsed { get; set; }
		public bool AdherenceUsed { get; set; }
		public bool AHTUsed { get; set; }

		public IAgentBadgeThresholdSettings Settings;

		public void Apply(IUnitOfWork uow)
		{
			var rep = new AgentBadgeSettingsRepository(uow);
			Settings = rep.GetSettings();

			Settings.EnableBadge = BadgeEnabled;
			Settings.SilverToBronzeBadgeRate = SilverToBronzeRate;
			Settings.GoldToSilverBadgeRate = GoldToSilverRate;
			Settings.AdherenceBadgeTypeSelected = AdherenceUsed;
			Settings.AHTBadgeTypeSelected = AHTUsed;
			Settings.AnsweredCallsBadgeTypeSelected = AnsweredCallsUsed;

			rep.PersistSettingValue(Settings);
		}
	}
}