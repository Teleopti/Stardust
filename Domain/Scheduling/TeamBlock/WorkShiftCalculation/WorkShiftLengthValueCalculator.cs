using System;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IWorkShiftLengthValueCalculator
	{
		double CalculateShiftValueForPeriod(double oldPeriodValue, double resourceInMinutes, WorkShiftLengthHintOption workShiftLengthHint);
	}

	public class WorkShiftLengthValueCalculator : IWorkShiftLengthValueCalculator
	{
		public double CalculateShiftValueForPeriod(double oldPeriodValue, double resourceInMinutes, WorkShiftLengthHintOption workShiftLengthHint)
		{
			double factor = (resourceInMinutes - 1)/100;
			if (workShiftLengthHint == WorkShiftLengthHintOption.Long)
			{
				double correctedValue = oldPeriodValue + (Math.Abs(oldPeriodValue)*factor);
				return correctedValue;
			}

			if (workShiftLengthHint == WorkShiftLengthHintOption.Short)
			{
				double correctedValue = oldPeriodValue - (Math.Abs(oldPeriodValue)*factor);
				return correctedValue;
			}

			return oldPeriodValue;
		}
	}
}