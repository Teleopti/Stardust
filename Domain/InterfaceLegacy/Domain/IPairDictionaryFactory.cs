using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Creates two dictionaries from a list of Pairs.
    /// Eg (int)
    /// 1, 2
    /// 1, 3
    /// 4, 6
    /// 5, 7
    /// => 
    /// (key, value) FirsDic
    /// 1, (2,3)
    /// 4, (6)
    /// 5, (7)
    /// (key, value) SecondDic
    /// 2, (1)
    /// 3, (1)
    /// 4, (6)
    /// 7, (5)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-12-08
    /// </remarks>
    public interface IPairDictionaryFactory<T>
    {
        /// <summary>
        /// Creates the dictionaries.
        /// </summary>
        /// <param name="pairList">The pair list.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-12-08
        /// </remarks>
        CollectionDictionaryPair<T> CreateDictionaries(IEnumerable<Tuple<T, T>> pairList);
    }

	public class CollectionDictionaryPair<T>
	{
		public CollectionDictionaryPair()
		{
			FirstDictionary = new Dictionary<T, ICollection<T>>();
			SecondDictionary = new Dictionary<T, ICollection<T>>();
		}

		public IDictionary<T, ICollection<T>> FirstDictionary { get; private set; }
		public IDictionary<T, ICollection<T>> SecondDictionary { get; private set; }
	}
}