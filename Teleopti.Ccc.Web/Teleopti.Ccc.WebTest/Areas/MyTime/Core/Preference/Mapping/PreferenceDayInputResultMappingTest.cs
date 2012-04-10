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
		private IPreferenceFeedbackProvider preferenceFeedbackProvider;

		[SetUp]
		public void Setup()
		{
			preferenceFeedbackProvider = MockRepository.GenerateMock<IPreferenceFeedbackProvider>();

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new PreferenceDayInputResultMappingProfile(Depend.On(preferenceFeedbackProvider))));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapPossibleStartTimes()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction
			                                                                    	{
			                                                                    		Absence = new Absence()
			                                                                    	});
			var workTimeMinMax = new WorkTimeMinMax(){StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10))};

			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today)).Return(workTimeMinMax);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay);

			result.PossibleStartTimes.Should().Be.EqualTo(workTimeMinMax.StartTimeLimitation.StartTimeString + "-" +
			                                              workTimeMinMax.StartTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapPossibleEndTimes()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction
			                                                                    	{
			                                                                    		Absence = new Absence()
			                                                                    	});
			var workTimeMinMax = new WorkTimeMinMax() { EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(18), TimeSpan.FromHours(19)) };

			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today)).Return(workTimeMinMax);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay);

			result.PossibleEndTimes.Should().Be.EqualTo(workTimeMinMax.EndTimeLimitation.StartTimeString + "-" +
														  workTimeMinMax.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapPossibleContractTimes()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction
			                                                                    	{
			                                                                    		Absence = new Absence()
			                                                                    	});
			var workTimeMinMax = new WorkTimeMinMax() { WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10)) };

			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today)).Return(workTimeMinMax);

			var result = Mapper.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay);

			result.PossibleContractTimes.Should().Be.EqualTo(workTimeMinMax.WorkTimeLimitation.StartTimeString + "-" +
														  workTimeMinMax.WorkTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapFeedbackError()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			var result = Mapper.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay);

			result.FeedbackError.Should().Be(Resources.NoAvailableShifts);
		}



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

			result.StyleClassName
				.Should().Be(Color.PeachPuff.ToStyleClass());
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

			result.StyleClassName
				.Should().Be(Color.Sienna.ToStyleClass());
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

			result.StyleClassName
				.Should().Be(Color.Thistle.ToStyleClass());
		}
	}
}