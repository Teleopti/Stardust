using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Sdk.LoadTest
{
	public static class Extensions
	{
		public static void ForEach<T>(this IEnumerable<T> instance, Action<T> action)
		{
			foreach (var element in instance)
				action(element);
		}
	}
}