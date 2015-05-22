using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System.Collections.Generic;
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
			var target = new StudentAvailabilityViewModelFactory(mapper, null);
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
			var target = new StudentAvailabilityViewModelFactory(mapper, studentAvailabilityProvider);
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
			var studentAvailabilityProvider = MockRepository.GenerateMock<IStudentAvailabilityProvider>();
			var target = new StudentAvailabilityViewModelFactory(mapper, studentAvailabilityProvider);
			var date = DateOnly.Today;
			var studentAvailabilityDays = new List<StudentAvailabilityDay>
			{
				new StudentAvailabilityDay(new Person(), date, new List<IStudentAvailabilityRestriction>())
			};

			var models = new StudentAvailabilityDayViewModel[] { };

			var period = new DateOnlyPeriod(date, date.AddDays(1));
			studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityDayForPeriod(period))
				.Return(studentAvailabilityDays);

			mapper.Stub(
				x => x.Map<IEnumerable<IStudentAvailabilityDay>, IEnumerable<StudentAvailabilityDayViewModel>>(studentAvailabilityDays))
				.Return(models);

			var result = target.CreateStudentAvailabilityAndSchedulesViewModels(period.StartDate, period.EndDate);

			result.Should().Be.SameInstanceAs(models);
		}

		[Test]
		public void ShouldCreateFeedbackDayViewModelByMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new StudentAvailabilityViewModelFactory(mapper, null);
			var viewModel = new StudentAvailabilityDayFeedbackViewModel();

			mapper.Stub(x => x.Map<DateOnly, StudentAvailabilityDayFeedbackViewModel>(DateOnly.Today))
				.Return(viewModel);

			var result = target.CreateDayFeedbackViewModel(DateOnly.Today);
			result.Should().Be.SameInstanceAs(viewModel);
		}
	}
}