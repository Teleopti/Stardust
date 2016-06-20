using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class OptimizerHelperHelper
	{
		public void ScheduleBlankSpots(
			IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
			IScheduleService scheduleService,
			IComponentContext container,
			ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{

			var effectiveRestrictionCreator = container.Resolve<IEffectiveRestrictionCreator>();
			var optimizerPreferences = container.Resolve<IOptimizationPreferences>();
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
						var resourceCalculateDelayer = new ResourceCalculateDelayer(container.Resolve<IResourceOptimizationHelper>(), 1, schedulingOptions.ConsiderShortBreaks, schedulingResultStateHolder);

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

		public void SetConsiderShortBreaks(IEnumerable<IPerson> persons, DateOnlyPeriod period, IReschedulingPreferences options, IComponentContext container)
		{
			var ruleSetBagsOfGroupOfPeopleCanHaveShortBreak =
				container.Resolve<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak>();
			options.ConsiderShortBreaks = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(persons, period);
		}

		public void SetConsiderShortBreaks(IEnumerable<IPerson> persons, DateOnlyPeriod period, ISchedulingOptions options, IComponentContext container)
		{
			var ruleSetBagsOfGroupOfPeopleCanHaveShortBreak =
				container.Resolve<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak>();
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

		public IWorkShiftBackToLegalStateServicePro CreateWorkShiftBackToLegalStateServicePro(ILifetimeScope container)
		{
			var workShiftMinMaxCalculator = container.Resolve<IWorkShiftMinMaxCalculator>();
			var bitArrayCreator = new WorkShiftBackToLegalStateBitArrayCreator();

			var dailySkillForecastAndScheduledValueCalculator = container.Resolve<IDailySkillForecastAndScheduledValueCalculator>();

			var allSkillExtractor = container.Resolve<SchedulingStateHolderAllSkillExtractor>();

			// when we move the period to the method we can have all this in autofac
			var dataExtractor = new RelativeDailyDifferencesByAllSkillsExtractor(dailySkillForecastAndScheduledValueCalculator, allSkillExtractor);

			var dayIndexCalculator = container.Resolve<IWorkShiftLegalStateDayIndexCalculator>();
			var decisionMaker = new WorkShiftBackToLegalStateDecisionMaker(dataExtractor, dayIndexCalculator);
			var deleteService = container.Resolve<IDeleteSchedulePartService>();
			var workShiftBackToLegalStateStep = new WorkShiftBackToLegalStateStep(bitArrayCreator, decisionMaker, deleteService);
			return new WorkShiftBackToLegalStateServicePro(workShiftBackToLegalStateStep, workShiftMinMaxCalculator);
		}
	}
}