using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Common
{
	/// <summary>
	/// Aggregates a list according to the AreAttached method.
	/// From the one dimensional list it creates a list of sublists 
	/// where members in the sublist are attached/aggregated to each other.
	/// </summary>
	/// <typeparam name="T">type of the items in the input list</typeparam>
	public class ListAggregator<T>
	{
		public delegate bool AreAttached(T object1, T object2);

		public IList<IList<T>> Aggregate(IEnumerable<T> listToAggregate, AreAttached areAttached)
		{
			IList<IList<T>> result = new List<IList<T>>();
			IList<T> subItems = new List<T>();
			T prevItem = default(T);

			bool firstItemFlag = true;

			foreach (T item in listToAggregate)
			{
				if (!firstItemFlag 
					&& areAttached(prevItem, item))
				{
					subItems.Add(item);
					prevItem = item;
					continue;
				}
				subItems = new List<T>();
				result.Add(subItems);
				subItems.Add(item);
				prevItem = item;
				firstItemFlag = false;
			}
			return result;
		}

	}
}
