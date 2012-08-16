using System.Drawing;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.WebTest.Core.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceDayViewModelMappingTest
	{
		private IExtendedPreferencePredicate extendedPreferencePredicate;
		private IPreferenceProvider preferenceDayProvider;

		[SetUp]
		public void Setup()
		{
			preferenceDayProvider = MockRepository.GenerateMock<IPreferenceProvider>();
			extendedPreferencePredicate = MockRepository.GenerateMock<IExtendedPreferencePredicate>();

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new PreferenceDayViewModelMappingProfile(
				Depend.On(Mapper.Engine),
				Depend.On(extendedPreferencePredicate),
				Depend.On(preferenceDayProvider)
				)));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapEmptyPreferenceShiftCategoryWhenNoShiftCategory()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

			result.Preference.Should().Be.Null();
		}

		[Test]
		public void ShouldMapShiftCategoryPreference()
		{
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory("AM")};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

			result.Preference.Should().Be(preferenceRestriction.ShiftCategory.Description.Name);
		}

		[Test]
		public void ShouldMapShiftCategoryColor()
		{
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory(" ") {DisplayColor = Color.PeachPuff}};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

			result.Color
				.Should().Be(Color.PeachPuff.ToHtml());
		}

		[Test]
		public void ShouldMapDayOffTemplatePreference()
		{
			var preferenceRestriction = new PreferenceRestriction { DayOffTemplate = new DayOffTemplate(new Description("Day Off", "DO"))};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

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

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

			result.Color
				.Should().Be(Color.Sienna.ToHtml());
		}

		[Test]
		public void ShouldMapAbsencePreference()
		{
			var absence = new Absence {Description = new Description("Holiday", "H")};
			var preferenceRestriction = new PreferenceRestriction { Absence = absence};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

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

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

			result.Color
				.Should().Be(Color.Thistle.ToHtml());
		}

		[Test]
		public void ShouldMapPreferenceExtendedWhenExtended()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			extendedPreferencePredicate.Stub(x => x.IsExtended(preferenceDay)).Return(true);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

			result.Extended.Should().Be.True();
		}

		[Test]
		public void ShouldNotMapPreferenceExtendedWhenNotExtended()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

			result.Extended.Should().Be.False();
		}

		[Test]
		public void ShouldMapPreferenceExtendedIfNoShiftCategoryLikeStuffAndExtended()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			extendedPreferencePredicate.Stub(x => x.IsExtended(preferenceDay)).Return(true);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

			result.Preference.Should().Be(Resources.Extended);
		}


		[Test]
		public void ShouldNotMapPreferenceExtendedIfShiftCategoryAndExtended()
		{
			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
					{
						ShiftCategory = new ShiftCategory(" ")
					});

			extendedPreferencePredicate.Stub(x => x.IsExtended(preferenceDay)).Return(true);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

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

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

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

			var result = Mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);

			result.Preference.Should().Not.Be(Resources.Extended);
		}

		//[Test]
		//public void ShouldMapPreferenceExtendedTitle()
		//{
		//    var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());
		//    data.Days = new[] { new PreferenceDayDomainData { Date = data.SelectedDate, PreferenceDay = preferenceDay } };

		//    var result = Mapper.Map<PreferenceDomainData, PreferenceViewModel>(data);

		//    result.DayViewModel(data.SelectedDate)
		//        .Preference.ExtendedTitle.Should().Be(Resources.Extended);
		//}

	}
}