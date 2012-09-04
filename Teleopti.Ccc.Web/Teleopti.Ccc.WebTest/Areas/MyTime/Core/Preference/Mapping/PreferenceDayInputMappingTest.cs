using System;
using System.Linq;
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
		private IActivityRepository activityRepository;

		[SetUp]
		public void Setup()
		{
			loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			shiftCategoryRepository = MockRepository.GenerateMock<IShiftCategoryRepository>();
			dayOffRepository = MockRepository.GenerateMock<IDayOffRepository>();
			absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();
			activityRepository = MockRepository.GenerateMock<IActivityRepository>();

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
						        () => shiftCategoryRepository,
						        () => dayOffRepository,
						        () => absenceRepository,
								() => activityRepository
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
			var input = new PreferenceDayInput { PreferenceId = Guid.NewGuid() };
			var shiftCategory = new ShiftCategory(" ");

			shiftCategoryRepository.Stub(x => x.Get(input.PreferenceId)).Return(shiftCategory);

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
			var input = new PreferenceDayInput { PreferenceId = shiftCategory.Id.Value };

			shiftCategoryRepository.Stub(x => x.Get(input.PreferenceId)).Return(shiftCategory);

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(input);

			result.Restriction.ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}

		[Test]
		public void ShouldMapDayOff()
		{
			var dayOffTemplate = new DayOffTemplate(new Description(" "));
			dayOffTemplate.SetId(Guid.NewGuid());
			var input = new PreferenceDayInput { PreferenceId = dayOffTemplate.Id.Value };

			dayOffRepository.Stub(x => x.Get(input.PreferenceId)).Return(dayOffTemplate);

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(input);

			result.Restriction.DayOffTemplate.Should().Be.SameInstanceAs(dayOffTemplate);
		}

		[Test]
		public void ShouldMapAbsence()
		{
			var absence = new Absence();
			absence.SetId(Guid.NewGuid());
			var input = new PreferenceDayInput { PreferenceId = absence.Id.Value };

			absenceRepository.Stub(x => x.Get(input.PreferenceId)).Return(absence);

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(input);

			result.Restriction.Absence.Should().Be.SameInstanceAs(absence);
		}

		[Test]
		public void ShouldMapStartTimeLimitation()
		{
			var earliest = TimeSpan.FromHours(8);
			var latest = TimeSpan.FromHours(9);
			var input = new PreferenceDayInput { EarliestStartTime = earliest,LatestStartTime = latest};

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(input);

			result.Restriction.StartTimeLimitation.StartTime.Should().Be.EqualTo(earliest);
			result.Restriction.StartTimeLimitation.EndTime.Should().Be.EqualTo(latest);
		}

		[Test]
		public void ShouldMapEndTimeLimitation()
		{
			var earliest = TimeSpan.FromHours(16);
			var latest = TimeSpan.FromHours(17);
			var input = new PreferenceDayInput { EarliestEndTime = earliest, LatestEndTime = latest };

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(input);

			result.Restriction.EndTimeLimitation.StartTime.Should().Be.EqualTo(earliest);
			result.Restriction.EndTimeLimitation.EndTime.Should().Be.EqualTo(latest);
		}

		[Test]
		public void ShouldMapWorkTimeLimitation()
		{
			var shortest = TimeSpan.FromHours(7);
			var longest = TimeSpan.FromHours(8);
			var input = new PreferenceDayInput { MinimumWorkTime = shortest, MaximumWorkTime = longest };

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(input);

			result.Restriction.WorkTimeLimitation.StartTime.Should().Be.EqualTo(shortest);
			result.Restriction.WorkTimeLimitation.EndTime.Should().Be.EqualTo(longest);
		}

		[Test]
		public void ShouldMapActivityRestriction()
		{
			var shortest = TimeSpan.FromHours(0.5);
			var longest = TimeSpan.FromHours(1);

			var earliestStartTime = TimeSpan.FromHours(11);
			var latestStartTime = TimeSpan.FromHours(12);

			var earliestEndTime = TimeSpan.FromHours(11.5);
			var latestEndTime = TimeSpan.FromHours(13);

			var activity = new Activity("Lunch");
			activity.SetId(Guid.NewGuid());

			var input = new PreferenceDayInput
				{
					ActivityMinimumTime = shortest,
					ActivityMaximumTime = longest,
					ActivityPreferenceId = activity.Id.Value,
					ActivityEarliestStartTime = earliestStartTime,
					ActivityLatestStartTime = latestStartTime,
					ActivityEarliestEndTime = earliestEndTime,
					ActivityLatestEndTime = latestEndTime
				};

			activityRepository.Stub(x => x.Get(input.ActivityPreferenceId)).Return(activity);

			var result = Mapper.Map<PreferenceDayInput, IPreferenceDay>(input);

			var activityRestriction = result.Restriction.ActivityRestrictionCollection.Single();
			activityRestriction.StartTimeLimitation.StartTime.Should().Be.EqualTo(earliestStartTime);
			activityRestriction.StartTimeLimitation.EndTime.Should().Be.EqualTo(latestStartTime);
			activityRestriction.EndTimeLimitation.StartTime.Should().Be.EqualTo(earliestEndTime);
			activityRestriction.EndTimeLimitation.EndTime.Should().Be.EqualTo(latestEndTime);
			activityRestriction.WorkTimeLimitation.StartTime.Should().Be.EqualTo(shortest);
			activityRestriction.WorkTimeLimitation.EndTime.Should().Be.EqualTo(longest);
		}
	}
}
