using Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Test;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class DatabaseTestAttribute : InfrastructureTestAttribute
	{
		protected override void BeforeTest()
		{
			InfrastructureTestStuff.BeforeWithLogon();
			base.BeforeTest();
		}

		protected override void AfterTest()
		{
			base.AfterTest();
			InfrastructureTestStuff.AfterWithLogon();
		}
	}
}