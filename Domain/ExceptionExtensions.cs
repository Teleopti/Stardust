using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Teleopti.Ccc.Domain
{
	public static class ExceptionExtensions
	{
		public static IEnumerable<Exception> AllExceptions(this Exception source)
		{
			yield return source;

			var inners = new Exception[] { };
			if (source.InnerException != null)
				inners = inners.Concat(new[] {source.InnerException}).ToArray();
			if (source is AggregateException)
				inners = inners.Concat((source as AggregateException).InnerExceptions).ToArray();

			foreach (var inner in inners)
			{
				foreach (var e in inner.AllExceptions())
				{
					yield return e;
				}
			}
		}

		public static bool ContainsSqlViolationOfPrimaryKey(this Exception e)
		{
			return e.AllExceptions().OfType<SqlException>().Any(x => x.Number == 2627);
		}

		public static bool ContainsSqlDeadlock(this Exception e)
		{
			return e.AllExceptions().OfType<SqlException>().Any(x => x.Number == 1205);
		}

		public static bool IsSqlDeadlock(this Exception source)
		{
			return (source as SqlException)?.Number == 1205;
		}
	}
}