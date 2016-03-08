using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Rta.PerformanceTest.Code;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("BackgroundWorkTest")]
	[RtaPerformanceTest]
	public class BackgroundWorkTest
	{
		public StatesSender States;
		public HangfireUtilties Hangfire;

		[Test]
		public void MeasurePerformance()
		{
			States.Send();

			Hangfire.WaitForQueue();
		}
    }
}
