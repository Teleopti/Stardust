using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Web.Core
{
	public static class CoreExtensions
	{
		public static TSource SingleOrDefaultNullSafe<TSource>(this IEnumerable<TSource> collection)
		{
			return collection == null ? default(TSource) : collection.SingleOrDefault();
		}
	}
}
