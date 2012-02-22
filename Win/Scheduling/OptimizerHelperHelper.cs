using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Autofac;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Scheduling
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static class OptimizerHelperHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "optimizationPreferences"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public static void ScheduleBlankSpots(IOptimizationPreferences optimizationPreferences, IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
            IScheduleService scheduleService, IComponentContext container)
        {
            // ***** create SchedulingOptions here
            ISchedulingOptions schedulingOptions = new SchedulingOptions();

            var effectiveRestrictionCreator = container.Resolve<IEffectiveRestrictionCreator>();

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
                        result = scheduleService.SchedulePersonOnDay(scheduleDayPro.DaySchedulePart(), false, effectiveRestriction);
                    }
                    if (!result)
                    {
                        matrixOriginalStateContainer.StillAlive = false;
                        break;
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "optimizerPreferences"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void LockDaysForDayOffOptimization(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList, ILifetimeScope container)
        {
            // ***** create SchedulingOptions here
            ISchedulingOptions schedulingOptions = new SchedulingOptions();

            var restrictionExtractor = container.Resolve<IRestrictionExtractor>();
            //the bloody optimizerPreferences must be in autofac soon
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
            IScheduleResultDataExtractorProvider dataExtractorProvider = new ScheduleResultDataExtractorProvider(advancedPreferences);
            IScheduleResultDataExtractor allSkillsDataExtractor = dataExtractorProvider.CreateAllSkillsDataExtractor(selectedPeriod, stateHolder);
            return allSkillsDataExtractor;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IScheduleResultDataExtractor CreatePersonalSkillsDataExtractor(
            IAdvancedPreferences advancedPreferences,
            IScheduleMatrixPro scheduleMatrix)
        {
            IScheduleResultDataExtractorProvider dataExtractorProvider = new ScheduleResultDataExtractorProvider(advancedPreferences);
            return dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrix);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static DateOnly GetStartDateInSelection(ClipHandler clipHandler, ICccTimeZoneInfo personTimeZone)
        {
            IEnumerable<IScheduleDay> selectedParts = ContainedSchedulePartList(clipHandler.ClipList);
            return GetStartDateInSelectedDays(selectedParts, personTimeZone);
        }
        public static DateOnly GetStartDateInSelectedDays(IEnumerable<IScheduleDay> selectedParts, ICccTimeZoneInfo personTimeZone)
        {
            return new DateOnly(selectedParts.Min(c => TimeZoneHelper.ConvertFromUtc(c.Period.StartDateTime, personTimeZone)));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static DateOnly GetEndDateInSelection(ClipHandler clipHandler, ICccTimeZoneInfo personTimeZone)
        {
            IEnumerable<IScheduleDay> selectedParts = ContainedSchedulePartList(clipHandler.ClipList);
            return GetEndDateInSelectedDays(selectedParts, personTimeZone);
        }
        public static DateOnly GetEndDateInSelectedDays(IEnumerable<IScheduleDay> selectedParts, ICccTimeZoneInfo personTimeZone)
        {
            return new DateOnly(selectedParts.Max(c => TimeZoneHelper.ConvertFromUtc(c.Period.StartDateTime, personTimeZone)));
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

        public static IList<IScheduleMatrixPro> CreateMatrixList(ClipHandler clipHandler, ISchedulingResultStateHolder resultStateHolder, IComponentContext container)
        {
            if (clipHandler == null) throw new ArgumentNullException("clipHandler");
            IEnumerable<IScheduleDay> scheduleDays = ContainedSchedulePartList(clipHandler.ClipList);
            return CreateMatrixList(scheduleDays, resultStateHolder, container);
        }

        public static IList<IScheduleMatrixPro> CreateMatrixList(IEnumerable<IScheduleDay> scheduleDays, ISchedulingResultStateHolder resultStateHolder, IComponentContext container)
        {
            if (scheduleDays == null) throw new ArgumentNullException("scheduleDays");

            IList<IScheduleMatrixPro> matrixes =
                new ScheduleMatrixListCreator(resultStateHolder).CreateMatrixListFromScheduleParts(scheduleDays);

            var matrixUserLockLocker = container.Resolve<IMatrixUserLockLocker>();
            matrixUserLockLocker.Execute(scheduleDays, matrixes);

            return matrixes;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IEnumerable<IPerson> CreatePersonsList(ClipHandler clipHandler)
        {
            IEnumerable<IScheduleDay> scheduleDays = ContainedSchedulePartList(clipHandler.ClipList);

            IList<IPerson> persons =
                new PersonListExtractorFromScheduleParts(scheduleDays).ExtractPersons().ToList();

            return persons;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Validators")]
        public static IList<IDayOffLegalStateValidator> CreateLegalStateValidators(
           IPerson person,
           ILockableBitArray bitArray,
           IDayOffPlannerSessionRuleSet ruleSet,
           IOptimizationPreferences optimizerPreferences)
        {
            MinMax<int> periodArea = bitArray.PeriodArea;
            if (!ruleSet.ConsiderWeekBefore)
                periodArea = new MinMax<int>(periodArea.Minimum + 7, periodArea.Maximum + 7);
            IOfficialWeekendDays weekendDays = new OfficialWeekendDays(person.PermissionInformation.Culture());
            IDayOffLegalStateValidatorListCreator validatorListCreator =
                new DayOffOptimizationLegalStateValidatorListCreator
                    (optimizerPreferences.DaysOff,
                     weekendDays,
                     bitArray.ToLongBitArray(),
                     periodArea);

            return validatorListCreator.BuildActiveValidatorList();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Validators")]
        public static IList<IDayOffLegalStateValidator> CreateLegalStateValidatorsToKeepWeekendNumbers(
            IPerson person,
            ILockableBitArray bitArray,
            IOptimizationPreferences optimizerPreferences)
        {
            MinMax<int> periodArea = bitArray.PeriodArea;
            if (!optimizerPreferences.DaysOff.ConsiderWeekBefore)
                periodArea = new MinMax<int>(periodArea.Minimum + 7, periodArea.Maximum + 7);
            IOfficialWeekendDays weekendDays = new OfficialWeekendDays(person.PermissionInformation.Culture());
            IDayOffLegalStateValidatorListCreator validatorListCreator =
                new DayOffOptimizationWeekendLegalStateValidatorListCreator(optimizerPreferences.DaysOff,
                     weekendDays,
                     periodArea);

            return validatorListCreator.BuildActiveValidatorList();
        }

        public static IEnumerable<IDayOffDecisionMaker> CreateDecisionMakers(
            CultureInfo culture,
            IPerson person,
            ILockableBitArray scheduleMatrixArray,
            IDayOffPlannerSessionRuleSet ruleSet,
            IOptimizationPreferences optimizerPreferences)
        {
            IList<IDayOffLegalStateValidator> legalStateValidators =
                 CreateLegalStateValidators(person, scheduleMatrixArray, ruleSet, optimizerPreferences);

            IList<IDayOffLegalStateValidator> legalStateValidatorsToKeepWeekEnds =
                CreateLegalStateValidatorsToKeepWeekendNumbers(person, scheduleMatrixArray, optimizerPreferences);

            IOfficialWeekendDays officialWeekendDays = new OfficialWeekendDays(culture);
            ILogWriter logWriter = new LogWriter<DayOffOptimizationService>();

            IDayOffDecisionMaker moveDayOffDecisionMaker = new MoveOneDayOffDecisionMaker(legalStateValidators, logWriter);
            IDayOffDecisionMaker moveWeekEndDecisionMaker = new MoveWeekendDayOffDecisionMaker(legalStateValidatorsToKeepWeekEnds, officialWeekendDays, true, logWriter);
            IDayOffDecisionMaker moveTwoWeekEndDaysDecisionMaker = new MoveWeekendDayOffDecisionMaker(legalStateValidators, officialWeekendDays, false, logWriter);

            return new List<IDayOffDecisionMaker> { moveDayOffDecisionMaker, moveTwoWeekEndDaysDecisionMaker, moveWeekEndDecisionMaker };
        }

        public static void SetConsiderShortBreaks(ClipHandler clipHandler, DateOnlyPeriod period, IReschedulingPreferences options, IComponentContext container)
        {
            IEnumerable<IPerson> persons = CreatePersonsList(clipHandler);
            SetConsiderShortBreaks(persons, period, options, container);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IDayOffPlannerSessionRuleSet DayOffPlannerRuleSetFromOptimizerPreferences(IDaysOffPreferences daysOffPreferences)
        {
            var creator = new DayOffPlannerRuleSetCreator();
            return creator.CreateDayOffPlannerSessionRuleSet(daysOffPreferences);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IList<ISmartDayOffBackToLegalStateSolverContainer> CreateSmartDayOffSolverContainers(
            IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
            IDayOffPlannerSessionRuleSet dayOffPlannerSessionRuleSet)
        {
            IList<ISmartDayOffBackToLegalStateSolverContainer> solverContainers = new List<ISmartDayOffBackToLegalStateSolverContainer>();
            foreach (var matrixOriginalStateContainer in matrixOriginalStateContainers)
            {
                IScheduleMatrixLockableBitArrayConverter bitArrayConverter =
                    new ScheduleMatrixLockableBitArrayConverter(matrixOriginalStateContainer.ScheduleMatrix);

                ILockableBitArray bitArray = bitArrayConverter.Convert(dayOffPlannerSessionRuleSet.ConsiderWeekBefore,
                                                                      dayOffPlannerSessionRuleSet.ConsiderWeekAfter);

                IDayOffBackToLegalStateFunctions functions = new DayOffBackToLegalStateFunctions(bitArray, matrixOriginalStateContainer.ScheduleMatrix.Person.PermissionInformation.Culture());
                ISmartDayOffBackToLegalStateService solverService = new SmartDayOffBackToLegalStateService(functions, dayOffPlannerSessionRuleSet, 20);
                ISmartDayOffBackToLegalStateSolverContainer solverContainer = new SmartDayOffBackToLegalStateSolverContainer(matrixOriginalStateContainer, bitArray, solverService);
                solverContainers.Add(solverContainer);
            }

            return solverContainers;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public static void SyncSmartDayOffContainerWithMatrix(ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer,
            IDayOffTemplate dayOffTemplate, IDayOffPlannerSessionRuleSet dayOffPlannerSessionRuleSet, IScheduleDayChangeCallback scheduleDayChangeCallback,
            IScheduleTagSetter scheduleTagSetter)
        {
            if (backToLegalStateSolverContainer.Result)
            {
                IScheduleMatrixPro matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;
                IScheduleMatrixLockableBitArrayConverter bitArrayConverter =
                    new ScheduleMatrixLockableBitArrayConverter(matrix);

                ILockableBitArray doArray = bitArrayConverter.Convert(dayOffPlannerSessionRuleSet.ConsiderWeekBefore,
                                                                      dayOffPlannerSessionRuleSet.ConsiderWeekAfter);

                int bitArrayToMatrixOffset = 0;
                if (!dayOffPlannerSessionRuleSet.ConsiderWeekBefore)
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
        public static IWorkShiftBackToLegalStateServicePro CreateWorkShiftBackToLegalStateServicePro(IScheduleMatrixPro scheduleMatrix,
            ISchedulePartModifyAndRollbackService rollbackService, ILifetimeScope container)
        {
            var workShiftMinMaxCalculator = container.Resolve<IWorkShiftMinMaxCalculator>();
            var bitArrayCreator = container.Resolve<IWorkShiftBackToLegalStateBitArrayCreator>();
            var period = new DateOnlyPeriod(scheduleMatrix.FullWeeksPeriodDays[0].Day, scheduleMatrix.FullWeeksPeriodDays[scheduleMatrix.FullWeeksPeriodDays.Count - 1].Day);

            var dailySkillForecastAndScheduledValueCalculator =
                container.Resolve<IDailySkillForecastAndScheduledValueCalculator>();

            ISkillExtractor allSkillExtractor = container.Resolve<SchedulingStateHolderAllSkillExtractor>();

            // when we move the period to the method we can have all this in autofac
            IScheduleResultDataExtractor dataExtractor = new RelativeDailyDifferencesByAllSkillsExtractor(period, dailySkillForecastAndScheduledValueCalculator, allSkillExtractor);

            var dayIndexCalculator = container.Resolve<IWorkShiftLegalStateDayIndexCalculator>();
            IWorkShiftBackToLegalStateDecisionMaker decisionMaker = new WorkShiftBackToLegalStateDecisionMaker(dataExtractor, dayIndexCalculator);
            var deleteService = container.Resolve<IDeleteSchedulePartService>();
            IWorkShiftBackToLegalStateStep workShiftBackToLegalStateStep =
                new WorkShiftBackToLegalStateStep(bitArrayCreator, decisionMaker, deleteService, rollbackService);
            return new WorkShiftBackToLegalStateServicePro(workShiftBackToLegalStateStep, workShiftMinMaxCalculator);
        }

    }
}