using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.ViewModelFactory
{
	[TestFixture]
	public class StudentAvailabilityViewModelFactoryTest
	{
		[Test]
		public void ShoudCreateViewModelByMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new StudentAvailabilityViewModelFactory(mapper, null, null);
			var date = DateOnly.Today;
			var viewModel = new StudentAvailabilityViewModel();

			mapper.Stub(x => x.Map<DateOnly, StudentAvailabilityViewModel>(date)).Return(viewModel);

			var result = target.CreateViewModel(date);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldCreateDayViewModel()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var studentAvailabilityProvider = MockRepository.GenerateMock<IStudentAvailabilityProvider>();
			var target = new StudentAvailabilityViewModelFactory(mapper, studentAvailabilityProvider, null);
			var date = DateOnly.Today;
			var studentAvailabilityDay = new StudentAvailabilityDay(new Person(), date, new List<IStudentAvailabilityRestriction>());
			var viewModel = new StudentAvailabilityDayViewModel();

			studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityDayForDate(date)).Return(studentAvailabilityDay);
			mapper.Stub(x => x.Map<IStudentAvailabilityDay, StudentAvailabilityDayViewModel>(studentAvailabilityDay)).Return(viewModel);

			var result = target.CreateDayViewModel(date);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldCreateStudentAvailabilityAndSchedulesViewModels()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var target = new StudentAvailabilityViewModelFactory(mapper, null, scheduleProvider);
			var scheduleDays = new IScheduleDay[] { };
			var models = new StudentAvailabilityAndScheduleDayViewModel[] {};

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1))))
				.Return(scheduleDays);
			mapper.Stub(x => x.Map<IEnumerable<IScheduleDay>, IEnumerable<StudentAvailabilityAndScheduleDayViewModel>>(scheduleDays))
				.Return(models);

			var result = target.CreateStudentAvailabilityAndSchedulesViewModels(DateOnly.Today, DateOnly.Today.AddDays(1));

			result.Should().Be.SameInstanceAs(models);
		}

		[Test]
		public void ShouldCreateFeedbackDayViewModelByMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new StudentAvailabilityViewModelFactory(mapper, null, null);
			var viewModel = new StudentAvailabilityDayFeedbackViewModel();

			mapper.Stub(x => x.Map<DateOnly, StudentAvailabilityDayFeedbackViewModel>(DateOnly.Today))
				.Return(viewModel);

			var result = target.CreateDayFeedbackViewModel(DateOnly.Today);
			result.Should().Be.SameInstanceAs(viewModel);
		}
	}
}