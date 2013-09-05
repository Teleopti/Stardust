using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	[TestFixture]
	public class OvertimeAvailabilityPersisterTest
	{
		[Test]
		public void ShouldAddOvertimeAvailability()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var overtimeAvailabilityRepository = MockRepository.GenerateMock<IOvertimeAvailabilityRepository>();
			var input = new OvertimeAvailabilityInput();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			overtimeAvailabilityRepository.Stub(x => x.Find(input.Date, person)).Return(null);
			var overtimeAvailability = new OvertimeAvailability(person, input.Date, input.StartTime.ToTimeSpan(), input.EndTime.ToTimeSpan());
			mapper.Stub(x => x.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(input))
			      .Return(overtimeAvailability);
			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository,
			                                               loggedOnUser, mapper);
			target.Persist(input);

			overtimeAvailabilityRepository.AssertWasCalled(x => x.Add(overtimeAvailability));
			mapper.AssertWasNotCalled(x => x.Map(input, (IOvertimeAvailability)null));
		}

		[Test]
		public void ShouldReturnInputResultModelOnAdd()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var overtimeAvailabilityRepository = MockRepository.GenerateMock<IOvertimeAvailabilityRepository>();
			var overtimeAvailability = MockRepository.GenerateMock<IOvertimeAvailability>();
			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository, loggedOnUser, mapper);
			var input = new OvertimeAvailabilityInput();

			mapper.Stub(x => x.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(input))
			      .Return(overtimeAvailability);
			var viewModel = new OvertimeAvailabilityViewModel();
			mapper.Stub(x => x.Map<IOvertimeAvailability, OvertimeAvailabilityViewModel>(overtimeAvailability))
			      .Return(viewModel);

			var result = target.Persist(input);
			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldUpdateExistingOvertimeAvailability()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var overtimeAvailabilityRepository = MockRepository.GenerateMock<IOvertimeAvailabilityRepository>();
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var input = new OvertimeAvailabilityInput();
			var existingOvertimeAvailability = MockRepository.GenerateMock<IOvertimeAvailability>();
			overtimeAvailabilityRepository.Stub(x => x.Find(input.Date, person))
			                              .Return(new List<IOvertimeAvailability>
				                              {
					                              existingOvertimeAvailability
				                              });
			var newOvertimeAvailability = MockRepository.GenerateMock<IOvertimeAvailability>();
			mapper.Stub(x => x.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(input))
			      .Return(newOvertimeAvailability);

			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository, loggedOnUser, mapper);
			target.Persist(input);

			overtimeAvailabilityRepository.AssertWasCalled(x => x.Remove(existingOvertimeAvailability));
			overtimeAvailabilityRepository.AssertWasCalled(x => x.Add(newOvertimeAvailability));
		}

		[Test]
		public void ShouldReturnFormResultModelOnUpdate()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var overtimeAvailabilityRepository = MockRepository.GenerateMock<IOvertimeAvailabilityRepository>();
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var input = new OvertimeAvailabilityInput();
			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository, loggedOnUser, mapper);
			var existingOvertimeAvailability = MockRepository.GenerateMock<IOvertimeAvailability>();
			overtimeAvailabilityRepository.Stub(x => x.Find(input.Date, person))
			                              .Return(new List<IOvertimeAvailability>
				                              {
					                              existingOvertimeAvailability
				                              });
			var newOvertimeAvailability = MockRepository.GenerateMock<IOvertimeAvailability>();
			mapper.Stub(x => x.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(input))
			      .Return(newOvertimeAvailability);
			var viewModel = new OvertimeAvailabilityViewModel();
			mapper.Stub(x => x.Map<IOvertimeAvailability, OvertimeAvailabilityViewModel>(newOvertimeAvailability))
			      .Return(viewModel);

			var result = target.Persist(input);
			result.Should().Be.SameInstanceAs(viewModel);

		}
	}
}