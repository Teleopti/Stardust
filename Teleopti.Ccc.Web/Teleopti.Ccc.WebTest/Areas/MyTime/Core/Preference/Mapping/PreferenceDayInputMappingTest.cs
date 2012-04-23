using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceDayInputMappingTest
	{
		private ILoggedOnUser loggedOnUser;
		private IShiftCategoryRepository shiftCategoryRepository;
		private IDayOffRepository dayOffRepository;
		private IAbsenceRepository absenceRepository;

		[SetUp]
		public void Setup()
		{
			loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			shiftCategoryRepository = MockRepository.GenerateMock<IShiftCategoryRepository>();
			dayOffRepository = MockRepository.GenerateMock<IDayOffRepository>();
			absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();

			Mapper.Reset();
			Mapper.Initialize(
				c =>
					{
						c.ConstructServicesUsing(t => 
							new PreferenceDayInputMappingProfile.PreferenceDayInputToPreferenceDay(
								() => Mapper.Engine, 
								() => loggedOnUser
							));
						c.AddProfile(
							new PreferenceDayInputMappingProfile(
						        () => Mapper.Engine,
						        () => loggedOnUser,
						        () => shiftCategoryRepository,
						        () => dayOffRepository,
						        () => absenceRepository
						        )
							);
					}
				);
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapToDestination()
		{
			var destination = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction());

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(new PreferenceDayInput(), destination);

			result.Should().Be.SameInstanceAs(destination);
		}

		[Test]
		public void ShouldMapShiftCategoryToDestination()
		{
			var destination = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction());
			var input = new PreferenceDayInput { Id = Guid.NewGuid() };
			var shiftCategory = new ShiftCategory(" ");

			shiftCategoryRepository.Stub(x => x.Get(input.Id)).Return(shiftCategory);

			Mapper.Map<PreferenceDayInput, IPreferenceDay>(input, destination);

			destination.Restriction.ShiftCategory.Should().Be(shiftCategory);
		}

		[Test]
		public void ShouldMapPerson()
		{
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(new PreferenceDayInput());

			result.Person.Should().Be.SameInstanceAs(person);
		}

		[Test]
		public void ShouldMapDate()
		{
			var input = new PreferenceDayInput { Date = DateOnly.Today.AddDays(1) };

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(input);

			result.RestrictionDate.Should().Be(input.Date);
		}

		[Test]
		public void ShouldMapShiftCategory()
		{
			var shiftCategory = new ShiftCategory(" ");
			shiftCategory.SetId(Guid.NewGuid());
			var input = new PreferenceDayInput { Id = shiftCategory.Id.Value };

			shiftCategoryRepository.Stub(x => x.Get(input.Id)).Return(shiftCategory);

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(input);

			result.Restriction.ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}

		[Test]
		public void ShouldMapDayOff()
		{
			var dayOffTemplate = new DayOffTemplate(new Description(" "));
			dayOffTemplate.SetId(Guid.NewGuid());
			var input = new PreferenceDayInput { Id = dayOffTemplate.Id.Value };

			dayOffRepository.Stub(x => x.Get(input.Id)).Return(dayOffTemplate);

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(input);

			result.Restriction.DayOffTemplate.Should().Be.SameInstanceAs(dayOffTemplate);
		}

		[Test]
		public void ShouldMapAbsence()
		{
			var absence = new Absence();
			absence.SetId(Guid.NewGuid());
			var input = new PreferenceDayInput { Id = absence.Id.Value };

			absenceRepository.Stub(x => x.Get(input.Id)).Return(absence);

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(input);

			result.Restriction.Absence.Should().Be.SameInstanceAs(absence);
		}

	}
}
