using NUnit.Framework;
using Teleopti.Ccc.Rta.PerformanceTest.Code;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("SendStateTest")]
	[RtaPerformanceTest]
	public class StateSendTest
	{
		public DataCreator Data;
		public StatesSender States;

		[Test]
		public void MeasurePerformance()
		{
			States.Send();
		}
	}
}