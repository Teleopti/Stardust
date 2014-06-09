
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IMaxSeatsCalculationForTeamBlock
	{
		double? PeriodValue(double periodValue, MaxSeatsFeatureOptions maxSeatsFeatureOption, bool isMaxSeatsReached, bool requiresSeat, double maxSeatBoostingFactor);
	}

	public class MaxSeatsCalculationForTeamBlock : IMaxSeatsCalculationForTeamBlock
	{
		private const double theBigNiceNumber = 1000;
		private const int theBigUglyNumber = -10000;


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
							return periodValue * theBigUglyNumber;
					}
					else
						return periodValue * maxSeatBoostingFactor * theBigNiceNumber;
					break;
				
				case MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak:
					if (isMaxSeatsReached)
					{
						if (requiresSeat)
							return null;
					}
					else
						return periodValue * maxSeatBoostingFactor * theBigNiceNumber;
					break;
			}
			return periodValue;
		}
	}
}
