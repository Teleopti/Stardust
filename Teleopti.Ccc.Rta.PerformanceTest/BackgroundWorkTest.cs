using System;
using System.Diagnostics;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Hangfire;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("BackgroundWorkTest")]
	[RtaPerformanceTest]
	public class BackgroundWorkTest
	{
		public Database Database;
		public RtaStates RtaStates;
		public HangfireUtilties Hangfire;

		[Test]
		public void MeasurePerformance()
		{
			var watch = Stopwatch.StartNew();
			Database.Setup();
			Console.WriteLine(watch.Elapsed);

			RtaStates.Send();
			Console.WriteLine(watch.Elapsed);

			Hangfire.WaitForQueue();
			Console.WriteLine(watch.Elapsed);
		}
    }
}
