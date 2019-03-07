using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Hangfire;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire
{
	[TestFixture]
	public class HangfireRetryDelaysTest
	{
		[Test]
		[Explicit]
		public void OutputDelayTimes()
		{
//			var field = typeof(AutomaticRetryAttribute)
//				.GetField("DefaultDelayInSecondsByAttemptFunc", BindingFlags.NonPublic | BindingFlags.Static);
			var method = typeof(AutomaticRetryAttribute)
				.GetMethod("SecondsToDelay", BindingFlags.NonPublic | BindingFlags.Static);
			
			Func<long, int> delayInSeconds = retry =>
			{
				Thread.Sleep(10); // for randomness
				return (int) method.Invoke(null, new object[] {retry});
			};
			
			var samples = Enumerable.Range(0, 10);
			var retries = Enumerable.Range(1, 20);
			var runs =
				from r in retries
				from s in samples
				let d = delayInSeconds(r)
				select new
				{
					retry = r, 
					sample = s, 
					delay = d
				};

			var results = runs.ToArray();
			
			var averages = results
				.GroupBy(x => x.retry)
				.Select(x => new
				{
					retry = x.Key,
					average = x.Average(y => y.delay),
					min = x.Min(y => y.delay),
					max = x.Max(y => y.delay)
				});

			var currentAverage = 0d;
			var currentMin = 0d;
			var currentMax = 0d;
			var running = averages.Select(x =>
			{
				currentAverage += x.average;
				currentMin += x.min;
				currentMax += x.max;
				return new
				{
					retry = x.retry,
					average = x.average,
					min = x.min,
					max = x.max,
					runningAverage = currentAverage,
					runningMin = currentMin,
					runningMax = currentMax
				};
			});
			
			running.ForEach(x =>
			{
				var min = TimeSpan.FromSeconds(x.min);
				var max = TimeSpan.FromSeconds(x.max);
				var average = TimeSpan.FromSeconds((int) x.average);
				var runningAverage = TimeSpan.FromSeconds((int) x.runningAverage);
				var runningMin = TimeSpan.FromSeconds((int) x.runningMin);
				var runningMax = TimeSpan.FromSeconds((int) x.runningMax);
				Console.WriteLine($"Retry {x.retry} {min}/{max}/{average} {runningMin}/{runningMax}/{runningAverage}");
			});
			
			results.ForEach(x =>
			{
				Console.WriteLine($"Retry {x.retry} {TimeSpan.FromSeconds(x.delay)}");
			});
			
		}
	}
}