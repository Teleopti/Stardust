using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IRankedPersonBasedOnStartDate
    {
        IEnumerable<IPerson> GetRankedPersonList(IEnumerable<IPerson> personsToRank);
		int GetRankForPerson(IEnumerable<IPerson> personsToCompareWith, IPerson person);
		IDictionary<IPerson, int> GetRankedPersonDictionary(IEnumerable<IPerson> personsToRank);
    }

    public class RankedPersonBasedOnStartDate : IRankedPersonBasedOnStartDate
    {
        private readonly IPersonStartDateFromPersonPeriod _personStartDateFromPersonPeriod;

        public RankedPersonBasedOnStartDate(IPersonStartDateFromPersonPeriod personStartDateFromPersonPeriod)
        {
            _personStartDateFromPersonPeriod = personStartDateFromPersonPeriod;
        }

		public IEnumerable<IPerson> GetRankedPersonList(IEnumerable<IPerson> personsToRank)
		{
			var personDictionary = getDictionary(personsToRank);
            return personDictionary.OrderBy(s => s.Value).Select(value => value.Key).ToList();
        }

		public int GetRankForPerson(IEnumerable<IPerson> personsToCompareWith, IPerson person)
		{
			var personDictionary = getDictionary(personsToCompareWith);
			return getPersonValueDictionary(personDictionary)[person];
		}

		public IDictionary<IPerson, int> GetRankedPersonDictionary(IEnumerable<IPerson> personsToRank)
	    {
			var personDictionary = getDictionary(personsToRank);
			return getPersonValueDictionary(personDictionary);
	    }

		private Dictionary<IPerson, DateOnly> getDictionary(IEnumerable<IPerson> personsToRank)
		{
			var personDictionary = new Dictionary<IPerson, DateOnly>();
			foreach (var person in personsToRank)
			{
				personDictionary.Add(person, _personStartDateFromPersonPeriod.GetPersonStartDate(person));
			}

			return personDictionary;
		}

		private IDictionary<IPerson, int> getPersonValueDictionary(Dictionary<IPerson, DateOnly> dicToEvaluate)
		{
			var sortedByValue = dicToEvaluate.OrderBy(s => s.Value).Select(value => value.Key).ToList();
			IDictionary<IPerson, int> ret = new Dictionary<IPerson, int>();
			for (int order = 0; order < sortedByValue.Count; order++)
			{
				ret.Add(sortedByValue[order], order);
			}

			return ret;
		}
    }
}