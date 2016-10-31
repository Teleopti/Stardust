using System.Collections.Generic;
using System.ServiceModel;

namespace Teleopti.Ccc.Sdk.Logic
{
	public static class CollectionExtensions
	{
		public static void VerifyCountLessThan<T>(this ICollection<T> items, int limit, string messageFormat)
		{
			if (items.Count > limit)
			{
				throw new FaultException(string.Format(System.Globalization.CultureInfo.InvariantCulture, messageFormat, items.Count));
			}
		}
	}
}