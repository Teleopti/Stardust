using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class RtaStartupTaskTest
	{
		[Test]
		public void ShouldInitializeTheAdherenceAggregatorInitializor()
		{
			var initializor = MockRepository.GenerateMock<IAdherenceAggregatorInitializor>();
			var target = new RtaStartupTask(initializor);

			target.Execute();

			initializor.AssertWasCalled(x => x.Initialize());
		}
	}
}