using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class RequestsShiftTradeScheduleFilterControllerTest
	{
		[Test]
		public void ShouldGetHourTextsAndDayoffNames()
		{
			var modelFactory = MockRepository.GenerateMock<IRequestsShiftTradeScheduleFilterViewModelFactory>();
			var model = new RequestsShiftTradeScheduleFilterViewModel();

			modelFactory.Stub(x => x.ViewModel())
							.Return(model);

			var target = new RequestsShiftTradeScheduleFilterController(modelFactory);

			var result = target.Get();
			result.Data.Should().Be.SameInstanceAs(model);
		}
	}
}
