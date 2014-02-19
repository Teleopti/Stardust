using System.Collections;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffRulesValidator
	{
		bool Validate(ILockableBitArray bitArray, IOptimizationPreferences optimizationPreferences);
	}

	public class DayOffRulesValidator : IDayOffRulesValidator
	{
		private readonly IDaysOffLegalStateValidatorsFactory _daysOffLegalStateValidatorsFactory;
		
		public DayOffRulesValidator(IDaysOffLegalStateValidatorsFactory daysOffLegalStateValidatorsFactory)
		{
			_daysOffLegalStateValidatorsFactory = daysOffLegalStateValidatorsFactory;
		}

		public bool Validate(ILockableBitArray array, IOptimizationPreferences optimizationPreferences)
		{
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