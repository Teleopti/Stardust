using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class FairnessAndMaxSeatCalculatorsManager28317
	{
		public IList<IWorkShiftCalculationResultHolder> FindBestShiftAccordingToValue(IEnumerable<IWorkShiftCalculationResultHolder> allValues)
		{
			var highestShiftValue = double.MinValue;
			var foundValues = new List<IWorkShiftCalculationResultHolder>();

            foreach (var thisShiftValue in allValues)
            {
                var shiftProjection = thisShiftValue.ShiftProjection;
	            var shiftValue = thisShiftValue.Value;

                if (shiftValue > highestShiftValue)
                {
                    var workShiftFinderResultHolder = new WorkShiftCalculationResult { ShiftProjection = shiftProjection, Value = shiftValue };
                    foundValues = new List<IWorkShiftCalculationResultHolder> { workShiftFinderResultHolder };
                    highestShiftValue = shiftValue;
                    continue;
                }
                if (Math.Abs(shiftValue - highestShiftValue) < 0.000001)
                {
                    var workShiftFinderResultHolder = new WorkShiftCalculationResult { ShiftProjection = shiftProjection, Value = shiftValue };
                    foundValues.Add(workShiftFinderResultHolder);
                }
            }
            return foundValues;
        }

		public IList<IWorkShiftCalculationResultHolder> FindBestLongShiftAccordingToValue(IEnumerable<IWorkShiftCalculationResultHolder> allValues)
		{
			var sortedList = allValues.OrderByDescending(r => r, new WorkShiftCalculationLengthAndValueResultComparer());
			var foundValues = new List<IWorkShiftCalculationResultHolder>();
			var highestValue = double.MinValue;
			foreach (var workShiftCalculationResultHolder in sortedList)
			{
				var shiftValue = workShiftCalculationResultHolder.Value;
				if (shiftValue > highestValue)
				{
					highestValue = shiftValue;
				}

				if (shiftValue < highestValue)
				{
					return foundValues;
				}

				if (Math.Abs(shiftValue - highestValue) < 0.000001 && foundValues.Any())
				{
					foundValues.Add(new WorkShiftCalculationResult { ShiftProjection = workShiftCalculationResultHolder.ShiftProjection, Value = shiftValue });
					continue;
				}

				if (workShiftCalculationResultHolder.Value > 0 && !foundValues.Any())
				{
					foundValues.Add(new WorkShiftCalculationResult { ShiftProjection = workShiftCalculationResultHolder.ShiftProjection, Value = shiftValue });
				}

				
			}
			return foundValues;
		}


	}
}