using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
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
			var modelFactory = MockRepository.GenerateMock<IRequestsShiftTradeBulletinViewModelFactory>();
			var model = new ShiftTradeScheduleViewModel();

			modelFactory.Stub(x => x.CreateShiftTradeBulletinViewModel(Arg<ShiftTradeScheduleViewModelData>.Is.Anything))
							.Return(model);

			var target = new RequestsShiftTradeBulletinBoardController(modelFactory, _timeFilterHelper, MockRepository.GenerateMock<IToggleManager>());

			var result = target.BulletinSchedules(DateOnly.Today, Guid.NewGuid().ToString(), new Paging());
			result.Data.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void ShouldGetAllBulletinSchedulesWithFilters()
		{
			var modelFactory = MockRepository.GenerateMock<IRequestsShiftTradeBulletinViewModelFactory>();
			var model = new ShiftTradeScheduleViewModel();

			modelFactory.Stub(
				x => x.CreateShiftTradeBulletinViewModel(Arg<ShiftTradeScheduleViewModelData>.Is.Anything))
				.Return(model);

			var target = new RequestsShiftTradeBulletinBoardController(modelFactory, _timeFilterHelper, null);

			var result = target.BulletinSchedulesWithTimeFilter(DateOnly.Today, new ScheduleFilter { TeamIds = Guid.NewGuid().ToString() }, new Paging());
			result.Data.Should().Be.SameInstanceAs(model);
		}
	}
}
