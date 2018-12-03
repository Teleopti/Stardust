using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture,SetCulture("sv-SE")]
	public class PreferenceDayViewModelMappingTest
	{
		private IExtendedPreferencePredicate extendedPreferencePredicate;
		private PreferenceDayViewModelMapper target;

		[SetUp]
		public void Setup()
		{
			extendedPreferencePredicate = MockRepository.GenerateMock<IExtendedPreferencePredicate>();

			target = new PreferenceDayViewModelMapper(extendedPreferencePredicate);
		}
		
		[Test]
		public void ShouldMapEmptyPreferenceShiftCategoryWhenNoShiftCategory()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			var result = target.Map(preferenceDay);

			result.Preference.Should().Be.Null();
		}

		[Test]
		public void ShouldMapShiftCategoryPreference()
		{
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory("AM")};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = target.Map(preferenceDay);

			result.Preference.Should().Be(preferenceRestriction.ShiftCategory.Description.Name);
		}

		[Test]
		public void ShouldMapShiftCategoryColor()
		{
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory("sc") {DisplayColor = Color.PeachPuff}};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = target.Map(preferenceDay);

			result.Color
				.Should().Be(Color.PeachPuff.ToCSV());
		}

		[Test]
		public void ShouldMapDayOffTemplatePreference()
		{
			var preferenceRestriction = new PreferenceRestriction { DayOffTemplate = new DayOffTemplate(new Description("Day Off", "DO"))};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = target.Map(preferenceDay);

			result.Preference.Should().Be(preferenceRestriction.DayOffTemplate.Description.Name);
		}

		[Test]
		public void ShouldMapDayOffTemplateColor()
		{
			var preferenceRestriction = new PreferenceRestriction
			                            	{
			                            		DayOffTemplate = new DayOffTemplate(new Description()) { DisplayColor = Color.Sienna}
			                            	};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = target.Map(preferenceDay);

			result.Color
				.Should().Be(Color.Sienna.ToCSV());
		}

		[Test]
		public void ShouldMapAbsencePreference()
		{
			var absence = new Absence {Description = new Description("Holiday", "H")};
			var preferenceRestriction = new PreferenceRestriction { Absence = absence};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = target.Map(preferenceDay);

			result.Preference.Should().Be(preferenceRestriction.Absence.Description.Name);
		}

		[Test]
		public void ShouldMapAbsenceColor()
		{
			var preferenceRestriction = new PreferenceRestriction
			                            	{
			                            		Absence = new Absence {DisplayColor = Color.Thistle}
			                            	};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = target.Map(preferenceDay);

			result.Color
				.Should().Be(Color.Thistle.ToCSV());
		}

		[Test]
		public void ShouldMapPreferenceExtendedWhenExtended()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			extendedPreferencePredicate.Stub(x => x.IsExtended(preferenceDay)).Return(true);

			var result = target.Map(preferenceDay);

			result.Extended.Should().Be.True();
		}

		[Test]
		public void ShouldNotMapPreferenceExtendedWhenNotExtended()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			var result = target.Map(preferenceDay);

			result.Extended.Should().Be.False();
		}

		[Test]
		public void ShouldMapPreferenceExtendedIfNoShiftCategoryLikeStuffAndExtended()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			extendedPreferencePredicate.Stub(x => x.IsExtended(preferenceDay)).Return(true);

			var result = target.Map(preferenceDay);

			result.Preference.Should().Be(Resources.Extended);
		}


		[Test]
		public void ShouldNotMapPreferenceExtendedIfShiftCategoryAndExtended()
		{
			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
					{
						ShiftCategory = new ShiftCategory("sc")
					});

			extendedPreferencePredicate.Stub(x => x.IsExtended(preferenceDay)).Return(true);

			var result = target.Map(preferenceDay);

			result.Preference.Should().Not.Be(Resources.Extended);
		}

		[Test]
		public void ShouldNotMapPreferenceExtendedIfAbsenceAndExtended()
		{
			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
					{
						Absence = new Absence()
					});

			extendedPreferencePredicate.Stub(x => x.IsExtended(preferenceDay)).Return(true);

			var result = target.Map(preferenceDay);

			result.Preference.Should().Not.Be(Resources.Extended);
		}

		[Test]
		public void ShouldNotMapPreferenceExtendedIfDayOffAndExtended()
		{
			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
					{
						DayOffTemplate = new DayOffTemplate(new Description())
					});

			extendedPreferencePredicate.Stub(x => x.IsExtended(preferenceDay)).Return(true);

			var result = target.Map(preferenceDay);

			result.Preference.Should().Not.Be(Resources.Extended);
		}

		[Test]
		public void ShouldMapExtendedTitle()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			var result = target.Map(preferenceDay);

			result.ExtendedTitle.Should().Be(Resources.Extended);
		}

		[Test]
		public void ShouldMapExtendedTitleFromShiftCategory()
		{
			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
				{
					ShiftCategory = new ShiftCategory("Late")
				});

			var result = target.Map(preferenceDay);

			result.ExtendedTitle.Should().Be("Late");
		}

		[Test]
		public void ShouldMapExtendedTitleFromAbsence()
		{
			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
				{
					Absence = new Absence() { Description = new Description("Absence")}
				});

			var result = target.Map(preferenceDay);

			result.ExtendedTitle.Should().Be("Absence");
		}

		[Test]
		public void ShouldMapExtendedTitleFromDayOff()
		{
			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
				{
					DayOffTemplate = new DayOffTemplate(new Description("DayOff"))
				});

			var result = target.Map(preferenceDay);

			result.ExtendedTitle.Should().Be("DayOff");
		}

		[Test]
		public void ShouldMapMustHave()
		{
			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
				{
					MustHave = true
				});

			var result = target.Map(preferenceDay);

			result.MustHave.Should().Be.True();
		}

		[Test]
		public void ShouldMapStartTimeLimitation()
		{
			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
				{
					StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9)) 
				});

			var result = target.Map(preferenceDay);

			result.StartTimeLimitation.Should().Be(
				preferenceDay.Restriction.StartTimeLimitation.StartTimeString + "-" + preferenceDay.Restriction.StartTimeLimitation.EndTimeString
				);
		}

		[Test]
		public void ShouldMapEndTimeLimitation()
		{
			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
				{
					EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9))
				});

			var result = target.Map(preferenceDay);

			result.EndTimeLimitation.Should().Be(
				preferenceDay.Restriction.EndTimeLimitation.StartTimeString + "-" + preferenceDay.Restriction.EndTimeLimitation.EndTimeString
				);
		}

		[Test]
		public void ShouldMapWorkTimeLimitation()
		{
			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
				{
					WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
				});

			var result = target.Map(preferenceDay);

			result.WorkTimeLimitation.Should().Be(
				preferenceDay.Restriction.WorkTimeLimitation.StartTimeString + "-" + preferenceDay.Restriction.WorkTimeLimitation.EndTimeString
				);
		}

		[Test]
		public void ShouldMapActivity()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());
			preferenceDay.Restriction.AddActivityRestriction(new ActivityRestriction(new Activity("Lunch")));

			var result = target.Map(preferenceDay);

			result.Activity.Should().Be("Lunch");
		}

		[Test]
		public void ShouldMapNoneWhenNoActivity()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			var result = target.Map(preferenceDay);

			result.Activity.Should().Be("(" + Resources.NoActivity + ")");
		}

		[Test]
		public void ShouldMapActivityStartTimeLimitation()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());
			var activityRestriction = new ActivityRestriction(new Activity("a"))
			                          	{
			                          		StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9))
			                          	};
			preferenceDay.Restriction.AddActivityRestriction(activityRestriction);

			var result = target.Map(preferenceDay);

			result.ActivityStartTimeLimitation.Should().Be(activityRestriction.StartTimeLimitation.StartTimeString + "-" + activityRestriction.StartTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapActivityEndTimeLimitation()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());
			var activityRestriction = new ActivityRestriction(new Activity("a"))
			{
				EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9))
			};
			preferenceDay.Restriction.AddActivityRestriction(activityRestriction);

			var result = target.Map(preferenceDay);

			result.ActivityEndTimeLimitation.Should().Be(activityRestriction.EndTimeLimitation.StartTimeString + "-" + activityRestriction.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapActivityWorkTimeLimitation()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());
			var activityRestriction = new ActivityRestriction(new Activity("a"))
			{
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			preferenceDay.Restriction.AddActivityRestriction(activityRestriction);

			var result = target.Map(preferenceDay);

			result.ActivityTimeLimitation.Should().Be(activityRestriction.WorkTimeLimitation.StartTimeString + "-" + activityRestriction.WorkTimeLimitation.EndTimeString);
		}


	}
}