using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class PairMatrixService<T> : IPairMatrixService<T>
    {
        private readonly IPairDictionaryFactory<T> _factory;
        private ICollection<T> _firstDependencies;
        private ICollection<T> _secondDependencies;

        public PairMatrixService(IPairDictionaryFactory<T> factory)
        {
            _factory = factory;
        }

        public IEnumerable<T> FirstDependencies
        {
            get { return _firstDependencies; }
        }

        public IEnumerable<T> SecondDependencies
        {
            get { return _secondDependencies; }
        }

        public void CreateDependencies(IEnumerable<Tuple<T, T>> pairList,IEnumerable<T> entriesForFirst)
        {
            _firstDependencies = new HashSet<T>();
            _secondDependencies = new HashSet<T>();
            _factory.CreateDictionaries(pairList);
            foreach (T orgFirst in entriesForFirst)
            {
                if(_factory.FirstDictionary.ContainsKey(orgFirst))
                {
                    markFirst(orgFirst);                    
                }
            }
        }

        private void markFirst(T first)
        {
            if (_firstDependencies.Contains(first)) return;
            _firstDependencies.Add(first);
            foreach (T second in _factory.FirstDictionary[first])
            {
                markSecond(second);
            }
        }

        private void markSecond(T second)
        {
            if (_secondDependencies.Contains(second)) return;
            _secondDependencies.Add(second);
            foreach (T first in _factory.SecondDictionary[second])
            {
                markFirst(first);
            }
        }
    }
}
