using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceDayInputMappingTest
	{
		private ILoggedOnUser loggedOnUser;
		private IShiftCategoryRepository shiftCategoryRepository;
		private IDayOffTemplateRepository dayOffRepository;
		private IAbsenceRepository absenceRepository;
		private IActivityRepository activityRepository;
		private PreferenceDayInputMapper target;

		[SetUp]
		public void Setup()
		{
			loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			shiftCategoryRepository = MockRepository.GenerateMock<IShiftCategoryRepository>();
			dayOffRepository = MockRepository.GenerateMock<IDayOffTemplateRepository>();
			absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();
			activityRepository = MockRepository.GenerateMock<IActivityRepository>();

			target = new PreferenceDayInputMapper(shiftCategoryRepository,dayOffRepository,absenceRepository,activityRepository,loggedOnUser);
		}

		[Test]
		public void ShouldMapToDestination()
		{
			var destination = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction());

			var result = target.Map(new PreferenceDayInput(), destination);

			result.Should().Be.SameInstanceAs(destination);
		}

		[Test]
		public void ShouldMapShiftCategoryToDestination()
		{
			var destination = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction());
			var input = new PreferenceDayInput { PreferenceId = Guid.NewGuid() };
			var shiftCategory = new ShiftCategory("sc");

			shiftCategoryRepository.Stub(x => x.Get(input.PreferenceId.Value)).Return(shiftCategory);

			target.Map(input, destination);

			destination.Restriction.ShiftCategory.Should().Be(shiftCategory);
		}

		[Test]
		public void ShouldMapTemplateNameToDestination()
		{
			var destination = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction()) {TemplateName = "name2"};
			var input = new PreferenceDayInput {PreferenceId = Guid.NewGuid(), TemplateName = "name1"};

			target.Map(input, destination);

			destination.TemplateName.Should().Be.EqualTo("name1");
		}

		[Test]
		public void ShouldMapTemplateName()
		{
			var result = target.Map(
				new PreferenceDayInput
					{
						TemplateName = "name1"
					});

			result.TemplateName.Should().Be.EqualTo("name1");
		}

		[Test]
		public void ShouldMapPerson()
		{
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var result = target.Map(new PreferenceDayInput());

			result.Person.Should().Be.SameInstanceAs(person);
		}

		[Test]
		public void ShouldMapDate()
		{
			var input = new PreferenceDayInput { Date = DateOnly.Today.AddDays(1) };

			var result = target.Map(input);

			result.RestrictionDate.Should().Be(input.Date);
		}

		[Test]
		public void ShouldMapShiftCategory()
		{
			var shiftCategory = new ShiftCategory("sc");
			shiftCategory.SetId(Guid.NewGuid());
			var input = new PreferenceDayInput { PreferenceId = shiftCategory.Id.Value };

			shiftCategoryRepository.Stub(x => x.Get(input.PreferenceId.Value)).Return(shiftCategory);

			var result = target.Map(input);

			result.Restriction.ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}

		[Test]
		public void ShouldMapDayOff()
		{
			var dayOffTemplate = new DayOffTemplate(new Description("do"));
			dayOffTemplate.SetId(Guid.NewGuid());
			var input = new PreferenceDayInput { PreferenceId = dayOffTemplate.Id.Value };

			dayOffRepository.Stub(x => x.Get(input.PreferenceId.Value)).Return(dayOffTemplate);

			var result = target.Map(input);

			result.Restriction.DayOffTemplate.Should().Be.SameInstanceAs(dayOffTemplate);
		}

		[Test]
		public void ShouldMapAbsence()
		{
			var absence = new Absence();
			absence.SetId(Guid.NewGuid());
			var input = new PreferenceDayInput { PreferenceId = absence.Id.Value };

			absenceRepository.Stub(x => x.Get(input.PreferenceId.Value)).Return(absence);

			var result = target.Map(input);

			result.Restriction.Absence.Should().Be.SameInstanceAs(absence);
		}

		[Test]
		public void ShouldMapStartTimeLimitation()
		{
			var input = new PreferenceDayInput
			            	{
			            		EarliestStartTime = new TimeOfDay(TimeSpan.FromHours(8)),
								LatestStartTime = new TimeOfDay(TimeSpan.FromHours(9))
			            	};

			var result = target.Map(input);

			result.Restriction.StartTimeLimitation
				.Should().Be.EqualTo(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
		}

		[Test]
		public void ShouldMapEndTimeLimitation()
		{
			var input = new PreferenceDayInput
			            	{
			            		EarliestEndTime = new TimeOfDay(TimeSpan.FromHours(16)), 
								LatestEndTime = new TimeOfDay(TimeSpan.FromHours(17))
			            	};

			var result = target.Map(input);

			result.Restriction.EndTimeLimitation
				.Should().Be.EqualTo(new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(17)));
		}

		[Test]
		public void ShouldMapEndTimeLimitationNextDay()
		{
			var input = new PreferenceDayInput
			            	{
			            		EarliestEndTime = new TimeOfDay(TimeSpan.FromHours(2)), 
								LatestEndTime = new TimeOfDay(TimeSpan.FromHours(3)), 
								EarliestEndTimeNextDay = true, 
								LatestEndTimeNextDay = true
			            	};

			var result = target.Map(input);

			result.Restriction.EndTimeLimitation
				.Should().Be.EqualTo(new EndTimeLimitation(TimeSpan.FromHours(24 + 2), TimeSpan.FromHours(24 + 3)));
		}

		[Test]
		public void ShouldMapWorkTimeLimitation()
		{
			var input = new PreferenceDayInput
			            	{
			            		MinimumWorkTime = TimeSpan.FromHours(7), 
								MaximumWorkTime = TimeSpan.FromHours(8)
			            	};

			var result = target.Map(input);

			result.Restriction.WorkTimeLimitation
				.Should().Be.EqualTo(new WorkTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(8)));
		}
		
		[Test]
		public void ShouldMapEmptyActivity()
		{
			var input = new PreferenceDayInput
			            	{
			            		ActivityPreferenceId = null,
								ActivityEarliestStartTime = new TimeOfDay(TimeSpan.FromHours(8))
			            	};

			var result = target.Map(input);

			result.Restriction.ActivityRestrictionCollection.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldMapActivity()
		{
			var activity = new Activity("Lunch");
			activity.SetId(Guid.NewGuid());
			var input = new PreferenceDayInput {ActivityPreferenceId = activity.Id.Value};

			activityRepository.Stub(x => x.Get(input.ActivityPreferenceId.Value)).Return(activity);

			var result = target.Map(input);

			result.Restriction.ActivityRestrictionCollection.Single().Activity.Should().Be(activity);
		}

		[Test]
		public void ShouldMapActivityStartTimeLimitation()
		{
			var input = new PreferenceDayInput
			            	{
			            		ActivityPreferenceId = Guid.NewGuid(),
								ActivityEarliestStartTime = new TimeOfDay(TimeSpan.FromHours(10)),
								ActivityLatestStartTime = new TimeOfDay(TimeSpan.FromHours(12)),
			            	};

			var result = target.Map(input);

			result.Restriction.ActivityRestrictionCollection.Single().StartTimeLimitation
				.Should().Be.EqualTo(new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(12)));
		}

		[Test]
		public void ShouldMapActivityEndTimeLimitation()
		{
			var input = new PreferenceDayInput
			            	{
			            		ActivityPreferenceId = Guid.NewGuid(),
			            		ActivityEarliestEndTime = new TimeOfDay(TimeSpan.FromHours(11)),
			            		ActivityLatestEndTime = new TimeOfDay(TimeSpan.FromHours(13)),
			            	};

			var result = target.Map(input);

			result.Restriction.ActivityRestrictionCollection.Single().EndTimeLimitation
				.Should().Be.EqualTo(new EndTimeLimitation(TimeSpan.FromHours(11), TimeSpan.FromHours(13)));
		}

		[Test]
		public void ShouldMapActivityWorkTimeLimitation()
		{
			var input = new PreferenceDayInput
			            	{
			            		ActivityPreferenceId = Guid.NewGuid(),
			            		ActivityMinimumTime = TimeSpan.FromHours(1),
			            		ActivityMaximumTime = TimeSpan.FromHours(3),
			            	};

			var result = target.Map(input);

			result.Restriction.ActivityRestrictionCollection.Single().WorkTimeLimitation
				.Should().Be.EqualTo(new WorkTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(3)));
		}
	}
}
