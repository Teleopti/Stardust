using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizerHelperHelper
	{
		public void ScheduleBlankSpots(
			IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
			IScheduleService scheduleService,
			ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IOptimizationPreferences optimizerPreferences,
			IResourceOptimizationHelper resourceOptimizationHelper)
		{
			var schedulingOptionsSynchronizer = new SchedulingOptionsCreator();
			var schedulingOptions = schedulingOptionsSynchronizer.CreateSchedulingOptions(optimizerPreferences);

			foreach (IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer in matrixOriginalStateContainers)
			{
				if (!matrixOriginalStateContainer.StillAlive)
					continue;

				foreach (IScheduleDayPro scheduleDayPro in matrixOriginalStateContainer.ScheduleMatrix.UnlockedDays)
				{
					bool result = true;
					if (!scheduleDayPro.DaySchedulePart().IsScheduled())
					{
						var effectiveRestriction =
							effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDayPro.DaySchedulePart(), schedulingOptions);
						var resourceCalculateDelayer = new ResourceCalculateDelayer(resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, schedulingResultStateHolder);

						result = scheduleService.SchedulePersonOnDay(scheduleDayPro.DaySchedulePart(), schedulingOptions, effectiveRestriction, resourceCalculateDelayer, rollbackService);
					}
					if (!result)
					{
						matrixOriginalStateContainer.StillAlive = false;
						break;
					}
				}
			}
		}

		public IScheduleResultDataExtractor CreatePersonalSkillsDataExtractor(
			IAdvancedPreferences advancedPreferences,
			IScheduleMatrixPro scheduleMatrix)
		{
			IScheduleResultDataExtractorProvider dataExtractorProvider = new ScheduleResultDataExtractorProvider();
			return dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrix, advancedPreferences);
		}

		public void SetConsiderShortBreaks(IEnumerable<IPerson> persons, DateOnlyPeriod period, IReschedulingPreferences options, IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak)
		{
			options.ConsiderShortBreaks = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(persons, period);
		}

		public void SetConsiderShortBreaks(IEnumerable<IPerson> persons, DateOnlyPeriod period, ISchedulingOptions options, IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak)
		{
			options.ConsiderShortBreaks = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(persons, period);
		}

		public IList<ISmartDayOffBackToLegalStateSolverContainer> CreateSmartDayOffSolverContainers(
			IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
			IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			IList<ISmartDayOffBackToLegalStateSolverContainer> solverContainers = new List<ISmartDayOffBackToLegalStateSolverContainer>();
			foreach (var matrixOriginalStateContainer in matrixOriginalStateContainers)
			{
				var matrix = matrixOriginalStateContainer.ScheduleMatrix;
				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person,matrix.EffectivePeriodDays.First().Day);

				ILockableBitArray bitArray = scheduleMatrixLockableBitArrayConverterEx.Convert(matrixOriginalStateContainer.ScheduleMatrix, dayOffOptimizationPreference.ConsiderWeekBefore, dayOffOptimizationPreference.ConsiderWeekAfter);
				IDayOffDecisionMaker cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker = new CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker(new OfficialWeekendDays(), new TrueFalseRandomizer());
				ISmartDayOffBackToLegalStateService solverService = new SmartDayOffBackToLegalStateService(20, cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker);
				ISmartDayOffBackToLegalStateSolverContainer solverContainer = new SmartDayOffBackToLegalStateSolverContainer(matrixOriginalStateContainer, bitArray, solverService);
				solverContainers.Add(solverContainer);
			}

			return solverContainers;
		}

		public void SyncSmartDayOffContainerWithMatrix(
			ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer,
			IDayOffTemplate dayOffTemplate,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IScheduleTagSetter scheduleTagSetter,
			IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			if (backToLegalStateSolverContainer.Result)
			{
				IScheduleMatrixPro matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;

				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person, matrix.EffectivePeriodDays.First().Day);

				ILockableBitArray doArray = scheduleMatrixLockableBitArrayConverterEx.Convert(matrix, dayOffOptimizationPreference.ConsiderWeekBefore, dayOffOptimizationPreference.ConsiderWeekAfter);

				int bitArrayToMatrixOffset = 0;
				if (!dayOffOptimizationPreference.ConsiderWeekBefore)
					bitArrayToMatrixOffset = 7;

				for (int i = 0; i < doArray.Count; i++)
				{
					if (doArray[i] && !backToLegalStateSolverContainer.BitArray[i])
					{
						IScheduleDay part =
							matrix.OuterWeeksPeriodDays[i + bitArrayToMatrixOffset].DaySchedulePart();
						part.DeleteDayOff();
						new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, scheduleDayChangeCallback, scheduleTagSetter).Modify(part);
					}
					else
					{
						if (!doArray[i] && backToLegalStateSolverContainer.BitArray[i])
						{
							IScheduleDay part =
								matrix.OuterWeeksPeriodDays[i + bitArrayToMatrixOffset].DaySchedulePart();
							part.DeleteMainShift(part);
							part.CreateAndAddDayOff(dayOffTemplate);
							new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, scheduleDayChangeCallback, scheduleTagSetter).Modify(part);
						}
					}

				}
			}
		}

		//TODO: move to ctor dep
		public IWorkShiftBackToLegalStateServicePro CreateWorkShiftBackToLegalStateServicePro(IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
			IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator,
			SchedulingStateHolderAllSkillExtractor allSkillExtractor,
			IWorkShiftLegalStateDayIndexCalculator dayIndexCalculator,
			IDeleteSchedulePartService deleteService)
		{
			var bitArrayCreator = new WorkShiftBackToLegalStateBitArrayCreator();
			// when we move the period to the method we can have all this in autofac
			var dataExtractor = new RelativeDailyDifferencesByAllSkillsExtractor(dailySkillForecastAndScheduledValueCalculator, allSkillExtractor);
			var decisionMaker = new WorkShiftBackToLegalStateDecisionMaker(dataExtractor, dayIndexCalculator);
			var workShiftBackToLegalStateStep = new WorkShiftBackToLegalStateStep(bitArrayCreator, decisionMaker, deleteService);
			return new WorkShiftBackToLegalStateServicePro(workShiftBackToLegalStateStep, workShiftMinMaxCalculator);
		}
	}
}