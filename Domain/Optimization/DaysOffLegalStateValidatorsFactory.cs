using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDaysOffLegalStateValidatorsFactory
	{
		IList<IDayOffLegalStateValidator> CreateLegalStateValidators(
			ILockableBitArray bitArray,
			IOptimizationPreferences optimizerPreferences);
	}

	public class DaysOffLegalStateValidatorsFactory : IDaysOffLegalStateValidatorsFactory
	{
		public IList<IDayOffLegalStateValidator> CreateLegalStateValidators(
		   ILockableBitArray bitArray,
		   IOptimizationPreferences optimizerPreferences)
		{
			MinMax<int> periodArea = bitArray.PeriodArea;
			if (!optimizerPreferences.DaysOff.ConsiderWeekBefore)
				periodArea = new MinMax<int>(periodArea.Minimum + 7, periodArea.Maximum + 7);
			IOfficialWeekendDays weekendDays = new OfficialWeekendDays();
			IDayOffLegalStateValidatorListCreator validatorListCreator =
				new DayOffOptimizationLegalStateValidatorListCreator
					(optimizerPreferences.DaysOff,
					 weekendDays,
					 bitArray.ToLongBitArray(),
					 periodArea);

			return validatorListCreator.BuildActiveValidatorList();
		} 
	}
}