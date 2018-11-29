using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    public interface IPersonFinderService
    {
		IPerson[] Find(string findText, DateOnlyPeriod dateOnlyPeriod);
    }

    public class PersonFinderService : IPersonFinderService
	{
	    private readonly IPersonIndexBuilder _searchIndexBuilder;

	    public PersonFinderService(IPersonIndexBuilder searchIndexBuilder)
        {
	        _searchIndexBuilder = searchIndexBuilder;
        }

		public IPerson[] Find(string findText, DateOnlyPeriod dateOnlyPeriod)
        {
            if(string.IsNullOrEmpty(findText)) return new IPerson[]{};

            var splitted = findText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
			var index = _searchIndexBuilder.BuildIndex(dateOnlyPeriod);

			return search(index, splitted);
        }

        private static IPerson[] search(IDictionary<string, IList<IPerson>> workingSet, IList<string> stringsToSearchFor)
        {
            if (stringsToSearchFor.Count > 0)
            {
	            var foundPeople = workingSet.SelectMany(x => x.Value.Select(v => new {v,x.Key}))
					.GroupBy(g => g.v,h => h.Key)
					.Where(
					k => stringsToSearchFor.All(s => k.Any(y => y.IndexOf(s, StringComparison.CurrentCultureIgnoreCase) > -1)))
					.Select(p => p.Key)
					.ToArray();
	            return foundPeople;
            }
            return workingSet.Values.SelectMany(v => v).Distinct().ToArray();
        }
    }
}