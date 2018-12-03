using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IRankedPersonBasedOnStartDate
    {
		int? GetRankForPerson(IEnumerable<IPerson> personsToCompareWith, IPerson person);
		IDictionary<IPerson, int> GetRankedPersonDictionary(IEnumerable<IPerson> personsToRank);
    }

    public class RankedPersonBasedOnStartDate : IRankedPersonBasedOnStartDate
    {
        private readonly IPersonStartDateFromPersonPeriod _personStartDateFromPersonPeriod;

        public RankedPersonBasedOnStartDate(IPersonStartDateFromPersonPeriod personStartDateFromPersonPeriod)
        {
            _personStartDateFromPersonPeriod = personStartDateFromPersonPeriod;
        }

        public int? GetRankForPerson(IEnumerable<IPerson> personsToCompareWith, IPerson person)
        {
            var personDictionary = getDictionary(personsToCompareWith);
            var rankedPerson = getPersonValueDictionary(personDictionary);
	        int value;
	        if (rankedPerson.TryGetValue(person, out value))
                return value;
            return null;
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
			//var sortedByValue = dicToEvaluate.OrderBy(s => s.Value).Select(value => value.Key).ToList();
			var sortedByValueDic = dicToEvaluate.OrderBy(s => s.Value).ToDictionary(s => s.Key, s => s.Value);

			

			IDictionary<IPerson, int> ret = new Dictionary<IPerson, int>();

			var rank = 0;
			var currentItem = sortedByValueDic.FirstOrDefault();
			foreach (var keyValuePair in sortedByValueDic)
			{
				if (!currentItem.Value.Equals(keyValuePair.Value)) ++rank;
				ret.Add(keyValuePair.Key, rank);
				
				currentItem = keyValuePair;
			}


			//for (int order = 0; order < sortedByValue.Count; order++)
			//{
			//	ret.Add(sortedByValue[order], order);
			//}

			return ret;
		}
    }
}