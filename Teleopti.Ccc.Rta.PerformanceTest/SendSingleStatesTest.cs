using NUnit.Framework;
using Teleopti.Ccc.Rta.PerformanceTest.Code;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[RtaPerformanceTest]
	public class SendSingleStatesTest
	{
		public StatesSender States;
		public TestCommon.PerformanceTest.PerformanceTest PerformanceTest;

		[Test]
		public void MeasurePerformance()
		{
			PerformanceTest.Measure("1mKUHvBlk5wIk0LDZESO2prWvRuimhpjiWaSvoKk2gsE", "SendSingleStatesTest", () =>
			{
				States.SendAllAsSingles();
			});
		}
	}
}