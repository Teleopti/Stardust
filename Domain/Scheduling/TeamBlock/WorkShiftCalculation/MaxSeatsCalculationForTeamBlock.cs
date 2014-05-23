using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IMaxSeatsCalculationForTeamBlock
	{
		double? PeriodValue(double periodValue, MaxSeatsFeatureOptions maxSeatsFeatureOption, bool isMaxSeatsReached);
	}

	public class MaxSeatsCalculationForTeamBlock : IMaxSeatsCalculationForTeamBlock
	{
		private const int theBigNumber = -10000;


		public double? PeriodValue(double periodValue, MaxSeatsFeatureOptions maxSeatsFeatureOption, bool isMaxSeatsReached)
		{
			switch (maxSeatsFeatureOption)
			{
				case MaxSeatsFeatureOptions.DoNotConsiderMaxSeats:
					return periodValue;
				case MaxSeatsFeatureOptions.ConsiderMaxSeats:
					if(isMaxSeatsReached)
						return periodValue*theBigNumber;
					break;
				case MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak:
					if (isMaxSeatsReached)
						return null;
					break;
			}
			return periodValue;
		}
	}

	public enum MaxSeatsFeatureOptions
	{
		DoNotConsiderMaxSeats = 0,
		ConsiderMaxSeats = 1,
		ConsiderMaxSeatsAndDoNotBreak = 2
	}

}
