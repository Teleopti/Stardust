using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Secrets.DayOffPlanning;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public class CMSBCaseSolver : IDayOffBackToLegalStateSolver
	{
		private readonly ILockableBitArray _bitArray;
        private readonly IDaysOffPreferences _daysOffPreferences;
		private readonly IDayOffDecisionMaker _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;
		private readonly IDayOffBackToLegalStateFunctions _functions;

		public CMSBCaseSolver(ILockableBitArray bitArray, IDayOffBackToLegalStateFunctions functions, IDaysOffPreferences daysOffPreferences, IDayOffDecisionMaker cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker)
        {
            _functions = functions;
            _bitArray = bitArray;
            _daysOffPreferences = daysOffPreferences;
			_cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker = cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;
        }

		public MinMaxNumberOfResult ResolvableState()
		{
			Point block = _functions.FindLongestConsecutiveWorkdayBlockWithAtLeastOneUnlockedBit();
			if (block.Y - block.X + 1 > _daysOffPreferences.ConsecutiveWorkdaysValue.Maximum)
				return MinMaxNumberOfResult.ToMany;

			return MinMaxNumberOfResult.Ok;
		}

		public bool SetToManyBackToLegalState()
		{
			if (ResolvableState() == MinMaxNumberOfResult.Ok)
				return false;

			IList<double?> values = Enumerable.Repeat((double?)0d, _bitArray.Count).ToArray();
			return _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker.Execute(_bitArray, values);
		}

		public bool SetToFewBackToLegalState()
		{
			return ResolvableState() != MinMaxNumberOfResult.Ok;
		}
	}
}