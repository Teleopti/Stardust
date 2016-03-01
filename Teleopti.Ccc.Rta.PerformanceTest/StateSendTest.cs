using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("SendStateTest")]
	[RtaPerformanceTest]
	public class StateSendTest
	{
		public Database Database;
		public RtaStates RtaStates;

		[Test]
		public void MeasurePerformance()
		{
			var watch = Stopwatch.StartNew();
			Database.Setup();
			Console.WriteLine(watch.Elapsed);

			RtaStates.Send();
			Console.WriteLine(watch.Elapsed);
		}
	}
}