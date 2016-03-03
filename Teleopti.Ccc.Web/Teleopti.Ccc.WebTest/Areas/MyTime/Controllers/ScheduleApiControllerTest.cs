using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class ScheduleApiControllerTest
	{
		[Test]
		public void ShouldReturnJsonScheduleForCurrentWeek()
		{
			var viewModelFactory = MockRepository.GenerateMock<IScheduleViewModelFactory>();
			var now = MockRepository.GenerateMock<INow>();
			now.Stub(x => x.UtcDateTime()).Return(new DateTime(2012, 8, 1));
			var weekScheduleViewModel = new WeekScheduleViewModel();
			viewModelFactory.Stub(x => x.CreateWeekViewModel(now.LocalDateOnly())).Return(weekScheduleViewModel);

			using (var target = new ScheduleApiController(viewModelFactory, now))
			{
				var result = target.FetchData(null);
				result.Should().Be.SameInstanceAs(weekScheduleViewModel);
			}
		}

		[Test]
		public void ShouldReturnJsonScheduleForCurrentMonth()
		{
			var viewModelFactory = MockRepository.GenerateMock<IScheduleViewModelFactory>();
			var now = MockRepository.GenerateMock<INow>();
			now.Stub(x => x.UtcDateTime()).Return(new DateTime(2012, 8, 1));
			var monthScheduleViewModel = new MonthScheduleViewModel();
			viewModelFactory.Stub(x => x.CreateMonthViewModel(now.LocalDateOnly())).Return(monthScheduleViewModel);

			using (var target = new ScheduleApiController(viewModelFactory, now))
			{
				var result = target.FetchMonthData(null);
				result.Should().Be.SameInstanceAs(monthScheduleViewModel);
			}
		}
		[Test]
		public void ShouldViewWeekScheduleForGivenDate()
		{
			var date = new DateOnly(2011, 01, 01);
			var viewModelFactory = MockRepository.GenerateMock<IScheduleViewModelFactory>();
			var now = MockRepository.GenerateMock<INow>();
			var weekScheduleViewModel = new WeekScheduleViewModel();
			viewModelFactory.Stub(x => x.CreateWeekViewModel(date)).Return(weekScheduleViewModel);
			using (var target = new ScheduleApiController(viewModelFactory, now))
			{
				var result = target.FetchData(date);
				result.Should().Be.SameInstanceAs(weekScheduleViewModel);
			}
		}

		[Test]
		public void ShouldViewMonthScheduleForGivenDate()
		{
			var date = new DateOnly(2011, 01, 01);
			var viewModelFactory = MockRepository.GenerateMock<IScheduleViewModelFactory>();
			var now = MockRepository.GenerateMock<INow>();
			var monthScheduleViewModel = new MonthScheduleViewModel();
			viewModelFactory.Stub(x => x.CreateMonthViewModel(date)).Return(monthScheduleViewModel);
			using (var target = new ScheduleApiController(viewModelFactory, now))
			{
				var result = target.FetchMonthData(date);
				result.Should().Be.SameInstanceAs(monthScheduleViewModel);
			}
		}
	}
}