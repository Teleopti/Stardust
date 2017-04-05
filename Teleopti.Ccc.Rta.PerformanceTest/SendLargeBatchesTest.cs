using NUnit.Framework;
using Teleopti.Ccc.Rta.PerformanceTest.Code;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[RtaPerformanceTest]
	[Explicit]
	public class SendLargeBatchesTest
	{
		public StatesSender States;
		public PerformanceTest PerformanceTest;

		[Test]
		public void MeasurePerformance()
		{
			using (PerformanceTest.Measure("1mKUHvBlk5wIk0LDZESO2prWvRuimhpjiWaSvoKk2gsE", "SendLargeBatchesTest"))
			{
				States.SendAllAsLargeBatches();
			}
		}
	}
}