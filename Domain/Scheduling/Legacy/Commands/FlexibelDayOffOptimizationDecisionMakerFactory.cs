using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class FlexibelDayOffOptimizationDecisionMakerFactory : IDayOffOptimizationDecisionMakerFactory
	{
		public IEnumerable<IDayOffDecisionMaker> CreateDecisionMakers(
			ILockableBitArray scheduleMatrixArray,
			IOptimizationPreferences optimizerPreferences,
			IDaysOffPreferences daysOffPreferences)
		{
			IList<IDayOffLegalStateValidator> legalStateValidators =
				 createLegalStateValidators(scheduleMatrixArray, daysOffPreferences);

			IList<IDayOffLegalStateValidator> legalStateValidatorsToKeepWeekEnds =
				createLegalStateValidatorsToKeepWeekendNumbers(scheduleMatrixArray, daysOffPreferences);

			IOfficialWeekendDays officialWeekendDays = new OfficialWeekendDays();
			ILogWriter logWriter = new LogWriter<FlexibelDayOffOptimizationDecisionMakerFactory>();

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

			if (optimizerPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime && !optimizerPreferences.General.OptimizationStepDaysOff)
			{
				var flexibleDayOffDecisionMaker = new FlexibelDayOffDecisionMaker(legalStateValidators);
				retList = new List<IDayOffDecisionMaker> { flexibleDayOffDecisionMaker, moveTwoWeekEndDaysDecisionMaker, moveWeekEndDecisionMaker, teDataDayOffDecisionMaker };
			}

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
		   IDaysOffPreferences dayOffPreferences)
		{
			MinMax<int> periodArea = bitArray.PeriodArea;
			if (!dayOffPreferences.ConsiderWeekBefore)
				periodArea = new MinMax<int>(periodArea.Minimum + 7, periodArea.Maximum + 7);
			IOfficialWeekendDays weekendDays = new OfficialWeekendDays();
			IDayOffLegalStateValidatorListCreator validatorListCreator =
				new DayOffOptimizationLegalStateValidatorListCreator
					(dayOffPreferences,
					 weekendDays,
					 bitArray.ToLongBitArray(),
					 periodArea);

			return validatorListCreator.BuildActiveValidatorList();
		}

		private static IList<IDayOffLegalStateValidator> createLegalStateValidatorsToKeepWeekendNumbers(
			ILockableBitArray bitArray,
			IDaysOffPreferences daysOffPreferences)
		{
			MinMax<int> periodArea = bitArray.PeriodArea;
			if (!daysOffPreferences.ConsiderWeekBefore)
				periodArea = new MinMax<int>(periodArea.Minimum + 7, periodArea.Maximum + 7);
			IOfficialWeekendDays weekendDays = new OfficialWeekendDays();
			IDayOffLegalStateValidatorListCreator validatorListCreator =
				new DayOffOptimizationWeekendLegalStateValidatorListCreator(daysOffPreferences,
					 weekendDays,
					 periodArea);

			return validatorListCreator.BuildActiveValidatorList();
		} 
	}
}
