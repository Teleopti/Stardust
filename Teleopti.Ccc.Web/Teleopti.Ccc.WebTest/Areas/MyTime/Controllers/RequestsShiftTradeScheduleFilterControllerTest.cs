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
		private IRequestsShiftTradeScheduleFilterViewModelFactory _modelFactory;

		public RequestsShiftTradeScheduleFilterControllerTest()
		{
			 _modelFactory = MockRepository.GenerateMock<IRequestsShiftTradeScheduleFilterViewModelFactory>();
		}

		[Test]
		public void ShouldGetHourTextsAndDayoffNames()
		{
			var model = new RequestsShiftTradeScheduleFilterViewModel();
			_modelFactory.Stub(x => x.ViewModel()).Return(model);

			var target = new RequestsShiftTradeScheduleFilterController(_modelFactory);

			var result = target.Get();
			result.Data.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void ShouldGetAllSiteText()
		{
			var text = "All Sites";
			_modelFactory.Stub(x => x.GetAllSitesText()).Return(text);

			var target = new RequestsShiftTradeScheduleFilterController(_modelFactory);

			var result = target.GetAllSitesText();
			result.Data.Should().Be.EqualTo(text);
		}
	}
}
