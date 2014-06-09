namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public class MaxSeatBoostingFactorCalculator 
	{
		public double GetBoostingFactor(double currentSeats, double maxSeats)
		{
			return maxSeats - currentSeats;
		}
	}
}