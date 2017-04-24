using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class FairnessAndMaxSeatCalculatorsManager28317
	{
		public IList<IWorkShiftCalculationResultHolder> RecalculateFoundValues(IEnumerable<IWorkShiftCalculationResultHolder> allValues)
		{
			var highestShiftValue = double.MinValue;
			var foundValues = new List<IWorkShiftCalculationResultHolder>();

            foreach (var thisShiftValue in allValues)
            {
                var shiftProjection = thisShiftValue.ShiftProjection;
	            var shiftValue = thisShiftValue.Value;

				//REMOVE THIS BLOCK WITH MAXSEAT TOGGLE
				if (shiftValue > highestShiftValue && schedulingOptions.UserOptionMaxSeatsFeature != MaxSeatsFeatureOptions.DoNotConsiderMaxSeats)
                {
                    var seatVal =
                        _seatLimitationWorkShiftCalculator.CalculateShiftValue(person,
                                                                               shiftProjection.MainShiftProjection,
                                                                               maxSeatSkillPeriods,
																			   schedulingOptions.UserOptionMaxSeatsFeature);
                    if (!seatVal.HasValue)
                        continue;

                    shiftValue += seatVal.Value;
                }
				////////////////////////////////////////
>>>>>>> other

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
		
	}
}