using NUnit.Framework;
using Teleopti.Ccc.Rta.PerformanceTest.Code;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("SendBatchTest")]
	[RtaPerformanceTest]
	public class BatchSendTest
	{
		public StatesSender States;

		[Test]
		public void MeasurePerformance()
		{
			States.SendBatches();
		}
	}
}