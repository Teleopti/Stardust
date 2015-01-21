using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon
{
	public static class Extensions
	{
		public static void Times(this int times, Action action)
		{
			Enumerable.Range(0, times).ForEach(i => action());
		}

		public static void Times(this int times, Action<int> action)
		{
			Enumerable.Range(0, times).ForEach(action);
		}
	}
}
