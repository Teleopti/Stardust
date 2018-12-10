using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDaysOffLegalStateValidatorsFactory
	{
		IList<IDayOffLegalStateValidator> CreateLegalStateValidators(
			ILockableBitArray bitArray,
			IOptimizationPreferences optimizerPreferences,
			IDaysOffPreferences daysOffPreferences);
	}

	public class DaysOffLegalStateValidatorsFactory : IDaysOffLegalStateValidatorsFactory
	{
		public IList<IDayOffLegalStateValidator> CreateLegalStateValidators(
		   ILockableBitArray bitArray,
		   IOptimizationPreferences optimizerPreferences,
			IDaysOffPreferences daysOffPreferences)
		{
			MinMax<int> periodArea = bitArray.PeriodArea;
			if (!daysOffPreferences.ConsiderWeekBefore)
				periodArea = new MinMax<int>(periodArea.Minimum + 7, periodArea.Maximum + 7);
			IOfficialWeekendDays weekendDays = new OfficialWeekendDays();
			IDayOffLegalStateValidatorListCreator validatorListCreator =
				new DayOffOptimizationLegalStateValidatorListCreator
					(daysOffPreferences,
					 weekendDays,
					 bitArray.ToLongBitArray(),
					 periodArea);

			return validatorListCreator.BuildActiveValidatorList();
		} 
	}
}