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
    public interface IWeeklyRestSolverService
    {
        void Execute(IList<IPerson> selectedPerson, IList<IScheduleMatrixPro> allMatrixOnSelectedPeriod,
            DateOnlyPeriod selectedPeriod);
    }
    public class WeeklyRestSolverService : IWeeklyRestSolverService
    {
        private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
        private readonly IEnsureWeeklyRestRule _ensureWeeklyRestRule;
        private readonly IContractWeeklyRestForPersonWeek _contractWeeklyRestForPersonWeek;
        private readonly IDayOffToTimeSpanExtractor _dayOffToTimeSpanExtractor;
	    private readonly IShiftNudgeManager _shiftNudgeManager;

        public WeeklyRestSolverService(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor, IEnsureWeeklyRestRule ensureWeeklyRestRule, 
                        IContractWeeklyRestForPersonWeek contractWeeklyRestForPersonWeek, IDayOffToTimeSpanExtractor dayOffToTimeSpanExtractor)
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
                    var selctedPersonWeeks = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(selectedPeriodScheduleDays, true);
                    var personWeeksVoilatingWeeklyRest = new List<PersonWeek>();
                    foreach (var personWeek in selctedPersonWeeks)
				    {
					    var weeklyRest = _contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(personWeek);
                        if (!weeklyRestInPersonWeek.ContainsKey(personWeek)) weeklyRestInPersonWeek.Add(personWeek, weeklyRest);
                        if(!_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek,personScheduleRange,weeklyRest  ) )
                            personWeeksVoilatingWeeklyRest.Add(personWeek);

				    }

				    //solving the weeks
                    foreach (var personWeek in personWeeksVoilatingWeeklyRest)
				    {
					    var possiblePositionsToFix = _dayOffToTimeSpanExtractor.GetDayOffWithTimeSpanAmongAWeek(personWeek.Week,
                        while (possiblePositionsToFix.Count() != 0)
					    bool success = false;
					    {
                            var highProbablePosition = getHighProbablePosition(possiblePositionsToFix);
                            startPokingShifts();
                            if (!_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personScheduleRange,weeklyRestInPersonWeek[personWeek]))
                                break;
                            possiblePositionsToFix.Remove(highProbablePosition);
                        }
                    }
					    if (!success)
					    {
						    //do something
                }
            }
            
        }

        private void startPokingShifts()
        {
            //while no validation error found continue doing below

            //poke the day before

            //do we need to check here ? it think yes
            //if (!_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personScheduleRange, weeklyRestInPersonWeek[personWeek]))
            //    return;

            //poke the day after
        }

        private DateOnly getHighProbablePosition(IDictionary<DateOnly, TimeSpan> possiblePositionsToFix)
        {
            var higestSpan = TimeSpan.MinValue;
            var resultDate = new DateOnly();
            foreach (var day in possiblePositionsToFix)
            {
                if (day.Value > higestSpan)
                {
                    higestSpan = day.Value;
                    resultDate = day.Key;
                }
            }
            return  resultDate;
        }
    }
}
