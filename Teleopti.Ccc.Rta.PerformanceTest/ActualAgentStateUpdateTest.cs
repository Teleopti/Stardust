using NUnit.Framework;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("ActualAgentStateUpdateTest")]
	[RtaPerformanceTest]
	public class ActualAgentStateUpdateTest
	{
		public Database Database;
		public RtaStates RtaStates;
		public StatesArePersisted StatesArePersisted;

		[Test]
		public void MeasurePerformance()
		{
			Database.Setup();

			RtaStates.Send();

			StatesArePersisted.WaitForAll();
		}		
	}
}