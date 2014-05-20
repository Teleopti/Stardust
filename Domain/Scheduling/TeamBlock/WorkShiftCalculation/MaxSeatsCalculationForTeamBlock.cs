using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IMaxSeatsCalculationForTeamBlock
	{
		double? PeriodValue(double periodValue, UseMaxSeatsOptions option, bool isMaxSeatsReached);
	}

	public class MaxSeatsCalculationForTeamBlock : IMaxSeatsCalculationForTeamBlock
	{
		private const int theBigNumber = -10000;


		public double? PeriodValue(double periodValue, UseMaxSeatsOptions option, bool isMaxSeatsReached)
		{
			switch (option)
			{
				case UseMaxSeatsOptions.DoNotConsiderMaxSeats:
					return periodValue;
				case UseMaxSeatsOptions.ConsiderMaxSeats:
					if(isMaxSeatsReached)
						return periodValue*theBigNumber;
					break;
				case UseMaxSeatsOptions.ConsiderMaxSeatsAndDoNotBreak:
					if (isMaxSeatsReached)
						return null;
					break;
			}
			return periodValue;
		}
	}

	public enum UseMaxSeatsOptions
	{
		DoNotConsiderMaxSeats = 0,
		ConsiderMaxSeats = 1,
		ConsiderMaxSeatsAndDoNotBreak = 2
	}

}
