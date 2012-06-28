using System;
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
	public class PreferenceDayInputResultMappingTest
	{
		[SetUp]
		public void Setup()
		{
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new PreferenceDayInputResultMappingProfile()));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapDate()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			var result = Mapper.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay);

			result.Date.Should().Be(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapShiftCategoryPreference()
		{
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory("AM")};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay);

			result.PreferenceRestriction.Should().Be(preferenceRestriction.ShiftCategory.Description.Name);
		}

		[Test]
		public void ShouldMapShiftCategoryStyleClassName()
		{
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory(" ") {DisplayColor = Color.PeachPuff}};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay);

			result.HexColor
				.Should().Be(Color.PeachPuff.ToHtml());
		}

		[Test]
		public void ShouldMapDayOffTemplatePreference()
		{
			var preferenceRestriction = new PreferenceRestriction { DayOffTemplate = new DayOffTemplate(new Description("Day Off", "DO"))};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay);

			result.PreferenceRestriction.Should().Be(preferenceRestriction.DayOffTemplate.Description.Name);
		}

		[Test]
		public void ShouldMapDayOffTemplateStyleClassName()
		{
			var preferenceRestriction = new PreferenceRestriction
			                            	{
			                            		DayOffTemplate = new DayOffTemplate(new Description()) { DisplayColor = Color.Sienna}
			                            	};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay);

			result.HexColor
				.Should().Be(Color.Sienna.ToHtml());
		}

		[Test]
		public void ShouldMapAbsencePreference()
		{
			var absence = new Absence {Description = new Description("Holiday", "H")};
			var preferenceRestriction = new PreferenceRestriction { Absence = absence};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay);

			result.PreferenceRestriction.Should().Be(preferenceRestriction.Absence.Description.Name);
		}

		[Test]
		public void ShouldMapAbsenceStyleClassName()
		{
			var preferenceRestriction = new PreferenceRestriction
			                            	{
			                            		Absence = new Absence() {DisplayColor = Color.Thistle}
			                            	};
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay);

			result.HexColor
				.Should().Be(Color.Thistle.ToHtml());
		}
	}
}