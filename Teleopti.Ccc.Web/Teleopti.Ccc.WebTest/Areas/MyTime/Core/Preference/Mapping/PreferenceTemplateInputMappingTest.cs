using System;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.WebTest.Core.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceTemplateInputMappingTest
	{
		private ILoggedOnUser loggedOnUser;
		private IShiftCategoryRepository shiftCategoryRepository;
		private IDayOffTemplateRepository dayOffRepository;
		private IAbsenceRepository absenceRepository;
		private IActivityRepository activityRepository;

		[SetUp]
		public void Setup()
		{
			loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			shiftCategoryRepository = MockRepository.GenerateMock<IShiftCategoryRepository>();
			dayOffRepository = MockRepository.GenerateMock<IDayOffTemplateRepository>();
			absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();
			activityRepository = MockRepository.GenerateMock<IActivityRepository>();

			Mapper.Reset();
			Mapper.Initialize(
				c =>
					{
						c.ConstructServicesUsing(t =>
						                         new PreferenceTemplateInputMappingProfile.PreferenceTemplateInputToExtendedPreferenceTemplate(
							                         () => Mapper.Engine,
							                         () => loggedOnUser
							                         ));
						c.AddProfile(
							new PreferenceTemplateInputMappingProfile(
								shiftCategoryRepository,
								dayOffRepository,
								absenceRepository,
								activityRepository,
								new Lazy<IMappingEngine>(() => Mapper.Engine))
							);
					}
				);
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapTemplateName()
		{
			var input = new PreferenceTemplateInput
				{
					NewTemplateName = "name1"
				};
			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Name.Should().Be.EqualTo(input.NewTemplateName);
		}

		[Test]
		public void ShouldMapPerson()
		{
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(new PreferenceTemplateInput());

			result.Person.Should().Be.SameInstanceAs(person);
		}

		[Test]
		public void ShouldMapShiftCategory()
		{
			var shiftCategory = new ShiftCategory("sc");
			shiftCategory.SetId(Guid.NewGuid());
			var input = new PreferenceTemplateInput { PreferenceId = shiftCategory.Id.Value };

			shiftCategoryRepository.Stub(x => x.Get(input.PreferenceId.Value)).Return(shiftCategory);

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Restriction.ShiftCategory.Should().Be.SameInstanceAs(shiftCategory);
		}

		[Test]
		public void ShouldMapDayOff()
		{
			var dayOffTemplate = new DayOffTemplate(new Description("do"));
			dayOffTemplate.SetId(Guid.NewGuid());
			var input = new PreferenceTemplateInput { PreferenceId = dayOffTemplate.Id.Value };

			dayOffRepository.Stub(x => x.Get(input.PreferenceId.Value)).Return(dayOffTemplate);

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Restriction.DayOffTemplate.Should().Be.SameInstanceAs(dayOffTemplate);
		}

		[Test]
		public void ShouldMapAbsence()
		{
			var absence = new Absence();
			absence.SetId(Guid.NewGuid());
			var input = new PreferenceTemplateInput { PreferenceId = absence.Id.Value };

			absenceRepository.Stub(x => x.Get(input.PreferenceId.Value)).Return(absence);

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Restriction.Absence.Should().Be.SameInstanceAs(absence);
		}

		[Test]
		public void ShouldMapStartTimeLimitation()
		{
			var input = new PreferenceTemplateInput
			{
				EarliestStartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				LatestStartTime = new TimeOfDay(TimeSpan.FromHours(9))
			};

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Restriction.StartTimeLimitation
				.Should().Be.EqualTo(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9)));
		}

		[Test]
		public void ShouldMapEndTimeLimitation()
		{
			var input = new PreferenceTemplateInput
			{
				EarliestEndTime = new TimeOfDay(TimeSpan.FromHours(16)),
				LatestEndTime = new TimeOfDay(TimeSpan.FromHours(17))
			};

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Restriction.EndTimeLimitation
				.Should().Be.EqualTo(new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(17)));
		}

		[Test]
		public void ShouldMapEndTimeLimitationNextDay()
		{
			var input = new PreferenceTemplateInput
			{
				EarliestEndTime = new TimeOfDay(TimeSpan.FromHours(2)),
				LatestEndTime = new TimeOfDay(TimeSpan.FromHours(3)),
				EarliestEndTimeNextDay = true,
				LatestEndTimeNextDay = true
			};

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Restriction.EndTimeLimitation
				.Should().Be.EqualTo(new EndTimeLimitation(TimeSpan.FromHours(24 + 2), TimeSpan.FromHours(24 + 3)));
		}

		[Test]
		public void ShouldMapWorkTimeLimitation()
		{
			var input = new PreferenceTemplateInput
			{
				MinimumWorkTime = TimeSpan.FromHours(7),
				MaximumWorkTime = TimeSpan.FromHours(8)
			};

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Restriction.WorkTimeLimitation
				.Should().Be.EqualTo(new WorkTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(8)));
		}

		[Test]
		public void ShouldMapEmptyActivity()
		{
			var input = new PreferenceTemplateInput
			{
				ActivityPreferenceId = null,
				ActivityEarliestStartTime = new TimeOfDay(TimeSpan.FromHours(8))
			};

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Restriction.ActivityRestrictionCollection.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldMapActivity()
		{
			var activity = new Activity("Lunch");
			activity.SetId(Guid.NewGuid());
			var input = new PreferenceTemplateInput { ActivityPreferenceId = activity.Id.Value };

			activityRepository.Stub(x => x.Get(input.ActivityPreferenceId.Value)).Return(activity);

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Restriction.ActivityRestrictionCollection.Single().Activity.Should().Be(activity);
		}

		[Test]
		public void ShouldMapActivityStartTimeLimitation()
		{
			var input = new PreferenceTemplateInput
			{
				ActivityPreferenceId = Guid.NewGuid(),
				ActivityEarliestStartTime = new TimeOfDay(TimeSpan.FromHours(10)),
				ActivityLatestStartTime = new TimeOfDay(TimeSpan.FromHours(12)),
			};

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Restriction.ActivityRestrictionCollection.Single().StartTimeLimitation
				.Should().Be.EqualTo(new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(12)));
		}

		[Test]
		public void ShouldMapActivityEndTimeLimitation()
		{
			var input = new PreferenceTemplateInput
			{
				ActivityPreferenceId = Guid.NewGuid(),
				ActivityEarliestEndTime = new TimeOfDay(TimeSpan.FromHours(11)),
				ActivityLatestEndTime = new TimeOfDay(TimeSpan.FromHours(13)),
			};

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Restriction.ActivityRestrictionCollection.Single().EndTimeLimitation
				.Should().Be.EqualTo(new EndTimeLimitation(TimeSpan.FromHours(11), TimeSpan.FromHours(13)));
		}

		[Test]
		public void ShouldMapActivityWorkTimeLimitation()
		{
			var input = new PreferenceTemplateInput
			{
				ActivityPreferenceId = Guid.NewGuid(),
				ActivityMinimumTime = TimeSpan.FromHours(1),
				ActivityMaximumTime = TimeSpan.FromHours(3),
			};

			var result = Mapper.Map<PreferenceTemplateInput, IExtendedPreferenceTemplate>(input);

			result.Restriction.ActivityRestrictionCollection.Single().WorkTimeLimitation
				.Should().Be.EqualTo(new WorkTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(3)));
		}
	}
}