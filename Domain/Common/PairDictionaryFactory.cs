
using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class PairDictionaryFactory<T> : IPairDictionaryFactory<T>
    {
        public CollectionDictionaryPair<T> CreateDictionaries(IEnumerable<Tuple<T, T>> pairList)
        {
	        var dictionaryPair = new CollectionDictionaryPair<T>();
            foreach (var pair in pairList)
            {
                addFirstValue(dictionaryPair,pair);
                addSecondValue(dictionaryPair,pair);
            }
	        return dictionaryPair;
        }

	    private void addFirstValue(CollectionDictionaryPair<T> dictionaryPair, Tuple<T, T> pair)
	    {
		    ICollection<T> list;
		    if (!dictionaryPair.FirstDictionary.TryGetValue(pair.Item1, out list))
		    {
			    list = new List<T>();
			    dictionaryPair.FirstDictionary.Add(pair.Item1, list);
		    }
		    list.Add(pair.Item2);
	    }

	    private void addSecondValue(CollectionDictionaryPair<T> dictionaryPair, Tuple<T, T> pair)
        {
		    ICollection<T> list;
		    if (!dictionaryPair.SecondDictionary.TryGetValue(pair.Item2, out list))
		    {
				list = new List<T>();
			    dictionaryPair.SecondDictionary.Add(pair.Item2, list);
		    }
            list.Add(pair.Item1);
        }
    }
}
