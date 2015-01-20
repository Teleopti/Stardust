using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
	public interface ITeamGamificationSetting : IAggregateRoot, IBelongsToBusinessUnit, ICloneableEntity<ITeamGamificationSetting>
	{
		ITeam Team { get; set; }
		IGamificationSetting GamificationSetting { get; set; }
	}
}
