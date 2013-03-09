

using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ITeamBlockDayOffOptimizerService
	{
		void OptimizeDaysOff(IList<IScheduleMatrixPro> allPersonMatrixList, 
								DateOnlyPeriod selectedPeriod,
								IList<IPerson> selectedPersons, 
								ISchedulingOptions schedulingOptions,
								IOptimizationPreferences optimizationPreferences);
	}

	public class TeamBlockDayOffOptimizerService : ITeamBlockDayOffOptimizerService
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly IScheduleMatrixLockableBitArrayConverter _converter;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly ISmartDayOffBackToLegalStateService _smartDayOffBackToLegalStateService;

		public TeamBlockDayOffOptimizerService(ITeamInfoFactory teamInfoFactory, 
												IScheduleMatrixLockableBitArrayConverter converter,
												IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
												ISmartDayOffBackToLegalStateService smartDayOffBackToLegalStateService)
		{
			_teamInfoFactory = teamInfoFactory;
			_converter = converter;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_smartDayOffBackToLegalStateService = smartDayOffBackToLegalStateService;
		}

		public void OptimizeDaysOff(IList<IScheduleMatrixPro> allPersonMatrixList, 
									DateOnlyPeriod selectedPeriod,
		                            IList<IPerson> selectedPersons, 
									ISchedulingOptions schedulingOptions, 
									IOptimizationPreferences optimizationPreferences)
		{
			// create a list of all teamInfos
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod, allPersonMatrixList));
			}

			// find a random selected TeamInfo/matrix
			var remainingInfoList = new List<ITeamInfo>(allTeamInfoListOnStartDate);
			foreach (var teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count, true))
			{
				foreach (var matrix in teamInfo.MatrixesForGroupMember(0))
				{
					ILockableBitArray originalArray = _converter.Convert(optimizationPreferences.DaysOff.ConsiderWeekBefore,
																   optimizationPreferences.DaysOff.ConsiderWeekAfter);

					IScheduleResultDataExtractor scheduleResultDataExtractor =
							_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix);

					ILockableBitArray resultingArray = tryFindMoves(originalArray, scheduleResultDataExtractor, optimizationPreferences);

					if (resultingArray.Equals(originalArray))
						continue;


					// execute do moves
					// ev back to legal state?
					// if possible reschedule block without clearing
					// else
					//	clear involved teamblocks
					//	reschedule involved teamblocks
					// rollback id failed or not good
					// remember not to break anything in shifts or restrictions
				}
			}
		}

		private ILockableBitArray tryFindMoves(ILockableBitArray originalArray, IScheduleResultDataExtractor scheduleResultDataExtractor, IOptimizationPreferences optimizationPreferences)
		{
			// find days off to move within the common matrix period
			IEnumerable<IDayOffDecisionMaker> decisionMakers = createDecisionMakers(originalArray, optimizationPreferences);
			foreach (var dayOffDecisionMaker in decisionMakers)
			{
				ILockableBitArray workingBitArray = (ILockableBitArray)originalArray.Clone();

				if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
				{
					if (!_smartDayOffBackToLegalStateService.Execute(_smartDayOffBackToLegalStateService.BuildSolverList(workingBitArray), 100))
						continue;

					if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
						continue;
				}

				// DayOffBackToLegal if decisionMaker did something wrong
				if (!_smartDayOffBackToLegalStateService.Execute(_smartDayOffBackToLegalStateService.BuildSolverList(workingBitArray), 100))
					continue;

				return workingBitArray;
			}

			return originalArray;
		}

		private static IEnumerable<IDayOffDecisionMaker> createDecisionMakers(
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