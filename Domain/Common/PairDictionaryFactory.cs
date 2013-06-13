
using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class PairDictionaryFactory<T> : IPairDictionaryFactory<T>
    {
        public IDictionary<T, ICollection<T>> FirstDictionary { get; private set; }

        public IDictionary<T, ICollection<T>> SecondDictionary { get; private set; }

        public void CreateDictionaries(IEnumerable<Tuple<T, T>> pairList)
        {
            FirstDictionary = new Dictionary<T, ICollection<T>>();
            SecondDictionary = new Dictionary<T, ICollection<T>>();
            foreach (var pair in pairList)
            {
                addFirstValue(pair);
                AddSecondValue(pair);
            }
        }

				private void addFirstValue(Tuple<T, T> pair)
        {
            if (!FirstDictionary.ContainsKey(pair.Item1))
                FirstDictionary[pair.Item1] = new List<T>();
            FirstDictionary[pair.Item1].Add(pair.Item2);
        }

				private void AddSecondValue(Tuple<T, T> pair)
        {
            if (!SecondDictionary.ContainsKey(pair.Item2))
                SecondDictionary[pair.Item2] = new List<T>();
            SecondDictionary[pair.Item2].Add(pair.Item1);
        }
    }
}
