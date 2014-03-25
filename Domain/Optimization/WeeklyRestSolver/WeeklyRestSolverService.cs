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
        void Execute(IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> allMatrixOnSelectedPeriod,
            DateOnlyPeriod selectedPeriod, ITeamBlockGenerator teamBlockGenerator, ISchedulingOptions schedulingOptions,
            ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer,
            ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allPersonMatrixList,
            IOptimizationPreferences optimizationPreferences);
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

	    public void Execute(IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> allMatrixOnSelectedPeriod,
		    DateOnlyPeriod selectedPeriod, ITeamBlockGenerator teamBlockGenerator, ISchedulingOptions schedulingOptions,
		    ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer,
		    ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allPersonMatrixList,
			IOptimizationPreferences optimizationPreferences)
	    {
	        foreach (var person in selectedPersons)
	        {
	            var personMatrix = allMatrixOnSelectedPeriod.FirstOrDefault(s => s.Person == person);
	            var weeklyRestInPersonWeek = new Dictionary<PersonWeek, TimeSpan>();
	            if (personMatrix != null)
	            {
	                var personScheduleRange = personMatrix.ActiveScheduleRange;
	                var selectedPeriodScheduleDays = personScheduleRange.ScheduledDayCollection(selectedPeriod);
	                var selctedPersonWeeks =
	                    _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(selectedPeriodScheduleDays,
	                        true);
	                var personWeeksVoilatingWeeklyRest = new List<PersonWeek>();
	                foreach (var personWeek in selctedPersonWeeks)
	                {
	                    var weeklyRest = _contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(personWeek);
	                    if (!weeklyRestInPersonWeek.ContainsKey(personWeek))
	                        weeklyRestInPersonWeek.Add(personWeek, weeklyRest);
	                    if (!_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personScheduleRange, weeklyRest))
	                        personWeeksVoilatingWeeklyRest.Add(personWeek);

	                }

	                foreach (var personWeek in personWeeksVoilatingWeeklyRest)
	                {
	                    if (_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personScheduleRange,weeklyRestInPersonWeek[personWeek])) continue;
	                    var possiblePositionsToFix = _dayOffToTimeSpanExtractor.GetDayOffWithTimeSpanAmongAWeek(personWeek.Week, personScheduleRange);
	                    bool success = false;
	                    while (possiblePositionsToFix.Count() != 0)
	                    {
	                        var highProbablePosition =_identifyDayOffWithHighestSpan.GetHighProbableDayOffPosition(possiblePositionsToFix);
	                        success = _shiftNudgeManager.TrySolveForDayOff(personWeek, highProbablePosition,
	                            teamBlockGenerator,
	                            allPersonMatrixList, schedulingOptions, rollbackService, resourceCalculateDelayer,
	                            schedulingResultStateHolder, selectedPeriod, selectedPersons, optimizationPreferences);

	                        if (success)
	                        {
                                var foundProblemWithThisWeek = _brokenWeekOutsideSelectionSpecification.IsSatisfy( personWeek, selctedPersonWeeks.ToList(),  weeklyRestInPersonWeek, personScheduleRange);
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
	                    if (!success)
	                    {
                            _deleteScheduleDayFromUnsolvedPersonWeek.DeleteAppropiateScheduleDay(personScheduleRange,possiblePositionsToFix.First().Key ,rollbackService );
	                    }
	                }
	            }
	        }

	    }

        //private bool checkForBrokenWeeksOutsideSelection(PersonWeek personWeek, IEnumerable<PersonWeek> selctedPersonWeeks, DateOnlyPeriod selectedPeriod, Dictionary<PersonWeek, TimeSpan> weeklyRestInPersonWeek, IScheduleRange personScheduleRange)
        //{
        //    //check week before
        //    var dateInPreviousWeek = personWeek.Week.StartDate.AddDays(-1);
        //    var dateInNextWeek = personWeek.Week.StartDate.AddDays(1);
        //    var foundAProblemInThisWeek = false;
        //    if (!selectedPeriod.Contains(dateInPreviousWeek))
        //    {
        //        var previousPersonWeek = selctedPersonWeeks.FirstOrDefault(s => s.Week.Contains(dateInPreviousWeek));
        //        if (!_ensureWeeklyRestRule.HasMinWeeklyRest(previousPersonWeek, personScheduleRange, weeklyRestInPersonWeek[personWeek])) foundAProblemInThisWeek = true;
        //    }
        //    if (!foundAProblemInThisWeek && !selectedPeriod.Contains(dateInNextWeek))
        //    {
        //        var nextPersonWeek = selctedPersonWeeks.FirstOrDefault(s => s.Week.Contains(dateInNextWeek ));
        //        if (!_ensureWeeklyRestRule.HasMinWeeklyRest(nextPersonWeek, personScheduleRange, weeklyRestInPersonWeek[personWeek])) foundAProblemInThisWeek = true;
        //    }
        //    return foundAProblemInThisWeek ;
        //}
        

    }
}
