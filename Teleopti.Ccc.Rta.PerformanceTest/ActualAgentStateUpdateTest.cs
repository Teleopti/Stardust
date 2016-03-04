using NUnit.Framework;
using Teleopti.Ccc.Rta.PerformanceTest.Code;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("ActualAgentStateUpdateTest")]
	[RtaPerformanceTest]
	public class ActualAgentStateUpdateTest
	{
		public DataCreator Data;
		public StatesSender States;
		public StatesArePersisted StatesArePersisted;

		[Test]
		public void MeasurePerformance()
		{
			Data.Create();

			States.Send();

			StatesArePersisted.WaitForAll();
		}		
	}
}