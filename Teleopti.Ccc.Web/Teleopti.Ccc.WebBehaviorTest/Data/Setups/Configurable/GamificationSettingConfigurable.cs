using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class GamificationSettingConfigurable : IDataSetup
	{
		public string Description { get; set; }
		public int SilverToBronzeRate { get; set; }
		public int GoldToSilverRate { get; set; }
		public bool AnsweredCallsUsed { get; set; }
		public bool AdherenceUsed { get; set; }
		public bool AHTUsed { get; set; }
		public GamificationRollingPeriodSet RollingPeriodSet { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var setting = new GamificationSetting(Description)
			{
				Description = new Description(Description),
				SilverToBronzeBadgeRate = SilverToBronzeRate,
				GoldToSilverBadgeRate = GoldToSilverRate,
				AdherenceBadgeEnabled = AdherenceUsed,
				AHTBadgeEnabled = AHTUsed,
				AnsweredCallsBadgeEnabled = AnsweredCallsUsed,
				RollingPeriodSet = RollingPeriodSet
			};

			var rep = GamificationSettingRepository.DONT_USE_CTOR(currentUnitOfWork);
			rep.Add(setting);
		}
	}
}
