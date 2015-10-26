using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class DayOffSettingsControllerTest
	{
		[Test]
		public void GetAllShouldReturnSameAsFetchModelService()
		{
			var model = new DayOffSettingsModel();
			var fetchModel = MockRepository.GenerateMock<IFetchDayOffSettingsModel>();
			fetchModel.Expect(x => x.FetchAll()).Return(model);
			var target = new DayOffSettingsController(fetchModel);
			target.GetAllDayOffSettings().OkContent<DayOffSettingsModel>()
				.Should().Be.SameInstanceAs(model);
		}
	}
}