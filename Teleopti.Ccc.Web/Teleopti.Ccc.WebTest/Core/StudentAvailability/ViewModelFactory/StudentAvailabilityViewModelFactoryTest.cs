using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.StudentAvailability.ViewModelFactory
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
			var studentAvailability = new StudentAvailabilityRestriction();
			var viewModel = new StudentAvailabilityDayViewModel();

			studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityForDate(DateOnly.Today)).Return(studentAvailability);
			mapper.Stub(x => x.Map<IStudentAvailabilityRestriction, StudentAvailabilityDayViewModel>(studentAvailability)).Return(viewModel);

			var result = target.CreateDayViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldCreateEmptyDayViewModelWhenNoStudentAvailabilityFound()
		{
			var studentAvailabilityProvider = MockRepository.GenerateMock<IStudentAvailabilityProvider>();
			var target = new StudentAvailabilityViewModelFactory(null, studentAvailabilityProvider);

			studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityForDate(DateOnly.Today)).Return(null);

			var result = target.CreateDayViewModel(DateOnly.Today);

			result.NextDay.Should().Be.False();
			result.StartTime.Should().Be.Null();
			result.EndTime.Should().Be.Null();
		}
	}
}