
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class PairDictionaryFactory<T> : IPairDictionaryFactory<T>
    {
        public IDictionary<T, ICollection<T>> FirstDictionary { get; private set; }

        public IDictionary<T, ICollection<T>> SecondDictionary { get; private set; }

        public void CreateDictionaries(IEnumerable<IPair<T>> pairList)
        {
            FirstDictionary = new Dictionary<T, ICollection<T>>();
            SecondDictionary = new Dictionary<T, ICollection<T>>();
            foreach (Pair<T> pair in pairList)
            {
                addFirstValue(pair);
                AddSecondValue(pair);
            }
        }

        private void addFirstValue(IPair<T> pair)
        {
            if (!FirstDictionary.ContainsKey(pair.First))
                FirstDictionary[pair.First] = new List<T>();
            FirstDictionary[pair.First].Add(pair.Second);
        }

        private void AddSecondValue(IPair<T> pair)
        {
            if (!SecondDictionary.ContainsKey(pair.Second))
                SecondDictionary[pair.Second] = new List<T>();
            SecondDictionary[pair.Second].Add(pair.First);            
        }
    }
}
