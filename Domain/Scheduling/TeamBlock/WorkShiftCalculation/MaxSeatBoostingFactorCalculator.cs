﻿namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public class MaxSeatBoostingFactorCalculator 
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