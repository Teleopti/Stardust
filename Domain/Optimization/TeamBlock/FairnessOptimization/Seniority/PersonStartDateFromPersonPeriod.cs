using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IPersonStartDateFromPersonPeriod
    {
        DateOnly GetPersonStartDate(IPerson person);
    }

    public class PersonStartDateFromPersonPeriod : IPersonStartDateFromPersonPeriod
    {
        public DateOnly GetPersonStartDate(IPerson person)
        {
            var startDate = DateOnly.MaxValue;
            foreach (var personPeriod in person.PersonPeriodCollection)
            {
                var startDateOfPerson = personPeriod.StartDate;
                if (startDateOfPerson < startDate)
                    startDate = startDateOfPerson;
            }
            return startDate;
        }
    }
}
