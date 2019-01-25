using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.MaxSeat.TestData
{
	public static class DefaultMaxSeatOptimizationPreferences
	{
		public static OptimizationPreferences Create(TeamBlockType teamBlockType)
		{
			switch (teamBlockType)
			{
				case TeamBlockType.Block:
					return forBlock();
				case TeamBlockType.Team:
					return forTeam();
				default:
					throw new ArgumentOutOfRangeException(nameof(teamBlockType), teamBlockType, null);
			}
		}

		private static OptimizationPreferences forTeam()
		{
			return new OptimizationPreferences
			{
				Extra =
				{
					UseTeams = true,
					TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy)
				},

				Advanced =
				{
					UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats
				}
			};
		}

		private static OptimizationPreferences forBlock()
		{
			return new OptimizationPreferences
			{
				Extra =
				{
					UseTeamBlockOption = true,
					BlockTypeValue = BlockFinderType.SchedulePeriod,
					TeamGroupPage = new GroupPageLight("_", GroupPageType.SingleAgent)
				},

				Advanced =
				{
					UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats
				}
			};
		}
	}
}