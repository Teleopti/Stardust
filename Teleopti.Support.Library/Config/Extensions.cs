using System;
using System.Collections.Generic;

namespace Teleopti.Support.Library.Config
{
	public static class Extensions
	{
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> act)
		{
			foreach (T item in source)
			{
				act(item);
			}
			return source;
		}
	}
}