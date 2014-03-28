

using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Secret;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class DayOffOptimizationDecisionMakerFactory : IDayOffOptimizationDecisionMakerFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IEnumerable<IDayOffDecisionMaker> CreateDecisionMakers(
			ILockableBitArray scheduleMatrixArray,
			IOptimizationPreferences optimizerPreferences)
		{
			var daysOffPreferences = optimizerPreferences.DaysOff;
			IList<IDayOffLegalStateValidator> legalStateValidators =
				 createLegalStateValidators(scheduleMatrixArray, daysOffPreferences, optimizerPreferences);

			IList<IDayOffLegalStateValidator> legalStateValidatorsToKeepWeekEnds =
				createLegalStateValidatorsToKeepWeekendNumbers(scheduleMatrixArray, optimizerPreferences);

			IOfficialWeekendDays officialWeekendDays = new OfficialWeekendDays();
			ILogWriter logWriter = new LogWriter<DayOffOptimizationService>();

			IDayOffDecisionMaker moveDayOffDecisionMaker = new MoveOneDayOffDecisionMaker(legalStateValidators, logWriter);
			IDayOffDecisionMaker moveWeekEndDecisionMaker = new MoveWeekendDayOffDecisionMaker(legalStateValidatorsToKeepWeekEnds, officialWeekendDays, true, logWriter);
			IDayOffDecisionMaker moveTwoWeekEndDaysDecisionMaker = new MoveWeekendDayOffDecisionMaker(legalStateValidators, officialWeekendDays, false, logWriter);


			bool is2222 = false;
			if (daysOffPreferences.UseDaysOffPerWeek && daysOffPreferences.DaysOffPerWeekValue.Minimum == 2 && daysOffPreferences.DaysOffPerWeekValue.Maximum == 2)
			{
				if (daysOffPreferences.UseConsecutiveDaysOff && daysOffPreferences.ConsecutiveDaysOffValue.Minimum == 2 && daysOffPreferences.ConsecutiveDaysOffValue.Maximum == 2)
				{
					if (daysOffPreferences.UseConsecutiveWorkdays)
						is2222 = true;
				}
			}
			IDayOffDecisionMaker teDataDayOffDecisionMaker = new TeDataDayOffDecisionMaker(legalStateValidators, is2222, logWriter);

			IList<IDayOffDecisionMaker> retList = new List<IDayOffDecisionMaker> { moveDayOffDecisionMaker, moveTwoWeekEndDaysDecisionMaker, moveWeekEndDecisionMaker, teDataDayOffDecisionMaker };

			if (daysOffPreferences.UseConsecutiveWorkdays && daysOffPreferences.ConsecutiveWorkdaysValue.Maximum == 5)
			{
				if (daysOffPreferences.UseFullWeekendsOff && daysOffPreferences.FullWeekendsOffValue.Equals(new MinMax<int>(1, 1)))
				{
					IDayOffDecisionMaker cMSBOneFreeWeekendMax5WorkingdaysDecisionMaker = new CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker(officialWeekendDays, new TrueFalseRandomizer());
					retList.Add(cMSBOneFreeWeekendMax5WorkingdaysDecisionMaker);
				}
			}

			return retList;
		}

		private static IList<IDayOffLegalStateValidator> createLegalStateValidators(
		   ILockableBitArray bitArray,
		   IDaysOffPreferences dayOffPreferences,
		   IOptimizationPreferences optimizerPreferences)
		{
			MinMax<int> periodArea = bitArray.PeriodArea;
			if (!dayOffPreferences.ConsiderWeekBefore)
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

		private static IList<IDayOffLegalStateValidator> createLegalStateValidatorsToKeepWeekendNumbers(
			ILockableBitArray bitArray,
			IOptimizationPreferences optimizerPreferences)
		{
			MinMax<int> periodArea = bitArray.PeriodArea;
			if (!optimizerPreferences.DaysOff.ConsiderWeekBefore)
				periodArea = new MinMax<int>(periodArea.Minimum + 7, periodArea.Maximum + 7);
			IOfficialWeekendDays weekendDays = new OfficialWeekendDays();
			IDayOffLegalStateValidatorListCreator validatorListCreator =
				new DayOffOptimizationWeekendLegalStateValidatorListCreator(optimizerPreferences.DaysOff,
					 weekendDays,
					 periodArea);

			return validatorListCreator.BuildActiveValidatorList();
		} 
	}
}