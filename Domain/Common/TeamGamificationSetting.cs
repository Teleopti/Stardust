using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	[Serializable]
	public class TeamGamificationSetting : AggregateRoot_Events_ChangeInfo_BusinessUnit, ITeamGamificationSetting
	{
		private ITeam _team;
		private IGamificationSetting _gamificationSetting;

		public virtual ITeam Team
		{
			get { return _team; }
			set { _team = value; }
		}

		public virtual IGamificationSetting GamificationSetting
		{
			get { return _gamificationSetting; }
			set { _gamificationSetting = value; }
		}


		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public virtual ITeamGamificationSetting NoneEntityClone()
		{
			return (ITeamGamificationSetting)MemberwiseClone();
		}

		public virtual ITeamGamificationSetting EntityClone()
		{
			return (ITeamGamificationSetting)MemberwiseClone();
		}
	}
}
