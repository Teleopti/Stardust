using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class RtaStartupTaskTest
	{
		[Test]
		public void ShouldInitializeTheAdherenceAggregatorInitializor()
		{
			var initializor = MockRepository.GenerateMock<IRta>();
			var target = new RtaStartupTask(initializor);

			target.Execute(null);

			initializor.AssertWasCalled(x => x.Initialize());
		}
	}
}