using System.Collections.Generic;
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
			var date = DateOnly.Today;
			var studentAvailabilityDay = new StudentAvailabilityDay(null, date, new List<IStudentAvailabilityRestriction>());
			var viewModel = new StudentAvailabilityDayViewModel();

			studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityDayForDate(date)).Return(studentAvailabilityDay);
			mapper.Stub(x => x.Map<IStudentAvailabilityDay, StudentAvailabilityDayViewModel>(studentAvailabilityDay)).Return(viewModel);

			var result = target.CreateDayViewModel(date);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldCreateEmptyDayViewModelWhenNoStudentAvailabilityFound()
		{
			var studentAvailabilityProvider = MockRepository.GenerateMock<IStudentAvailabilityProvider>();
			var target = new StudentAvailabilityViewModelFactory(null, studentAvailabilityProvider);

			var date = DateOnly.Today;
			studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityForDate(date)).Return(null);

			var result = target.CreateDayViewModel(date);

			result.Should().Be.Null();
		}
	}
}