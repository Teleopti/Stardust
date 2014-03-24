using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public class WeeklyRestSolverService
    {
        private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
        private readonly IEnsureWeeklyRestRule _ensureWeeklyRestRule;
        private readonly IContractWeeklyRestForPersonWeek _contractWeeklyRestForPersonWeek;
        private readonly IDayOffToTimeSpanExtractor _dayOffToTimeSpanExtractor;
	    private readonly IShiftNudgeManager _shiftNudgeManager;

	    public WeeklyRestSolverService(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor, IEnsureWeeklyRestRule ensureWeeklyRestRule, IContractWeeklyRestForPersonWeek contractWeeklyRestForPersonWeek, IDayOffToTimeSpanExtractor dayOffToTimeSpanExtractor, IShiftNudgeManager shiftNudgeManager)
        {
            _weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
            _ensureWeeklyRestRule = ensureWeeklyRestRule;
            _contractWeeklyRestForPersonWeek = contractWeeklyRestForPersonWeek;
            _dayOffToTimeSpanExtractor = dayOffToTimeSpanExtractor;
	        _shiftNudgeManager = shiftNudgeManager;
        }

	    public void Execute(IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> allMatrixOnSelectedPeriod,
		    DateOnlyPeriod selectedPeriod, ITeamBlockGenerator teamBlockGenerator, ISchedulingOptions schedulingOptions,
		    ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer,
		    ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allPersonMatrixList)
	    {
		    foreach (var person in selectedPersons)
		    {
			    var personMatrix = allMatrixOnSelectedPeriod.FirstOrDefault(s => s.Person == person);
			    if (personMatrix != null)
			    {
				    var personScheduleRange = personMatrix.ActiveScheduleRange;
				    var scheduleDays = personScheduleRange.ScheduledDayCollection(selectedPeriod);
				    var personWeeks = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDays, true);
				    var personWeeksWithProblem = new List<PersonWeek>();
				    foreach (var personWeek in personWeeks)
				    {
					    var weeklyRest = _contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(personWeek);
					    if (_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personScheduleRange, weeklyRest))
						    personWeeksWithProblem.Add(personWeek);

				    }

				    //solving the weeks
				    foreach (var personWeek in personWeeksWithProblem)
				    {
					    var possiblePositionsToFix = _dayOffToTimeSpanExtractor.GetDayOffWithTimeSpanAmongAWeek(personWeek.Week,
						    personScheduleRange);
					    bool success = false;
					    foreach (var possiblePosition in possiblePositionsToFix)
					    {
						    success = _shiftNudgeManager.TrySolveForDayOff(personWeek, possiblePosition.Key, teamBlockGenerator,
								allPersonMatrixList, schedulingOptions, rollbackService, resourceCalculateDelayer,
							    schedulingResultStateHolder, selectedPeriod, selectedPersons);

						    if (success)
							    break;
					    }
					    if (!success)
					    {
						    //do something
					    }
				    }

			    }


		    }

	    }
    }
}
