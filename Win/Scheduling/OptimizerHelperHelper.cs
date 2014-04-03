using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Scheduling
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static class OptimizerHelperHelper
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "schedulingOptions"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public static void ScheduleBlankSpots(
            IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
            IScheduleService scheduleService, 
            IComponentContext container,
			ISchedulePartModifyAndRollbackService rollbackService)
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
						var resourceCalculateDelayer = new ResourceCalculateDelayer(container.Resolve<IResourceOptimizationHelper>(), 1, true,
																		schedulingOptions.ConsiderShortBreaks);

						result = scheduleService.SchedulePersonOnDay(scheduleDayPro.DaySchedulePart(), schedulingOptions, effectiveRestriction, resourceCalculateDelayer, null, rollbackService);
                    }
                    if (!result)
                    {
                        matrixOriginalStateContainer.StillAlive = false;
                        break;
                    }
                }
            }
        }

		public static void LockDaysForIntradayOptimization(IList<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod)
		{
			IMatrixOvertimeLocker matrixOvertimeLocker = new MatrixOvertimeLocker(matrixList);
			matrixOvertimeLocker.Execute();
			IMatrixNoMainShiftLocker noMainShiftLocker = new MatrixNoMainShiftLocker(matrixList);
			noMainShiftLocker.Execute();
			var matrixUnselectedDaysLocker = new MatrixUnselectedDaysLocker(matrixList, selectedPeriod);
			matrixUnselectedDaysLocker.Execute();

		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "optimizerPreferences"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void LockDaysForDayOffOptimization(IList<IScheduleMatrixPro> matrixList,IRestrictionExtractor restrictionExtractor,IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod)
        {
            var schedulingOptionsCreator = new SchedulingOptionsCreator();
            var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);

			//Not needed anymore i think, 
			IMatrixRestrictionLocker restrictionLocker = new MatrixRestrictionLocker(schedulingOptions, restrictionExtractor);
			foreach (IScheduleMatrixPro scheduleMatrixPro in matrixList)
				lockRestrictionDaysInMatrix(scheduleMatrixPro, restrictionLocker);
            IMatrixMeetingDayLocker meetingDayLocker = new MatrixMeetingDayLocker(matrixList);
            meetingDayLocker.Execute();
            IMatrixPersonalShiftLocker personalShiftLocker = new MatrixPersonalShiftLocker(matrixList);
            personalShiftLocker.Execute();
            IMatrixOvertimeLocker matrixOvertimeLocker = new MatrixOvertimeLocker(matrixList);
            matrixOvertimeLocker.Execute();
            IMatrixNoMainShiftLocker noMainShiftLocker = new MatrixNoMainShiftLocker(matrixList);
            noMainShiftLocker.Execute();
			IMatrixShiftsNotAvailibleLocker matrixShiftsNotAvailibleLocker = new MatrixShiftsNotAvailibleLocker();
			matrixShiftsNotAvailibleLocker.Execute(matrixList);
	        var matrixUnselectedDaysLocker = new MatrixUnselectedDaysLocker(matrixList, selectedPeriod);
			matrixUnselectedDaysLocker.Execute();
        }

		private static void lockRestrictionDaysInMatrix(IScheduleMatrixPro matrix, IMatrixRestrictionLocker locker)
		{

			IList<DateOnly> daysToLock = locker.Execute(matrix);
			foreach (var dateOnly in daysToLock)
			{
				matrix.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
			}
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IPeriodValueCalculator CreatePeriodValueCalculator(IAdvancedPreferences advancedPreferences,
            IScheduleResultDataExtractor dataExtractor)
        {
            IPeriodValueCalculatorProvider calculatorProvider = new PeriodValueCalculatorProvider();
            return calculatorProvider.CreatePeriodValueCalculator(advancedPreferences, dataExtractor);
        }

        public static IScheduleResultDataExtractor CreateAllSkillsDataExtractor(
            IAdvancedPreferences advancedPreferences,
            DateOnlyPeriod selectedPeriod,
            ISchedulingResultStateHolder stateHolder)
        {
            IScheduleResultDataExtractorProvider dataExtractorProvider = new ScheduleResultDataExtractorProvider();
            IScheduleResultDataExtractor allSkillsDataExtractor = dataExtractorProvider.CreateAllSkillsDataExtractor(selectedPeriod, stateHolder, advancedPreferences);
            return allSkillsDataExtractor;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IScheduleResultDataExtractor CreatePersonalSkillsDataExtractor(
            IAdvancedPreferences advancedPreferences,
            IScheduleMatrixPro scheduleMatrix)
        {
            IScheduleResultDataExtractorProvider dataExtractorProvider = new ScheduleResultDataExtractorProvider();
            return dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrix, advancedPreferences);
        }

        public static DateOnlyPeriod GetSelectedPeriod(IEnumerable<IScheduleDay> scheduleDays)
        {
            if (scheduleDays == null) throw new ArgumentNullException("scheduleDays");
            DateOnly minDate = DateOnly.MaxValue;
            DateOnly maxDate = DateOnly.MinValue;
            foreach (var scheduleDay in scheduleDays)
            {
                if (scheduleDay.DateOnlyAsPeriod.DateOnly < minDate)
                    minDate = scheduleDay.DateOnlyAsPeriod.DateOnly;

                if (scheduleDay.DateOnlyAsPeriod.DateOnly > maxDate)
                    maxDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
            }

            return new DateOnlyPeriod(minDate, maxDate);
        }

        /// <summary>
        /// Get the ISchedulePart list contained in the clips. This method creates a list of the ISchedulePart objects
        /// that are contained in the grid. Filters out those that are null, or if does not have the ISchedulePart type.
        /// </summary>
        /// <remarks>
        /// This method is used to get the selected objects in a grid.
        /// </remarks>
        public static ReadOnlyCollection<IScheduleDay> ContainedSchedulePartList(IEnumerable<Clip> clipList)
        {
            Func<IList<IScheduleDay>> clipObjectListFilter = delegate
            {
                IList<IScheduleDay> result = new List<IScheduleDay>();
                foreach (Clip clip in clipList)
                {
                    if (clip != null && clip.ClipObject != null)
                    {
                        var clipObject = clip.ClipObject as IScheduleDay;
                        if (clipObject != null)
                            result.Add(clipObject);
                    }
                }
                return result;
            };
            return new ReadOnlyCollection<IScheduleDay>(clipObjectListFilter());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Validators")]
        public static IList<IDayOffLegalStateValidator> CreateLegalStateValidators(
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public static IEnumerable<IDayOffDecisionMaker> CreateDecisionMakers(
            ILockableBitArray scheduleMatrixArray,
            IOptimizationPreferences optimizerPreferences, IComponentContext container)
		{
			IDayOffOptimizationDecisionMakerFactory dayOffOptimizationDecisionMakerFactory =
				container.Resolve<IDayOffOptimizationDecisionMakerFactory>();

			return dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(scheduleMatrixArray, optimizerPreferences);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public static void SetConsiderShortBreaks(IEnumerable<IPerson> persons, DateOnlyPeriod period, IReschedulingPreferences options, IComponentContext container)
        {
            var ruleSetBagsOfGroupOfPeopleCanHaveShortBreak =
                container.Resolve<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak>();
            options.ConsiderShortBreaks = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(persons, period);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public static void SetConsiderShortBreaks(IEnumerable<IPerson> persons, DateOnlyPeriod period, ISchedulingOptions options, IComponentContext container)
        {
            var ruleSetBagsOfGroupOfPeopleCanHaveShortBreak =
                container.Resolve<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak>();
            options.ConsiderShortBreaks = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(persons, period);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IList<ISmartDayOffBackToLegalStateSolverContainer> CreateSmartDayOffSolverContainers(
            IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
            IDaysOffPreferences daysOffPreferences)
        {
            IList<ISmartDayOffBackToLegalStateSolverContainer> solverContainers = new List<ISmartDayOffBackToLegalStateSolverContainer>();
            foreach (var matrixOriginalStateContainer in matrixOriginalStateContainers)
            {
                IScheduleMatrixLockableBitArrayConverter bitArrayConverter =
                    new ScheduleMatrixLockableBitArrayConverter(matrixOriginalStateContainer.ScheduleMatrix);

                ILockableBitArray bitArray = bitArrayConverter.Convert(daysOffPreferences.ConsiderWeekBefore,
                                                                      daysOffPreferences.ConsiderWeekAfter);

                IDayOffBackToLegalStateFunctions functions = new DayOffBackToLegalStateFunctions(bitArray);
				IDayOffDecisionMaker cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker = new CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker(new OfficialWeekendDays(), new TrueFalseRandomizer());
                ISmartDayOffBackToLegalStateService solverService = new SmartDayOffBackToLegalStateService(functions, daysOffPreferences, 20, cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker);
                ISmartDayOffBackToLegalStateSolverContainer solverContainer = new SmartDayOffBackToLegalStateSolverContainer(matrixOriginalStateContainer, bitArray, solverService);
                solverContainers.Add(solverContainer);
            }

            return solverContainers;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public static void SyncSmartDayOffContainerWithMatrix(
            ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer,
            IDayOffTemplate dayOffTemplate, 
            IDaysOffPreferences daysOffPreferences, 
            IScheduleDayChangeCallback scheduleDayChangeCallback,
            IScheduleTagSetter scheduleTagSetter)
        {
            if (backToLegalStateSolverContainer.Result)
            {
                IScheduleMatrixPro matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;
                IScheduleMatrixLockableBitArrayConverter bitArrayConverter =
                    new ScheduleMatrixLockableBitArrayConverter(matrix);

                ILockableBitArray doArray = bitArrayConverter.Convert(daysOffPreferences.ConsiderWeekBefore,
                                                                      daysOffPreferences.ConsiderWeekAfter);

                int bitArrayToMatrixOffset = 0;
                if (!daysOffPreferences.ConsiderWeekBefore)
                    bitArrayToMatrixOffset = 7;

                for (int i = 0; i < doArray.Count; i++)
                {
                    if (doArray[i] && !backToLegalStateSolverContainer.BitArray[i])
                    {
                        IScheduleDay part =
                            matrix.OuterWeeksPeriodDays[i + bitArrayToMatrixOffset].DaySchedulePart();
                        part.DeleteDayOff();
                        new SchedulePartModifyAndRollbackService(matrix.SchedulingStateHolder, scheduleDayChangeCallback, scheduleTagSetter).Modify(part);
                    }
                    else
                    {
                        if (!doArray[i] && backToLegalStateSolverContainer.BitArray[i])
                        {
                            IScheduleDay part =
                                matrix.OuterWeeksPeriodDays[i + bitArrayToMatrixOffset].DaySchedulePart();
                            part.DeleteMainShift(part);
                            part.CreateAndAddDayOff(dayOffTemplate);
                            new SchedulePartModifyAndRollbackService(matrix.SchedulingStateHolder, scheduleDayChangeCallback, scheduleTagSetter).Modify(part);
                        }
                    }

                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static IWorkShiftBackToLegalStateServicePro CreateWorkShiftBackToLegalStateServicePro(ILifetimeScope container)
        {
            var workShiftMinMaxCalculator = container.Resolve<IWorkShiftMinMaxCalculator>();
            //var bitArrayCreator = container.Resolve<IWorkShiftBackToLegalStateBitArrayCreator>();
			var bitArrayCreator = new WorkShiftBackToLegalStateBitArrayCreator();

            var dailySkillForecastAndScheduledValueCalculator =
                container.Resolve<IDailySkillForecastAndScheduledValueCalculator>();

            ISkillExtractor allSkillExtractor = container.Resolve<SchedulingStateHolderAllSkillExtractor>();

            // when we move the period to the method we can have all this in autofac
			IRelativeDailyDifferencesByAllSkillsExtractor dataExtractor = new RelativeDailyDifferencesByAllSkillsExtractor(dailySkillForecastAndScheduledValueCalculator, allSkillExtractor);

            var dayIndexCalculator = container.Resolve<IWorkShiftLegalStateDayIndexCalculator>();
            IWorkShiftBackToLegalStateDecisionMaker decisionMaker = new WorkShiftBackToLegalStateDecisionMaker(dataExtractor, dayIndexCalculator);
            var deleteService = container.Resolve<IDeleteSchedulePartService>();
            IWorkShiftBackToLegalStateStep workShiftBackToLegalStateStep =
                new WorkShiftBackToLegalStateStep(bitArrayCreator, decisionMaker, deleteService);
            return new WorkShiftBackToLegalStateServicePro(workShiftBackToLegalStateStep, workShiftMinMaxCalculator);
        }

    }
}