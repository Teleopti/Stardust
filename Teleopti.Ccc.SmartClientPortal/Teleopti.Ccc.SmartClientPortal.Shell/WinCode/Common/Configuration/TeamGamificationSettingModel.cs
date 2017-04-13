using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public class TeamGamificationSettingModel : EntityContainer<ITeamGamificationSetting>
	{
		private readonly IGamificationSetting nullGamificationSetting = GamificationSettingProvider.NullGamificationSetting;

		public TeamGamificationSettingModel(ITeamGamificationSetting teamSetting)
		{
			// Sets the properties
			setContainedEntity(teamSetting);
		}

		public IGamificationSetting GamificationSetting
		{
			get
			{
				if (ContainedEntity.GamificationSetting == null) return nullGamificationSetting;
				return ContainedEntity.GamificationSetting;
			}
			set
			{
				if (nullGamificationSetting.Equals(value))
					value = null;

				ContainedEntity.GamificationSetting = value;
			}
		}

		public string SiteAndTeam
		{
			get { return ContainedEntity.Team.SiteAndTeam; }
		}

		public ITeam Team 
		{
			get { return ContainedEntity.Team; }
			set { ContainedEntity.Team = value; }
		}

		private void setContainedEntity(ITeamGamificationSetting teamSetting)
        {
			ContainedEntity = teamSetting.EntityClone();
			ContainedOriginalEntity = teamSetting;
        }

		public ITeamGamificationSetting ContainedOriginalEntity { get; private set; }

		public void UpdateAfterMerge(ITeamGamificationSetting updatedTeamSetting)
        {
			setContainedEntity(updatedTeamSetting);
        }
	}
}
