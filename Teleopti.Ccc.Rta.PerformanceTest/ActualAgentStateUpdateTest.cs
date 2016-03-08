using NUnit.Framework;
using Teleopti.Ccc.Rta.PerformanceTest.Code;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("ActualAgentStateUpdateTest")]
	[RtaPerformanceTest]
	public class ActualAgentStateUpdateTest
	{
		public StatesSender States;
		public StatesArePersisted StatesArePersisted;

		[Test]
		public void MeasurePerformance()
		{
			States.Send();

			StatesArePersisted.WaitForAll();
		}		
	}
}