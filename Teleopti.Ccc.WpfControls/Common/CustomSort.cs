using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Teleopti.Ccc.WpfControls.Common
{
	public static class CustomSort
	{
		public static IList<T> SortViewSource<T>(IQueryable<T> itemSource, IList<SortDescription> sortDescriptions)
		{
			if (!itemSource.Any())
				return itemSource.ToList();

			var result = (IOrderedQueryable<T>)itemSource.AsQueryable();
			
			foreach (var sort in sortDescriptions)
			{
				var sort1 = sort;
				switch (sort.Direction)
				{
					case ListSortDirection.Ascending:
						result = result.OrderBy(t => getPropertyValue(t, sort1.PropertyName));
						break;
					case ListSortDirection.Descending:
						result = result.OrderByDescending(t => getPropertyValue(t, sort1.PropertyName));
						break;
				}
			}
			return result.ToList();
		}

		private static object getPropertyValue(object obj, string property)
		{
			return obj.GetType().GetProperty(property).GetValue(obj, null);
		}
	}
}
