using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class TeamGamificationSettingConfigurable: IDataSetup
	{
		public string Team { get; set; }
		public string GamificationSetting { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var team = TeamRepository.DONT_USE_CTOR(currentUnitOfWork).FindTeamByDescriptionName(Team).First();
			var setting = GamificationSettingRepository.DONT_USE_CTOR(currentUnitOfWork).FindSettingByDescriptionName(GamificationSetting).First();

			var teamSetting = new TeamGamificationSetting {Team = team, GamificationSetting = setting};

			var teamSettingRepo = new TeamGamificationSettingRepository(currentUnitOfWork);
			teamSettingRepo.Add(teamSetting);

		}
	}
}
