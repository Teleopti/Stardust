using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class PairMatrixService<T> : IPairMatrixService<T>
    {
        private readonly IPairDictionaryFactory<T> _factory;
        
        public PairMatrixService(IPairDictionaryFactory<T> factory)
        {
            _factory = factory;
        }

        public DependenciesPair<T> CreateDependencies(IEnumerable<Tuple<T, T>> pairList,IEnumerable<T> entriesForFirst)
        {
            var firstDependencies = new HashSet<T>();
            var secondDependencies = new HashSet<T>();
            var dictionaryPair = _factory.CreateDictionaries(pairList);
			
			Action<T> markSecond = null;
	        Action<T, ICollection<T>> markFirst = null;

	        markFirst = (first, targetCollection) =>
	        {
		        if (firstDependencies.Contains(first)) return;
		        firstDependencies.Add(first);
		        foreach (T second in targetCollection)
		        {
			        markSecond(second);
		        }
	        };

	        markSecond = second =>
	        {
		        if (secondDependencies.Contains(second)) return;
		        secondDependencies.Add(second);
		        foreach (T first in dictionaryPair.SecondDictionary[second])
		        {
			        markFirst(first, dictionaryPair.FirstDictionary[first]);
		        }
	        };

	        foreach (T orgFirst in entriesForFirst)
            {
	            ICollection<T> targetCollection;
	            if(dictionaryPair.FirstDictionary.TryGetValue(orgFirst, out targetCollection))
                {
                    markFirst(orgFirst, targetCollection);
                }
            }

			return new DependenciesPair<T>(firstDependencies,secondDependencies);
        }
    }
}
