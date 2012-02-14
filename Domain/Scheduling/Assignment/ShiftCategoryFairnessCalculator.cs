﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// Executes the equal shift category fairness calculations 
    /// </summary>
    public interface IShiftCategoryFairnessCalculator
    {
        IShiftCategoryFairnessFactors ShiftCategoryFairnessFactors(
            IShiftCategoryFairness groupCategoryFairness,
            IShiftCategoryFairness personCategoryFairness);
    }

    public class ShiftCategoryFairnessCalculator : IShiftCategoryFairnessCalculator
    {


        public IShiftCategoryFairnessFactors ShiftCategoryFairnessFactors(
            IShiftCategoryFairness groupCategoryFairness,
            IShiftCategoryFairness personCategoryFairness)
        {
            IDictionary<IShiftCategory, double> dictionary = ExecuteCalculations(groupCategoryFairness, personCategoryFairness);
            return new ShiftCategoryFairnessFactors(dictionary, groupCategoryFairness.FairnessValueResult.FairnessPoints);
        }

        private static IDictionary<IShiftCategory, double> ExecuteCalculations(
            IShiftCategoryFairness groupCategoryFairness,
            IShiftCategoryFairness personCategoryFairness)
        {
            IDictionary<IShiftCategory, double> dictionary = new Dictionary<IShiftCategory, double>();

            int sumGroupShifts = 0;
            foreach (int value in groupCategoryFairness.ShiftCategoryFairnessDictionary.Values)
            {
                sumGroupShifts += value;
            }

            int sumPersonShifts = 0;
            foreach (int value in personCategoryFairness.ShiftCategoryFairnessDictionary.Values)
            {
                sumPersonShifts += value;
            }

            foreach (IShiftCategory shiftCategory in groupCategoryFairness.ShiftCategoryFairnessDictionary.Keys)
            {
                int currentGroupCategoryShifts = groupCategoryFairness.ShiftCategoryFairnessDictionary[shiftCategory];
                int currentPersonCategoryShifts = 0;

                if (personCategoryFairness.ShiftCategoryFairnessDictionary.ContainsKey(shiftCategory))
                    currentPersonCategoryShifts = personCategoryFairness.ShiftCategoryFairnessDictionary[shiftCategory];

                double currentFairnessValue =
                    CalculateShiftValue(sumGroupShifts, currentGroupCategoryShifts, sumPersonShifts, currentPersonCategoryShifts);

                double currentShiftEvaluationValue =
                      CalculateShiftEvaluationValue(currentFairnessValue);

                dictionary.Add(shiftCategory, currentShiftEvaluationValue);
            }

            return dictionary;

        }

        private static double CalculateShiftEvaluationValue(double shiftFairnessValue)
        {
            return Math.Pow(1 - shiftFairnessValue, 2d);
        }

        private static double CalculateShiftValue(
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
