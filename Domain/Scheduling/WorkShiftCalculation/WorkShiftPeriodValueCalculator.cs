using System;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface IWorkShiftPeriodValueCalculator
	{
		double PeriodValue(ISkillIntervalData skillIntervalData, int addedResourceInMinutes, bool useMinimumPersons, bool useMaximumPersons, double overStaffingFactor, double priorityFactor);
	}

	public class WorkShiftPeriodValueCalculator : IWorkShiftPeriodValueCalculator
	{
		const int theBigNumber = 100000;

		public double PeriodValue(ISkillIntervalData skillIntervalData, int addedResourceInMinutes, bool useMinimumPersons, bool useMaximumPersons, double overStaffingFactor, double priorityFactor)
		{
			double partOfResolution = addedResourceInMinutes / skillIntervalData.Period.ElapsedTime().TotalMinutes;

			double intervalLengthInMinutes = skillIntervalData.Period.ElapsedTime().TotalMinutes;
			double forecastedDemand = skillIntervalData.ForecastedDemand;
			double tweakedCurrentDamand = getTweakedCurrentDemand(skillIntervalData.CurrentDemand, overStaffingFactor,
			                                                      priorityFactor);
			double calculatedValue =
				calculateWorkShiftPeriodValue(forecastedDemand * intervalLengthInMinutes * partOfResolution,
											  tweakedCurrentDamand * intervalLengthInMinutes * partOfResolution,
				                              addedResourceInMinutes);

			//double assignedResourceInMinutes = (forecastedDemand - currentDemand) * intervalLengthInMinutes;
			double corrFactor;
			//if (assignedResourceInMinutes == 0)  //eller ska man bara boosta första och sista intervall=
			//    corrFactor = TheBigNumber;
			//else
			corrFactor = getCorrectionFactor(useMinimumPersons, useMaximumPersons, skillIntervalData);

			calculatedValue += corrFactor;

			return calculatedValue;
		}


		private static double calculateWorkShiftPeriodValue(double forecastedDemandInMinutes, double tweakedCurrentDemandInMinutes, int currentResourceInMinutes)
		{
			if (forecastedDemandInMinutes == 0)
				return 0;
			int logicalSign = Math.Sign(tweakedCurrentDemandInMinutes);
			double weightedCurrentDemand = tweakedCurrentDemandInMinutes * tweakedCurrentDemandInMinutes;

			double oldValue = (weightedCurrentDemand / forecastedDemandInMinutes) * logicalSign;
			double afterAddingCurrent = tweakedCurrentDemandInMinutes + currentResourceInMinutes;

			int nextLogicalSign = Math.Sign(afterAddingCurrent);
			weightedCurrentDemand = afterAddingCurrent * afterAddingCurrent;
			double newValue = (weightedCurrentDemand / forecastedDemandInMinutes) * nextLogicalSign;

			return (oldValue - newValue) * logicalSign;
		}

		private static double getCorrectionFactor(bool useMinimumPersons, bool useMaximumPersons, ISkillIntervalData skillIntervalData)
		{

			if (!useMinimumPersons && !useMaximumPersons)
				return 0;

			if(useMinimumPersons && skillIntervalData.MinimumHeads.HasValue)
			{
				if (skillIntervalData.CurrentHeads >= skillIntervalData.MinimumHeads.Value)
					return 0;
				return (skillIntervalData.MinimumHeads.Value - skillIntervalData.CurrentHeads)*theBigNumber;
			}

			if(useMaximumPersons && skillIntervalData.MaximumHeads.HasValue)
			{
				if (skillIntervalData.CurrentHeads + 1 <= skillIntervalData.MaximumHeads.Value)
					return 0;
				return -(skillIntervalData.CurrentHeads + 1 - skillIntervalData.MaximumHeads.Value)*theBigNumber;
			}

			return 0;
		}

		private double getTweakedCurrentDemand(double currentDemandInMinutes, double overStaffingFactor, double priorityFactor)
		{
			double overUnderStaffFaktor = overStaffingFactor;
			if (currentDemandInMinutes < 0)
				overUnderStaffFaktor = 1 - overStaffingFactor;
			return priorityFactor * overUnderStaffFaktor * currentDemandInMinutes;
		}
	}
}