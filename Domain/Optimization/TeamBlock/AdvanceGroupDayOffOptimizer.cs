using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface IAdvanceGroupDayOffOptimizer
	{
		bool Execute(IScheduleMatrixPro matrix,
									 IList<IScheduleMatrixPro> allMatrixes,
									 ISchedulingOptions schedulingOptions,
									 IOptimizationPreferences optimizationPreferences,
									 ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler,
									 ITeamSteadyStateHolder teamSteadyStateHolder,
									 IScheduleDictionary scheduleDictionary,
									 IList<IPerson> selectedPerson,
									 IList<IScheduleMatrixPro> allPersonMatrixList);
	}

    public class AdvanceGroupDayOffOptimizer : IAdvanceGroupDayOffOptimizer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _converter;
        private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
        private readonly IDayOffDecisionMaker _decisionMaker;
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private readonly ILockableBitArrayChangesTracker _changesTracker;
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private readonly IGroupMatrixHelper _groupMatrixHelper;
        private readonly IGroupOptimizationValidatorRunner _groupOptimizationValidatorRunner;
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
        private readonly ISmartDayOffBackToLegalStateService _smartDayOffBackToLegalStateService;
        private readonly IRestrictionAggregator _restrictionAggregator;
        private readonly IDynamicBlockFinder _dynamicBlockFinder;
        private readonly IGroupPersonBuilderBasedOnContractTime _groupPersonBuilderBasedOnContractTime;
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
        private readonly IWorkShiftFilterService _workShiftFilterService;
        private readonly IWorkShiftSelector _workShiftSelector;
        private readonly ITeamScheduling _teamScheduling;

        public bool TeamSchedulingSuccessfullForTesting { get; set; }

        public AdvanceGroupDayOffOptimizer(IScheduleMatrixLockableBitArrayConverter converter,
                                           IDayOffDecisionMaker decisionMaker,
                                           IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
                                           IDaysOffPreferences daysOffPreferences,
                                           IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
                                           ILockableBitArrayChangesTracker changesTracker,
                                           ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
                                           IGroupMatrixHelper groupMatrixHelper,
                                           IGroupOptimizationValidatorRunner groupOptimizationValidatorRunner,
                                           IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
                                           ISmartDayOffBackToLegalStateService smartDayOffBackToLegalStateService,
                                           IRestrictionAggregator restrictionAggregator, 
                                           IDynamicBlockFinder dynamicBlockFinder,
                                           IGroupPersonBuilderBasedOnContractTime groupPersonBuilderBasedOnContractTime,
                                           ISchedulingOptions schedulingOptions,
                                           ISkillDayPeriodIntervalDataGenerator skillDayPeriodIntervalDataGenerator,
                                           IWorkShiftFilterService workShiftFilterService,
                                           IWorkShiftSelector workShiftSelector,
                                           ITeamScheduling teamScheduling)
        {
            _converter = converter;
            _scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
            _decisionMaker = decisionMaker;
            _daysOffPreferences = daysOffPreferences;
            _dayOffDecisionMakerExecuter = dayOffDecisionMakerExecuter;
            _changesTracker = changesTracker;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
            _groupMatrixHelper = groupMatrixHelper;
            _groupOptimizationValidatorRunner = groupOptimizationValidatorRunner;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
            _smartDayOffBackToLegalStateService = smartDayOffBackToLegalStateService;
            _restrictionAggregator = restrictionAggregator;
            _dynamicBlockFinder = dynamicBlockFinder;
            _groupPersonBuilderBasedOnContractTime = groupPersonBuilderBasedOnContractTime;
            _schedulingOptions = schedulingOptions;
            _skillDayPeriodIntervalDataGenerator = skillDayPeriodIntervalDataGenerator;
            _workShiftFilterService = workShiftFilterService;
            _workShiftSelector = workShiftSelector;
            _teamScheduling = teamScheduling;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public bool Execute(IScheduleMatrixPro matrix, 
                            IList<IScheduleMatrixPro> allMatrixes, 
                            ISchedulingOptions schedulingOptions, 
                            IOptimizationPreferences optimizationPreferences, 
                            ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, 
                            ITeamSteadyStateHolder teamSteadyStateHolder, 
                            IScheduleDictionary scheduleDictionary,
                            IList<IPerson> selectedPerson,
                            IList<IScheduleMatrixPro> allPersonMatrixList)
        {
            ILockableBitArray originalArray = _converter.Convert(_daysOffPreferences.ConsiderWeekBefore,
                                                                 _daysOffPreferences.ConsiderWeekAfter);
            WorkingBitArray = _converter.Convert(_daysOffPreferences.ConsiderWeekBefore,
                                                 _daysOffPreferences.ConsiderWeekAfter);

            if (!dataExtractorExecute(matrix)) return false;

            IList<DateOnly> daysOffToRemove = _changesTracker.DaysOffRemoved(WorkingBitArray, originalArray, matrix,
                                                                             _daysOffPreferences.ConsiderWeekBefore);
            IList<DateOnly> daysOffToAdd = _changesTracker.DaysOffAdded(WorkingBitArray, originalArray, matrix,
                                                                        _daysOffPreferences.ConsiderWeekBefore);

            if (daysOffToRemove.Count == 0)
                return false;
            IPerson matrixPerson = matrix.Person;
            IGroupPerson matrixGroupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(matrixPerson, daysOffToRemove[0]);
            if (matrixGroupPerson == null)
                return false;

            var result = new ValidatorResult();
            result.Success = true;

            IList<GroupMatrixContainer> containers;

            if (schedulingOptions.UseSameDayOffs)
            {
                //Will always return true IFairnessValueCalculator not using GroupOptimizerValidateProposedDatesInSameMatrix daysoff
                result = _groupOptimizationValidatorRunner.Run(matrixPerson, daysOffToRemove, daysOffToAdd, schedulingOptions.UseSameDayOffs);
                if (!result.Success)
                {
                    return false;
                }

                containers = _groupMatrixHelper.CreateGroupMatrixContainers(allMatrixes, daysOffToRemove, daysOffToAdd, matrixGroupPerson, _daysOffPreferences);
            }
            else
            {
                containers = _groupMatrixHelper.CreateGroupMatrixContainers(allMatrixes, daysOffToRemove, daysOffToAdd, matrixPerson, _daysOffPreferences);
            }

            if (containers == null || containers.Count() == 0)
                return false;

            if (result.MatrixList.Count == 0)
            {
                //kan flytta hur mycket som helst, beh�ver f� veta vad som ska schemal�ggas
                if (!_groupMatrixHelper.ExecuteDayOffMoves(containers, _dayOffDecisionMakerExecuter, _schedulePartModifyAndRollbackService))
                    return false;
                daysOffToRemove = _changesTracker.DaysOffRemoved(WorkingBitArray, originalArray, matrix,
                                                                 _daysOffPreferences.ConsiderWeekBefore);

                IList<IScheduleDay> removedDays = _groupMatrixHelper.GoBackToLegalState(daysOffToRemove, matrixGroupPerson, schedulingOptions, allMatrixes, _schedulePartModifyAndRollbackService);
                if (removedDays == null)
                    return false;

                //var teamSteadyStateSuccess = false;

                //if (teamSteadyStateHolder.IsSteadyState(groupPerson))
                //{
                //    foreach (var dateOnly in daysOffToRemove)
                //    {
                //        teamSteadyStateSuccess = teamSteadyStateMainShiftScheduler.ScheduleTeam(dateOnly, groupPerson, _groupSchedulingService, _schedulePartModifyAndRollbackService, schedulingOptions, _groupPersonBuilderForOptimization, allMatrixes, scheduleDictionary);

                //        if (!teamSteadyStateSuccess)
                //            break;
                //    }
                //}

                //if (!teamSteadyStateSuccess)
                //{
                //    if (!_groupMatrixHelper.ScheduleRemovedDayOffDays(daysOffToRemove, groupPerson, _groupSchedulingService, _schedulePartModifyAndRollbackService, schedulingOptions, _groupPersonBuilderForOptimization, allMatrixes))
                //        return false;
                //}

                //if (!_groupMatrixHelper.ScheduleBackToLegalStateDays(removedDays, _groupSchedulingService, _schedulePartModifyAndRollbackService, schedulingOptions, optimizationPreferences, _groupPersonBuilderForOptimization, allMatrixes))
                //    return false;

                //combining the two lists
                var dayOffDates = daysOffToRemove;
                foreach(var scheduleDay in removedDays)
                    if(!dayOffDates.Contains(scheduleDay.DateOnlyAsPeriod.DateOnly))
                        dayOffDates.Add(scheduleDay.DateOnlyAsPeriod.DateOnly );

                executeAdvanceBlockScheduling(allMatrixes, teamSteadyStateHolder, selectedPerson, allPersonMatrixList, dayOffDates);
            }

            return true;
        }

        private void executeAdvanceBlockScheduling(IList<IScheduleMatrixPro> allMatrixes, ITeamSteadyStateHolder teamSteadyStateHolder,
                                                   IList<IPerson> selectedPerson, IList<IScheduleMatrixPro> allPersonMatrixList, IList<DateOnly> dayOffDates)
        {
			//var unLockedDays = new List<DateOnly>();
			//for (var i = 0; i < allMatrixes.Count; i++)
			//{
			//    var openMatrixList = allMatrixes.Where(x => x.Person.Equals(allMatrixes[i].Person));
			//    foreach (var scheduleMatrixPro in openMatrixList)
			//    {
			//        foreach (var scheduleDayPro in scheduleMatrixPro.EffectivePeriodDays.OrderBy(x => x.Day))
			//        {
			//            if (scheduleMatrixPro.UnlockedDays.Contains(scheduleDayPro))
			//                unLockedDays.Add(scheduleDayPro.Day);
			//        }
			//    }
			//}

			//foreach (var dateOnly in dayOffDates)
			//{
			//    var allGroupPersonListOnStartDate = new HashSet<IGroupPerson>();

			//    foreach (var person in selectedPerson)
			//    {
			//        allGroupPersonListOnStartDate.Add(_groupPersonBuilderForOptimization.BuildGroupPerson(person, dateOnly));
			//    }

			//    foreach (var fullGroupPerson in allGroupPersonListOnStartDate.GetRandom(allGroupPersonListOnStartDate.Count, true))
			//    {
			//        if (!teamSteadyStateHolder.IsSteadyState(fullGroupPerson))
			//        {
			//            TeamSchedulingSuccessfullForTesting = false;
			//            continue;
			//        }
			//        var dateOnlyList = _dynamicBlockFinder.ExtractBlockDays(dateOnly,fullGroupPerson );
			//        var groupPersonList = _groupPersonBuilderBasedOnContractTime.SplitTeams(fullGroupPerson, dateOnly);
			//        foreach (var groupPerson in groupPersonList)
			//        {
			//            var groupMatrixList = getScheduleMatrixProList(groupPerson, dateOnly, allPersonMatrixList);
                        
			//            var restriction = _restrictionAggregator.Aggregate(dateOnlyList, groupPerson, groupMatrixList,
			//                                                               _schedulingOptions);

			//            var activityInternalData = _skillDayPeriodIntervalDataGenerator.Generate(fullGroupPerson, dateOnlyList);

			//            var shifts = _workShiftFilterService.Filter(dateOnly, groupPerson, groupMatrixList, restriction,
			//                                                        _schedulingOptions);
			//            if (shifts != null && shifts.Count > 0)
			//            {
			//                IShiftProjectionCache bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts,
			//                                                                                                               activityInternalData,
			//                                                                                                               _schedulingOptions.
			//                                                                                                                   WorkShiftLengthHintOption,
			//                                                                                                               _schedulingOptions.
			//                                                                                                                   UseMinimumPersons,
			//                                                                                                               _schedulingOptions.
			//                                                                                                                   UseMaximumPersons);

			//                _teamScheduling.Execute(dateOnlyList, groupMatrixList, groupPerson,
			//                                        bestShiftProjectionCache, unLockedDays, selectedPerson);
			//                //if (_cancelMe)
			//                //    break;
			//            }
			//            //if (_cancelMe)
			//            //    break;
			//        }
			//        //if (_cancelMe)
			//        //    break;
			//    }
			//}
        }

        private bool dataExtractorExecute(IScheduleMatrixPro matrix)
        {
            IScheduleResultDataExtractor scheduleResultDataExtractor =
                _scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix);

            bool success = _decisionMaker.Execute(WorkingBitArray, scheduleResultDataExtractor.Values());
            if (!success)
            {
                success =
                    _smartDayOffBackToLegalStateService.Execute(
                        _smartDayOffBackToLegalStateService.BuildSolverList(WorkingBitArray), 100);
                if (!success)
                    return false;

                success = _decisionMaker.Execute(WorkingBitArray, scheduleResultDataExtractor.Values());
                if (!success)
                    return false;
            }
            // DayOffBackToLegal if decisionMaker did something wrong
            success =
                _smartDayOffBackToLegalStateService.Execute(
                    _smartDayOffBackToLegalStateService.BuildSolverList(WorkingBitArray), 100);
            if (!success)
                return false;
            return true;
        }

        public ILockableBitArray WorkingBitArray { get; private set; }

        private static List<IScheduleMatrixPro> getScheduleMatrixProList(IGroupPerson groupPerson, DateOnly startDate, IEnumerable<IScheduleMatrixPro> matrixList)
        {
            var person = groupPerson;
            var date = startDate;
            var groupMatrixList =
                matrixList.Where(x => person.GroupMembers.Contains(x.Person) && x.SchedulePeriod.DateOnlyPeriod.Contains(date))
                    .ToList();
            return groupMatrixList;
        }
    }

   
}