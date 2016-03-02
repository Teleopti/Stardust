using NUnit.Framework;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("BackgroundWorkTest")]
	[RtaPerformanceTest]
	public class BackgroundWorkTest
	{
		public Database Database;
		public RtaStates RtaStates;
		public HangFireUtilitiesWrapperForLogTime Hangfire;

		[Test]
		public void MeasurePerformance()
		{
			Database.Setup();

			RtaStates.Send();

			Hangfire.WaitForQueue();
		}
    }
}
