using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IRankedPersonBasedOnStartDate
    {
        IEnumerable<IPerson> GetRankedPerson(IEnumerable<IPerson> personList);
    }

    public class RankedPersonBasedOnStartDate : IRankedPersonBasedOnStartDate
    {
        private readonly IPersonStartDateFromPersonPeriod _personStartDateFromPersonPeriod;

        public RankedPersonBasedOnStartDate(IPersonStartDateFromPersonPeriod personStartDateFromPersonPeriod)
        {
            _personStartDateFromPersonPeriod = personStartDateFromPersonPeriod;
        }

        public IEnumerable<IPerson> GetRankedPerson(IEnumerable<IPerson> personList)
        {
            var personDictionary = new Dictionary<IPerson, DateOnly>();
            foreach (var person in personList)
            {
                personDictionary.Add(person, _personStartDateFromPersonPeriod.GetPersonStartDate(person ));
            }

            return personDictionary.OrderBy(s => s.Value).Select(value => value.Key).ToList();
        }

    }
}