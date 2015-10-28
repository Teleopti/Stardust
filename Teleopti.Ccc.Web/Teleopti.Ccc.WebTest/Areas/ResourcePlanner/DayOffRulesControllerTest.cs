using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class DayOffRulesControllerTest
	{
		[Test]
		public void GetAllShouldReturnSameAsFetchModelService()
		{
			var model = new DayOffRulesModel();
			var fetchModel = MockRepository.GenerateMock<IFetchDayOffRulesModel>();
			fetchModel.Expect(x => x.FetchDefaultRules()).Return(model);
			var target = new DayOffRulesController(fetchModel);
			target.GetDefaultSettings().OkContent<DayOffRulesModel>()
				.Should().Be.SameInstanceAs(model);
		}
	}
}