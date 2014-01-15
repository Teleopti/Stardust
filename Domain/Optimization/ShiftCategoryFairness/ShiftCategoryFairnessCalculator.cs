using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    /// <summary>
    /// Executes the equal shift category fairnessHolder calculations 
    /// </summary>
	public interface IShiftCategoryFairnessCalculator
	{
		IShiftCategoryFairnessFactors ShiftCategoryFairnessFactors(
			IShiftCategoryFairnessHolder groupCategoryFairnessHolder,
			IShiftCategoryFairnessHolder personCategoryFairnessHolder);
	}

	public class ShiftCategoryFairnessCalculator : IShiftCategoryFairnessCalculator
	{


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IShiftCategoryFairnessFactors ShiftCategoryFairnessFactors(
			IShiftCategoryFairnessHolder groupCategoryFairnessHolder,
			IShiftCategoryFairnessHolder personCategoryFairnessHolder)
		{
			var dictionary = executeCalculations(groupCategoryFairnessHolder, personCategoryFairnessHolder);
			return new ShiftCategoryFairnessFactors(dictionary, groupCategoryFairnessHolder.FairnessValueResult.FairnessPoints);
		}

		private static IDictionary<IShiftCategory, double> executeCalculations(
			IShiftCategoryFairnessHolder groupCategoryFairnessHolder,
			IShiftCategoryFairnessHolder personCategoryFairnessHolder)
		{
			IDictionary<IShiftCategory, double> dictionary = new Dictionary<IShiftCategory, double>();

			int sumGroupShifts = 0;
			foreach (int value in groupCategoryFairnessHolder.ShiftCategoryFairnessDictionary.Values)
			{
				sumGroupShifts += value;
			}

			int sumPersonShifts = 0;
			foreach (int value in personCategoryFairnessHolder.ShiftCategoryFairnessDictionary.Values)
			{
				sumPersonShifts += value;
			}

			foreach (var shiftCategory in groupCategoryFairnessHolder.ShiftCategoryFairnessDictionary.Keys)
			{
				int currentGroupCategoryShifts = groupCategoryFairnessHolder.ShiftCategoryFairnessDictionary[shiftCategory];
				int currentPersonCategoryShifts = 0;

				if (personCategoryFairnessHolder.ShiftCategoryFairnessDictionary.ContainsKey(shiftCategory))
					currentPersonCategoryShifts = personCategoryFairnessHolder.ShiftCategoryFairnessDictionary[shiftCategory];

				double currentFairnessValue =
					calculateShiftValue(sumGroupShifts, currentGroupCategoryShifts, sumPersonShifts, currentPersonCategoryShifts);

				double currentShiftEvaluationValue =
					  calculateShiftEvaluationValue(currentFairnessValue);

				dictionary.Add(shiftCategory, currentShiftEvaluationValue);
			}

			return dictionary;

		}

		private static double calculateShiftEvaluationValue(double shiftFairnessValue)
		{
			return Math.Pow(1 - shiftFairnessValue, 2d);
		}

		private static double calculateShiftValue(
			int sumGroupShifts,
			int currentGroupCategoryShifts,
			int sumPersonShifts,
			int currentPersonCategoryShifts)
		{
			if (sumPersonShifts == 0)
				return 0;

			double relativeGroupCategoryShifts = (double)currentGroupCategoryShifts / sumGroupShifts;
			double relativePersonCategoryShifts = (double)currentPersonCategoryShifts / sumPersonShifts;
			return relativePersonCategoryShifts - relativeGroupCategoryShifts;
		}
	}
}
