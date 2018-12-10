using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	[TestFixture]
	public class OvertimeAvailabilityPersisterTest
	{
		[Test]
		public void ShouldAddOvertimeAvailability()
		{
			var overtimeAvailabilityRepository = MockRepository.GenerateMock<IOvertimeAvailabilityRepository>();
			var input = new OvertimeAvailabilityInput();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			overtimeAvailabilityRepository.Stub(x => x.Find(input.Date, person)).Return(null);
			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository,
			                                               loggedOnUser, new OvertimeAvailabilityInputMapper(loggedOnUser), new OvertimeAvailabilityViewModelMapper(new SwedishCulture()));
			target.Persist(input);

			overtimeAvailabilityRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldReturnInputResultModelOnAdd()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var overtimeAvailabilityRepository = MockRepository.GenerateMock<IOvertimeAvailabilityRepository>();
			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository, loggedOnUser, new OvertimeAvailabilityInputMapper(loggedOnUser), new OvertimeAvailabilityViewModelMapper(new SwedishCulture()));
			var input = new OvertimeAvailabilityInput();
			
			target.Persist(input).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUpdateExistingOvertimeAvailability()
		{
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
			
			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository, loggedOnUser, new OvertimeAvailabilityInputMapper(loggedOnUser), new OvertimeAvailabilityViewModelMapper(new SwedishCulture()));
			target.Persist(input);

			overtimeAvailabilityRepository.AssertWasCalled(x => x.Remove(existingOvertimeAvailability));
			overtimeAvailabilityRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldReturnFormResultModelOnUpdate()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var overtimeAvailabilityRepository = MockRepository.GenerateMock<IOvertimeAvailabilityRepository>();
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var input = new OvertimeAvailabilityInput();
			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository, loggedOnUser, new OvertimeAvailabilityInputMapper(loggedOnUser), new OvertimeAvailabilityViewModelMapper(new SwedishCulture()));
			var existingOvertimeAvailability = MockRepository.GenerateMock<IOvertimeAvailability>();
			overtimeAvailabilityRepository.Stub(x => x.Find(input.Date, person))
			                              .Return(new List<IOvertimeAvailability>
				                              {
					                              existingOvertimeAvailability
				                              });

			target.Persist(input).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDeleteOvertimeAvailability()
		{
			var overtimeAvailabilityRepository = MockRepository.GenerateMock<IOvertimeAvailabilityRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var date = DateOnly.Today;
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var overtimeAvailability = MockRepository.GenerateMock<IOvertimeAvailability>();
			overtimeAvailabilityRepository.Stub(x => x.Find(date, person))
			                              .Return(new List<IOvertimeAvailability>
				                              {
					                              overtimeAvailability
				                              });
			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository, loggedOnUser, new OvertimeAvailabilityInputMapper(loggedOnUser), new OvertimeAvailabilityViewModelMapper(new SwedishCulture()));

			var result = target.Delete(date);

			overtimeAvailabilityRepository.AssertWasCalled(x => x.Remove(overtimeAvailability));
			result.HasOvertimeAvailability.Should().Be.False();
		}
	}
}