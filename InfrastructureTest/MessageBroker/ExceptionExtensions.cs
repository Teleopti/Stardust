using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	public static class ExceptionExtensions
	{
		public static IEnumerable<Exception> AllExceptions(this Exception source)
		{
			yield return source;

			var inners = new Exception[] {};
			if (source.InnerException != null)
				inners = inners.Concat(new[] { source.InnerException}).ToArray();
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
	}
}