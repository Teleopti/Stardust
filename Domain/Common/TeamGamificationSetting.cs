using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	[Serializable]
	public class TeamGamificationSetting : NonversionedAggregateRootWithBusinessUnit, ITeamGamificationSetting
	{
		private ITeam _team;
		private IGamificationSetting _gamificationSetting;

		/// <summary>
		/// Constructor for NHibernate
		/// </summary>
		public TeamGamificationSetting()
		{
			
		}

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
