

using System.Collections;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
	public interface IDayOffRulesValidator
	{
	}

	public class DayOffRulesValidator : IDayOffRulesValidator
	{
		private readonly IDayOffLegalStateValidatorListCreator _dayOffLegalStateValidatorListCreator;
		private readonly IScheduleMatrixLockableBitArrayConverter _matrixConverter;

		public DayOffRulesValidator(IDayOffLegalStateValidatorListCreator dayOffLegalStateValidatorListCreator,
			IScheduleMatrixLockableBitArrayConverter matrixConverter)
		{
			_dayOffLegalStateValidatorListCreator = dayOffLegalStateValidatorListCreator;
			_matrixConverter = matrixConverter;
		}

		public bool Validate(ILockableBitArray array)
		{
			//var array = _matrixConverter.Convert()
			var validatorList = _dayOffLegalStateValidatorListCreator.BuildActiveValidatorList();
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