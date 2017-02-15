using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public interface IMaxSeatBoostingFactorCalculator
	{
		double GetBoostingFactor(double currentSeats, double maxSeats);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class MaxSeatBoostingFactorCalculator : IMaxSeatBoostingFactorCalculator
	{
		public double GetBoostingFactor(double currentSeats, double maxSeats)
		{
			if (currentSeats > maxSeats)
			{
				return currentSeats - maxSeats;
			}
			return 1;
		}
	}
}