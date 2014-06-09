using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.PeformanceTool;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.PerformanceTool.Controllers
{
	public class RtaControllerTest
	{
		[Test]
		public void ShouldCallPersonCreator()
		{
			var rtaTestPersonCreator = MockRepository.GenerateMock<ITestPersonCreator>();
			var target = new RtaController(rtaTestPersonCreator);

			target.CreatePersons(1);

			rtaTestPersonCreator.AssertWasCalled(x => x.CreatePersons(1));
		}

		[Test]
		public void ShouldCleanupData()
		{
			var rtaTestPersonCreator = MockRepository.GenerateMock<ITestPersonCreator>();
			var target = new RtaController(rtaTestPersonCreator);

			target.RemoveCreatedData();

			rtaTestPersonCreator.AssertWasCalled(x => x.RemoveCreatedPersons());
		}
	}
}
