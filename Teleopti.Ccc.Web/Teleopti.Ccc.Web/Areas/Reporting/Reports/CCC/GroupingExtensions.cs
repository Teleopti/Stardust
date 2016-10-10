using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Web.Areas.Reporting.Reports.CCC
{
	public static class GroupingExtensions
	{
		public static void ForEach<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> groups, Action<TKey, IGrouping<TKey, TElement>> action)
		{
			foreach (var group in groups)
			{
				action.Invoke(group.Key, group);
			}
		}

		public static void ForEach<TKey, TElement>(this IGrouping<TKey, TElement> grouping, Action<TElement> action)
		{
			foreach (var group in grouping)
			{
				action.Invoke(group);
			}
		}

	}
}