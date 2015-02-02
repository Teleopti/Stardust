using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class RequestsShiftTradeBulletinBoardControllerTest
	{
		private ITimeFilterHelper _timeFilterHelper;
		[SetUp]
		public void Setup()
		{
			_timeFilterHelper = MockRepository.GenerateMock<ITimeFilterHelper>();
		}

		[Test]
		public void ShouldGetAllBulletinSchedules()
		{
			var modelFactory = MockRepository.GenerateMock<IRequestsShiftTradebulletinViewModelFactory>();
			var model = new ShiftTradeScheduleViewModel();

			modelFactory.Stub(x => x.CreateShiftTradeBulletinViewModel(Arg<ShiftTradeScheduleViewModelDataForAllTeams>.Is.Anything))
							.Return(model);

			var target = new RequestsShiftTradeBulletinBoardController(modelFactory, _timeFilterHelper);

			var result = target.BulletinSchedules(DateOnly.Today, Guid.NewGuid().ToString(), new Paging());
			result.Data.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void ShouldGetAllBulletinSchedulesWithFilters()
		{
			const bool isDayOff = true;

			var modelFactory = MockRepository.GenerateMock<IRequestsShiftTradebulletinViewModelFactory>();
			var model = new ShiftTradeScheduleViewModel();

			modelFactory.Stub(
				x => x.CreateShiftTradeBulletinViewModel(Arg<ShiftTradeScheduleViewModelDataForAllTeams>.Is.Anything))
				.Return(model);

			var target = new RequestsShiftTradeBulletinBoardController(modelFactory, _timeFilterHelper);

			var result = target.BulletinSchedulesWithTimeFilter(DateOnly.Today, Guid.NewGuid().ToString(), "", "", isDayOff,
				false, new Paging());
			result.Data.Should().Be.SameInstanceAs(model);
		}
	}
}
