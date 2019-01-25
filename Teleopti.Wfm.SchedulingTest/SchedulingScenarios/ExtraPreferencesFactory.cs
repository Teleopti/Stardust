using System;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios
{
	public static class ExtraPreferencesFactory
	{
		public static ExtraPreferences CreateExtraPreferences(this TeamBlockType teamBlockType)
		{
			switch (teamBlockType)
			{
				case TeamBlockType.Individual:
					return new ExtraPreferences { UseTeams = false, UseTeamBlockOption = false };
				case TeamBlockType.Block:
					return new ExtraPreferences { UseTeams = false, UseTeamBlockOption = true };
				case TeamBlockType.Team:
					return new ExtraPreferences { UseTeams = true, UseTeamBlockOption = false };
				case TeamBlockType.TeamAndBlock:
					return new ExtraPreferences { UseTeams = true, UseTeamBlockOption = true };
				default:
					throw new ArgumentOutOfRangeException(nameof(teamBlockType), teamBlockType, null);
			}
		}
	}
}