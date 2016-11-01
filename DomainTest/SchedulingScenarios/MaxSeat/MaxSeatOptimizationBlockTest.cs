using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	public class MaxSeatOptimizationBlockTest : MaxSeatOptimizationTest
	{
		protected override OptimizationPreferences CreateOptimizationPreferences()
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