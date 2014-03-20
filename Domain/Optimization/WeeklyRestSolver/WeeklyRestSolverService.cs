using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public WeeklyRestSolverService(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor, IEnsureWeeklyRestRule ensureWeeklyRestRule, IContractWeeklyRestForPersonWeek contractWeeklyRestForPersonWeek, IDayOffToTimeSpanExtractor dayOffToTimeSpanExtractor)
        {
            _weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
            _ensureWeeklyRestRule = ensureWeeklyRestRule;
            _contractWeeklyRestForPersonWeek = contractWeeklyRestForPersonWeek;
            _dayOffToTimeSpanExtractor = dayOffToTimeSpanExtractor;
        }

        public void Execute(IList<IPerson> selectedPerson, IList<IScheduleMatrixPro> allMatrixOnSelectedPeriod, DateOnlyPeriod selectedPeriod)
        {
            foreach (var person in selectedPerson)
            {
                var personMatrix =  allMatrixOnSelectedPeriod.FirstOrDefault(s => s.Person == person) ;
                if (personMatrix != null)
                {
                    var personScheduleRange = personMatrix.ActiveScheduleRange;
                    var scheduleDays = personScheduleRange.ScheduledDayCollection(selectedPeriod);
                    var personWeeks = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDays, true);
                    var personWeeksWithProblem = new List<PersonWeek>();
                    foreach (var personWeek in personWeeks)
                    {
                        var weeklyRest = _contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(personWeek);
                        if(_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek,personScheduleRange,weeklyRest  ) )
                            personWeeksWithProblem.Add(personWeek );

                    }

                    //solving the weeks
                    foreach (var personWeek in personWeeksWithProblem)
                    {
                        var possiblePositionsToFix  =  _dayOffToTimeSpanExtractor.GetDayOffWithTimeSpanAmongAWeek(personWeek.Week, personScheduleRange);
                        foreach (var possiblePosition in possiblePositionsToFix)
                        {
                            //fix it using nudge
                        }
                    }

                }

                
            }
            
        }  
    }
}
