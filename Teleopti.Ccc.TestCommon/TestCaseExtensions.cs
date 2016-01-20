using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Teleopti.Ccc.TestCommon
{
	public static class TestCaseExtensions
	{
		public static IEnumerable TestCases(this IEnumerable<IEnumerable<object>> enumerables)
		{
			return from p in enumerables
				let name = (
					from pe in p
					select pe.GetType().Name)
					.Aggregate((current, next) => current + ", " + next)
				select new TestCaseData(p).SetName(name);
		}
	}
}