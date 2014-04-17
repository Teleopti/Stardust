using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public interface IWeeklyRestSolverService
    {
	    void Execute(IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod, ITeamBlockGenerator teamBlockGenerator, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allPersonMatrixList, IOptimizationPreferences optimizationPreferences, ISchedulingOptions schedulingOptions);
        event EventHandler<BlockSchedulingServiceEventArgs> ResolvingWeek;
    }
    public class WeeklyRestSolverService : IWeeklyRestSolverService
    {
        private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
        private readonly IEnsureWeeklyRestRule _ensureWeeklyRestRule;
        private readonly IContractWeeklyRestForPersonWeek _contractWeeklyRestForPersonWeek;
        private readonly IDayOffToTimeSpanExtractor _dayOffToTimeSpanExtractor;
	    private readonly IShiftNudgeManager _shiftNudgeManager;
        private readonly IdentifyDayOffWithHighestSpan _identifyDayOffWithHighestSpan;
        private readonly IDeleteScheduleDayFromUnsolvedPersonWeek _deleteScheduleDayFromUnsolvedPersonWeek;
        private readonly IBrokenWeekOutsideSelectionSpecification _brokenWeekOutsideSelectionSpecification;
        private bool _cancelMe;
        public event EventHandler<BlockSchedulingServiceEventArgs> ResolvingWeek;

        public WeeklyRestSolverService(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor, IEnsureWeeklyRestRule ensureWeeklyRestRule, IContractWeeklyRestForPersonWeek contractWeeklyRestForPersonWeek, 
                    IDayOffToTimeSpanExtractor dayOffToTimeSpanExtractor, IShiftNudgeManager shiftNudgeManager,  IdentifyDayOffWithHighestSpan identifyDayOffWithHighestSpan, 
                    IDeleteScheduleDayFromUnsolvedPersonWeek deleteScheduleDayFromUnsolvedPersonWeek, IBrokenWeekOutsideSelectionSpecification brokenWeekOutsideSelectionSpecification)
        {
            _weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
            _ensureWeeklyRestRule = ensureWeeklyRestRule;
            _contractWeeklyRestForPersonWeek = contractWeeklyRestForPersonWeek;
            _dayOffToTimeSpanExtractor = dayOffToTimeSpanExtractor;
	        _shiftNudgeManager = shiftNudgeManager;
            _identifyDayOffWithHighestSpan = identifyDayOffWithHighestSpan;
            _deleteScheduleDayFromUnsolvedPersonWeek = deleteScheduleDayFromUnsolvedPersonWeek;
            _brokenWeekOutsideSelectionSpecification = brokenWeekOutsideSelectionSpecification;
        }

        protected virtual void OnDayScheduled(BlockSchedulingServiceEventArgs blockSchedulingServiceEventArgs )
        {
            EventHandler<BlockSchedulingServiceEventArgs> temp = ResolvingWeek;
            if (temp != null)
            {
                temp(this, blockSchedulingServiceEventArgs);
            }
            _cancelMe = blockSchedulingServiceEventArgs.Cancel;
        }

	    public void Execute(IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod, ITeamBlockGenerator teamBlockGenerator, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allPersonMatrixList, IOptimizationPreferences optimizationPreferences, ISchedulingOptions schedulingOptions)
	    {
	        foreach (var person in selectedPersons)
	        {
	            var personMatrix = allPersonMatrixList.FirstOrDefault(s => s.Person == person);
	            var weeklyRestInPersonWeek = new Dictionary<PersonWeek, TimeSpan>();
	            if (personMatrix != null)
	            {
						var personScheduleRange = schedulingResultStateHolder.Schedules[person];
	                var selectedPeriodScheduleDays = personScheduleRange.ScheduledDayCollection(selectedPeriod);
	                var selctedPersonWeeks =
	                    _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(selectedPeriodScheduleDays,
	                        false).ToList();
	                var personWeeksVoilatingWeeklyRest = new List<PersonWeek>();
	                foreach (var personWeek in selctedPersonWeeks)
	                {
	                    var weeklyRest = _contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(personWeek);
	                    if (!weeklyRestInPersonWeek.ContainsKey(personWeek))
	                        weeklyRestInPersonWeek.Add(personWeek, weeklyRest);
	                    if (!_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personScheduleRange, weeklyRest))
	                        personWeeksVoilatingWeeklyRest.Add(personWeek);

	                }
						 var selctedPersonWeeksWithNeighbouringWeeks =
								_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(selectedPeriodScheduleDays,
									 true).ToList();
	                foreach (var personWeek in personWeeksVoilatingWeeklyRest)
	                {
	                    if (_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personScheduleRange,weeklyRestInPersonWeek[personWeek])) continue;
	                    var possiblePositionsToFix = _dayOffToTimeSpanExtractor.GetDayOffWithTimeSpanAmongAWeek(personWeek.Week, personScheduleRange);
	                    var fisrtDayOfElement = DateOnly.MinValue;
	                    if (possiblePositionsToFix.Count > 0)
	                        fisrtDayOfElement = possiblePositionsToFix.FirstOrDefault().Key;
	                    bool success = true;
                        while (possiblePositionsToFix.Count() != 0)
	                    {
	                        OnDayScheduled(new BlockSchedulingServiceEventArgs());
                            if (_cancelMe)
                                return;
                            var highProbablePosition =_identifyDayOffWithHighestSpan.GetHighProbableDayOffPosition(possiblePositionsToFix);
	                        success = _shiftNudgeManager.TrySolveForDayOff(personWeek, highProbablePosition,
	                            teamBlockGenerator,
	                            allPersonMatrixList, rollbackService, resourceCalculateDelayer,
	                            schedulingResultStateHolder, selectedPeriod, selectedPersons, optimizationPreferences,schedulingOptions );

	                        if (success)
	                        {
										var foundProblemWithThisWeek = _brokenWeekOutsideSelectionSpecification.IsSatisfy(personWeek, selctedPersonWeeksWithNeighbouringWeeks, weeklyRestInPersonWeek, personScheduleRange);
	                            if (foundProblemWithThisWeek)
	                            {
	                                //rollback this week 
	                                _shiftNudgeManager.RollbackLastScheduledWeek(rollbackService, resourceCalculateDelayer);
                                    _deleteScheduleDayFromUnsolvedPersonWeek.DeleteAppropiateScheduleDay(personScheduleRange, possiblePositionsToFix.First().Key, rollbackService);
	                            }
                                break;
	                        }
	                            
	                        possiblePositionsToFix.Remove(highProbablePosition);
	                    }
	                    if (!success  && fisrtDayOfElement != DateOnly.MinValue)
	                    {
                            _deleteScheduleDayFromUnsolvedPersonWeek.DeleteAppropiateScheduleDay(personScheduleRange, fisrtDayOfElement, rollbackService);
	                    }
	                }
	            }
	        }

	    }


    }
}
