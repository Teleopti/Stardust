using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IWorkShiftCalculator
	{
		double CalculateShiftValue(
			IEnumerable<IWorkShiftCalculatableLayer> mainShiftLayers,
			IWorkShiftCalculatorSkillStaffPeriodData skillStaffPeriodData,
			WorkShiftLengthHintOption lengthFactor,
			bool useMinimumPersons,
			bool useMaximumPersons,
			Func<TimeSpan, int, TimeSpan> fitToDefaultResolution
		);

		double CalculateShiftValueForPeriod(
			double oldPeriodValue,
			int resourceInMinutes,
			WorkShiftLengthHintOption lengthFactor,
			int resolution
		);

	}

	public class NonSecretWorkShiftCalculatorClassic : IWorkShiftCalculator
	{
		public double CalculateShiftValue(
		  IEnumerable<IWorkShiftCalculatableLayer> mainShiftLayers,
		  IWorkShiftCalculatorSkillStaffPeriodData skillStaffPeriodData,
		  WorkShiftLengthHintOption lengthFactor,
		  bool useMinimumPersons,
		  bool useMaximumPersons,
			Func<TimeSpan, int, TimeSpan> fitToDefaultResolution)
		{
			if (skillStaffPeriodData.Empty())
				return double.MinValue;

			double periodValue = 0;
			int resourceInMinutes = 0;
			int resolution = 1;
			foreach (IWorkShiftCalculatableLayer layer in mainShiftLayers)
			{
				// If the person has a skill that this layer's activity corresponds to
				IWorkShiftCalculatableActivity activity = layer.Activity;
				var staffPeriodData = skillStaffPeriodData.ForActivity(activity);
				if (staffPeriodData != null)
				{
					resolution = staffPeriodData.Resolution();
					var currentResourceInMinutes = resolution;

					DateTime layerStart = layer.PeriodStartDateTime;
					DateTime layerEnd = layer.PeriodEndDateTime;

					DateTime currentStart =
						layerStart.Date.Add(fitToDefaultResolution(layerStart.TimeOfDay, resolution));
					if (currentStart > layerStart)
					{
						currentStart = currentStart.AddMinutes(-resolution);
					}
					// If the layer doesn't fit to the resolution (maybe a short break of 5 minutes in the start)
					if (currentStart < layerStart)
					{
						currentResourceInMinutes = currentResourceInMinutes - (int)layerStart.Subtract(currentStart).TotalMinutes;
					}

					// IF the shift is outside opening hours and Activity needs skill dont't use it (otherwise it could be outside opening hours).
					var calculatableSkillStaffPeriod = staffPeriodData.ForTime(currentStart);
					if (calculatableSkillStaffPeriod == null)
					{
						if (activity.RequiresSkill)
						{
							return double.MinValue;
						}
						// try to find a SkillStaffPeriodDataHolder
						do
						{
							currentStart = currentStart.AddMinutes(resolution);
							calculatableSkillStaffPeriod = staffPeriodData.ForTime(currentStart);
							if (calculatableSkillStaffPeriod != null) break;
						} while (currentStart < layerEnd);
					}
					// make sure we did find any
					if (calculatableSkillStaffPeriod != null)
					{
						// check so we still is inside the layer
						if (calculatableSkillStaffPeriod.PeriodStartDateTime > layerEnd)
							currentResourceInMinutes = 0;
						// only part of the period should count
						if (currentResourceInMinutes > 0 && calculatableSkillStaffPeriod.PeriodEndDateTime > layerEnd)
							currentResourceInMinutes = currentResourceInMinutes - (int)calculatableSkillStaffPeriod.PeriodEndDateTime.Subtract(layerEnd).TotalMinutes;

						while (currentResourceInMinutes > 0)
						{
							periodValue += calculatableSkillStaffPeriod.PeriodValue(currentResourceInMinutes, useMinimumPersons, useMaximumPersons);

							resourceInMinutes += currentResourceInMinutes;

							currentResourceInMinutes = resolution;
							currentStart = currentStart.AddMinutes(resolution);

							if (currentStart >= layer.PeriodEndDateTime)
							{
								break;
							}

							calculatableSkillStaffPeriod = staffPeriodData.ForTime(currentStart);
							if (calculatableSkillStaffPeriod == null)
							{
								if (activity.RequiresSkill)
									return double.MinValue;
								do
								{
									currentStart = currentStart.AddMinutes(resolution);
									calculatableSkillStaffPeriod = staffPeriodData.ForTime(currentStart);
									if (calculatableSkillStaffPeriod != null)
										break;
								} while (currentStart < layer.PeriodEndDateTime);
							}

							if (calculatableSkillStaffPeriod == null)
							{
								if (activity.RequiresSkill)
									return double.MinValue;
								break;
							}

							if (calculatableSkillStaffPeriod.PeriodStartDateTime > layerEnd)
								currentResourceInMinutes = 0;
							// only part of the period should count
							if (currentResourceInMinutes > 0 && calculatableSkillStaffPeriod.PeriodEndDateTime > layerEnd)
								currentResourceInMinutes = currentResourceInMinutes - (int)calculatableSkillStaffPeriod.PeriodEndDateTime.Subtract(layerEnd).TotalMinutes;
						}
					}

				}
				else
				{
					if (activity.RequiresSkill)
						return double.MinValue;
				}

			}

			var lengthCorrectedValue = CalculateShiftValueForPeriod(periodValue, resourceInMinutes, lengthFactor, resolution);

			return Math.Round(lengthCorrectedValue, 5);
		}

		/// <summary>
		/// Calculates the new period value.
		/// </summary>
		/// <param name="oldPeriodValue">The old period value.</param>
		/// <param name="resourceInMinutes">The resource in minutes.</param>
		/// <param name="lengthFactor">The length factor.</param>
		/// <param name="resolution">The resolution.</param>
		/// <returns></returns>
		public double CalculateShiftValueForPeriod(double oldPeriodValue, int resourceInMinutes, WorkShiftLengthHintOption lengthFactor, int resolution)
		{
			if (oldPeriodValue == 0 || resourceInMinutes == 0)
				return 0;

			double factor = (resourceInMinutes - 1d) / 100d;
			if (lengthFactor == WorkShiftLengthHintOption.Long)
			{
				double correctedValue = oldPeriodValue + Math.Abs(oldPeriodValue) * factor;
				return correctedValue;
			}

			if (lengthFactor == WorkShiftLengthHintOption.Short)
			{
				double correctedValue = oldPeriodValue - Math.Abs(oldPeriodValue) * factor;
				return correctedValue;
			}

			return oldPeriodValue;
		}

		public static IEnumerable<IImprovableWorkShiftCalculation> CalculateListForBestImprovementAfterAssignment(
			IEnumerable<IImprovableWorkShiftCalculation> workShiftFinderResultHolders,
			IWorkShiftCalculatorSkillStaffPeriodData skillStaffPeriodData
		  )
		{
			if (!workShiftFinderResultHolders.Skip(1).Any())
				return workShiftFinderResultHolders;

			IList<IImprovableWorkShiftCalculation> returnList = new List<IImprovableWorkShiftCalculation>();
			double bestValue = double.MinValue;
			foreach (var resultHolder in workShiftFinderResultHolders)
			{
				double tmpValue = CalculateDeviationImprovementAfterAssignment(resultHolder.WorkShiftCalculatableProjection.WorkShiftCalculatableLayers, skillStaffPeriodData);
				if (tmpValue > bestValue)
				{
					returnList = new List<IImprovableWorkShiftCalculation> { resultHolder };
					bestValue = tmpValue;
					continue;
				}

				if (Math.Abs(tmpValue - bestValue) < 0.00001)
				{
					returnList.Add(resultHolder);
				}

			}
			return returnList;
		}

		public static double CalculateDeviationImprovementAfterAssignment(
			IEnumerable<IWorkShiftCalculatableLayer> mainShiftLayers,
			IWorkShiftCalculatorSkillStaffPeriodData skillStaffPeriodData
			)
		{
			double sumDeviationBefore = 0;
			double sumDeviationAfter = 0;
			foreach (var staffPeriods in skillStaffPeriodData.All())
			{
				foreach (var periodDataHolder in staffPeriods.All())
				{
					if (periodDataHolder.CanCalculateDeviations())
					{
						double tmpDeviation = periodDataHolder.CalculateStandardDeviation();
						// if Deviation > 0 try to add layers and see if it gets better
						if (tmpDeviation > 0)
						{
							sumDeviationBefore += tmpDeviation;
							sumDeviationAfter += periodDataHolder.DeviationAfterNewLayers(mainShiftLayers);
						}
					}
				}
			}
			return sumDeviationBefore - sumDeviationAfter;
		}

		
	}

	public static class SkillStaffPeriodCalculator
	{
		public const int TheBigNumber = 100000;

		public static double CalculateWorkShiftPeriodValue(double originalDemandInMinutes, double tweakedCurrentDemand, int currentResourceInMinutes)
		{
			if (originalDemandInMinutes == 0) return 0;

			int logicalSign = Math.Sign(tweakedCurrentDemand);
			double weightedCurrentDemand = tweakedCurrentDemand * tweakedCurrentDemand;

			double oldValue = weightedCurrentDemand / originalDemandInMinutes * logicalSign;
			double afterAddingCurrent = tweakedCurrentDemand + currentResourceInMinutes;

			int nextLogicalSign = Math.Sign(afterAddingCurrent);
			weightedCurrentDemand = afterAddingCurrent * afterAddingCurrent;
			double newValue = weightedCurrentDemand / originalDemandInMinutes * nextLogicalSign;

			return (oldValue - newValue) * logicalSign;
		}

		public static double GetTweakedCurrentDemand(double originalDemandInMinutes, double assignedResourceInMinutes, double overstaffingFactor, double priorityValue)
		{
			double currentDemandInMinutes = assignedResourceInMinutes - originalDemandInMinutes;
			if (Math.Abs(overstaffingFactor - 0.5) < 0.05)
				return priorityValue * currentDemandInMinutes;

			double overUnderStaffFactor = 1;
			if (currentDemandInMinutes < 0)
				overUnderStaffFactor = 1d - overstaffingFactor;
			return priorityValue * overUnderStaffFactor * currentDemandInMinutes;
		}

		public static double GetCorrectionFactor(bool useMinimumPersons, bool useMaximumPersons, double absoluteDifferenceScheduledHeadsAndMinMaxHeads, int minimumPersons, double assignedResourceInMinutes)
		{
			double corrFactor = 0;
			double underOverStaffed = absoluteDifferenceScheduledHeadsAndMinMaxHeads;
			if (underOverStaffed == 0)
				return 0;

			if (useMinimumPersons && underOverStaffed < 0 && minimumPersons > 0) // understaffed
			{
				corrFactor = -underOverStaffed * TheBigNumber;
				if (assignedResourceInMinutes > 0)
				{
					corrFactor = corrFactor / minimumPersons;
				}
				return corrFactor;
			}
			if (useMaximumPersons && underOverStaffed > 0)
			{
				corrFactor = -underOverStaffed * TheBigNumber;
			}
			return corrFactor;
		}
	}

	public enum WorkShiftLengthHintOption
	{
		///<summary>
		/// Use average work time
		///</summary>
		AverageWorkTime = 4,
		///<summary>
		/// Raise the workshift
		///</summary>
		/// <remarks>
		/// This option is used for indicating that the workshift should be longer
		/// </remarks>
		Long = 8,
		///<summary>
		/// Raise the workshift
		///</summary>
		/// <remarks>
		/// This option is used for indicating that the workshift should be shorter
		/// </remarks>
		Short = 1,
		/// <summary>
		/// Not calculated
		/// </summary>
		/// <remarks>
		/// This option is used for indicating that the value is not calculated.
		/// </remarks>
		Free = 0
	}

	public interface IImprovableWorkShiftCalculation
	{
		IWorkShiftCalculatableProjection WorkShiftCalculatableProjection { get; }
	}

	public interface IWorkShiftCalculatableProjection
	{
		IEnumerable<IWorkShiftCalculatableLayer> WorkShiftCalculatableLayers { get; }
	}

	public interface IWorkShiftCalculatableLayer
	{
		DateTime PeriodStartDateTime { get; }
		DateTime PeriodEndDateTime { get; }
		IWorkShiftCalculatableActivity Activity { get; }
	}

	public interface IWorkShiftCalculatableActivity
	{
		bool RequiresSkill { get; }
	}

	public interface IWorkShiftCalculatorSkillStaffPeriodData
	{
		bool Empty();
		IWorkShiftCalculatorStaffPeriodData ForActivity(IWorkShiftCalculatableActivity activity);
		IEnumerable<IWorkShiftCalculatorStaffPeriodData> All();
	}

	public interface IWorkShiftCalculatorStaffPeriodData
	{
		IWorkShiftCalculatableSkillStaffPeriod ForTime(DateTime dateTime);
		IEnumerable<IWorkShiftCalculatableSkillStaffPeriod> All();
		int Resolution();
	}

	public interface IWorkShiftCalculatableSkillStaffPeriod : ICalculateDevitions
	{
		DateTime PeriodStartDateTime { get; }
		DateTime PeriodEndDateTime { get; }
		double PeriodValue(int currentResourceInMinutes, bool useMinimumPersons, bool useMaximumPersons);
	}

	public interface ICalculateDevitions
	{
		bool CanCalculateDeviations();//http://martinfowler.com/bliki/TellDontAsk.html
		double CalculateStandardDeviation();
		double DeviationAfterNewLayers(IEnumerable<IWorkShiftCalculatableLayer> mainShiftLayers);
	}
}