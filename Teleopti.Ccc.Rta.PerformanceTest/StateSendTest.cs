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
			Database.Setup();

			RtaStates.Send();
		}
	}
}