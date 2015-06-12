using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.DBManager.Library
{
	public static class Extensions
	{
		public static string GetName(this DatabaseType type)
		{
			return Enum.GetName(typeof(DatabaseType), type);
		}
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