using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class TeamGamificationSettingConfigurable: IDataSetup
	{
		public string Team { get; set; }
		public string GamificationSetting { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var team = new TeamRepository(currentUnitOfWork).FindTeamByDescriptionName(Team).First();
			var setting = new GamificationSettingRepository(currentUnitOfWork).FindSettingByDescriptionName(GamificationSetting).First();

			var teamSetting = new TeamGamificationSetting {Team = team, GamificationSetting = setting};

			var teamSettingRepo = new TeamGamificationSettingRepository(currentUnitOfWork);
			teamSettingRepo.Add(teamSetting);

		}
	}
}
