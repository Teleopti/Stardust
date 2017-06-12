using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Helper
{
	public static class EnumExtensions
	{
		public static IEnumerable<T> GetValues<T>(params T[] excepteds) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}

			var values = Enum.GetValues(typeof(T)).Cast<T>();
			return values.Except(excepteds);
		}
	}
}
