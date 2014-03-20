using System;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public interface IContractWeeklyRestForPersonWeek
    {
        TimeSpan GetWeeklyRestFromContract(PersonWeek personWeek);
    }

    public class ContractWeeklyRestForPersonWeek : IContractWeeklyRestForPersonWeek
    {
        public TimeSpan GetWeeklyRestFromContract(PersonWeek personWeek)
        {
            var person = personWeek.Person;
            var period = person.Period(personWeek.Week.StartDate) ?? person.Period(personWeek.Week.EndDate);
            if (period == null)
            {
                return TimeSpan.FromSeconds(0);
            }
            return  period.PersonContract.Contract.WorkTimeDirective.WeeklyRest;
        }
    }
}