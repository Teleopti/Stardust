using System.Collections;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffRulesValidator
	{
		bool Validate(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences);
	}

	public class DayOffRulesValidator : IDayOffRulesValidator
	{
		private readonly IDaysOffLegalStateValidatorsFactory _daysOffLegalStateValidatorsFactory;
		private readonly IScheduleMatrixLockableBitArrayConverterEx _matrixConverter;

		public DayOffRulesValidator(IDaysOffLegalStateValidatorsFactory daysOffLegalStateValidatorsFactory,
			IScheduleMatrixLockableBitArrayConverterEx matrixConverter)
		{
			_daysOffLegalStateValidatorsFactory = daysOffLegalStateValidatorsFactory;
			_matrixConverter = matrixConverter;
		}

		public bool Validate(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			var array = _matrixConverter.Convert(matrix, optimizationPreferences.DaysOff.ConsiderWeekBefore, optimizationPreferences.DaysOff.ConsiderWeekBefore);
			var validatorList = _daysOffLegalStateValidatorsFactory.CreateLegalStateValidators(array, optimizationPreferences);
			BitArray longBitArray = array.ToLongBitArray();
			int offset = 0;
			if (array.PeriodArea.Minimum < 7)
				offset = 7;
			for (int i = array.PeriodArea.Minimum; i <= array.PeriodArea.Maximum; i++)
			{
				if (longBitArray[i + offset])
				{
					if (!validateIndex(longBitArray, i + offset, validatorList))
					{
						return false;
					}
				}
			}
			return true;
		}

		private static bool validateIndex(BitArray daysOffArray, int index, IEnumerable<IDayOffLegalStateValidator> validatorList)
		{
			foreach (IDayOffLegalStateValidator validator in validatorList)
			{
				if (!validator.IsValid(daysOffArray, index))
					return false;
			}
			return true;
		}
	}
}