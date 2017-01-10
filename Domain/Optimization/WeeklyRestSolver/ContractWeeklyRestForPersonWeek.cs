using System;
using System.Linq;
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
            var period = person.PersonPeriods(personWeek.Week);
            if (!period.Any())
            {
                return TimeSpan.Zero;
            }
            return  period.First().PersonContract.Contract.WorkTimeDirective.WeeklyRest;
        }
    }
}