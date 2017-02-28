
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public interface IMaxSeatsCalculationForTeamBlock
	{
		double? PeriodValue(double periodValue, MaxSeatsFeatureOptions maxSeatsFeatureOption, bool isMaxSeatsReached, bool requiresSeat, double maxSeatBoostingFactor);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class MaxSeatsCalculationForTeamBlock : IMaxSeatsCalculationForTeamBlock
	{
		private const double theBigUglyNumber = -1000000;


		public double? PeriodValue(double periodValue, MaxSeatsFeatureOptions maxSeatsFeatureOption, bool isMaxSeatsReached, bool requiresSeat, double maxSeatBoostingFactor)
		{
			switch (maxSeatsFeatureOption)
			{
				case MaxSeatsFeatureOptions.DoNotConsiderMaxSeats:
					return periodValue;
				
				case MaxSeatsFeatureOptions.ConsiderMaxSeats:
					if (isMaxSeatsReached)
					{
						if (requiresSeat)
							return periodValue + ( theBigUglyNumber * maxSeatBoostingFactor);
					}
					break;
				
				case MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak:
					if (isMaxSeatsReached)
					{
						if (requiresSeat)
							return null;
					}

					break;
			}
			return periodValue;
		}
	}
}
