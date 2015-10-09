using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var setting = new GamificationSetting(Description)
			{
				Description = new Description(Description),
				SilverToBronzeBadgeRate = SilverToBronzeRate,
				GoldToSilverBadgeRate = GoldToSilverRate,
				AdherenceBadgeEnabled = AdherenceUsed,
				AHTBadgeEnabled = AHTUsed,
				AnsweredCallsBadgeEnabled = AnsweredCallsUsed
			};

			var rep = new GamificationSettingRepository(currentUnitOfWork);
			rep.Add(setting);
		}
	}
}
