using System;
using System.Diagnostics;
using NUnit.Framework;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[RtaPerformanceTest]
	[Explicit]
	public class SendLargeBatchesTest
	{
		public StatesSender States;
		public Http Http;

		[Test]
		public void MeasurePerformance()
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			States.SendAllAsLargeBatches();

			stopwatch.Stop();

			Http.PostJson("https://sheetsu.com/apis/v1.0/8eb000dab2d0", new
			{
				version = typeof(SendLargeBatchesTest).Assembly.GetName().Version.ToString(),
				agent = Environment.MachineName,
				duration = stopwatch.Elapsed.ToString(),
				seconds = stopwatch.Elapsed.TotalSeconds
			});
		}
	}
}