using System;
using System.Drawing;
using AutoMapper;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceTemplateViewModelMappingTest
	{
		[SetUp]
		public void Setup()
		{
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new PreferenceTemplateViewModelMappingProfile()));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapName()
		{
			var template = new PreferenceRestrictionTemplate();
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, template, "name", Color.Red);
			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.Name.Should().Be.EqualTo("name");
		}

		[Test]
		public void ShouldMapShiftCategoryPreference()
		{
			var template = new PreferenceRestrictionTemplate { ShiftCategory = new ShiftCategory("AM") };
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, template, "name", Color.Red);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.Preference.Should().Be(template.ShiftCategory.Description.Name);
		}

		[Test]
		public void ShouldMapDayOffTemplatePreference()
		{
			var template = new PreferenceRestrictionTemplate { DayOffTemplate = new DayOffTemplate(new Description("Day Off", "DO")) };
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, template, "name", Color.Red);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.Preference.Should().Be(template.DayOffTemplate.Description.Name);
		}


		[Test]
		public void ShouldMapAbsencePreference()
		{
			var absence = new Absence { Description = new Description("Holiday", "H") };
			var template = new PreferenceRestrictionTemplate { Absence = absence };
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, template, "name", Color.Red);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.Preference.Should().Be(template.Absence.Description.Name);
		}
	
		[Test]
		public void ShouldMapStartTimeLimitation()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(
				null, new PreferenceRestrictionTemplate
				{
					StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9))
				}, "name", Color.Red);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.StartTimeLimitation.Should().Be(
				extendedPreferenceTemplate.Restriction.StartTimeLimitation.StartTimeString + "-" + extendedPreferenceTemplate.Restriction.StartTimeLimitation.EndTimeString
				);
		}

		[Test]
		public void ShouldMapEndTimeLimitation()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(
				null, new PreferenceRestrictionTemplate
				{
					EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9))
				}, "name", Color.Red);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.EndTimeLimitation.Should().Be(
				extendedPreferenceTemplate.Restriction.EndTimeLimitation.StartTimeString + "-" + extendedPreferenceTemplate.Restriction.EndTimeLimitation.EndTimeString
				);
		}

		[Test]
		public void ShouldMapWorkTimeLimitation()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(
				null, new PreferenceRestrictionTemplate
				{
					WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
				}, "name", Color.Red);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.WorkTimeLimitation.Should().Be(
				extendedPreferenceTemplate.Restriction.WorkTimeLimitation.StartTimeString + "-" + extendedPreferenceTemplate.Restriction.WorkTimeLimitation.EndTimeString
				);
		}

		[Test]
		public void ShouldMapActivity()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, new PreferenceRestrictionTemplate(), "name", Color.Red);

			extendedPreferenceTemplate.Restriction.AddActivityRestriction(new ActivityRestriction(new Activity("Lunch")));
			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.Activity.Should().Be("Lunch");
		}

		[Test]
		public void ShouldMapNoneWhenNoActivity()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, new PreferenceRestrictionTemplate(), "name", Color.Red);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.Activity.Should().Be("(" + Resources.NoActivity + ")");
		}

		[Test]
		public void ShouldMapActivityStartTimeLimitation()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, new PreferenceRestrictionTemplate(), "name", Color.Red);
			var activityRestriction = new ActivityRestriction(new Activity(" "))
			{
				StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9))
			};
			extendedPreferenceTemplate.Restriction.AddActivityRestriction(activityRestriction);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.ActivityStartTimeLimitation.Should().Be(activityRestriction.StartTimeLimitation.StartTimeString + "-" + activityRestriction.StartTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapActivityEndTimeLimitation()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, new PreferenceRestrictionTemplate(), "name", Color.Red);
			var activityRestriction = new ActivityRestriction(new Activity(" "))
			{
				EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9))
			};
			extendedPreferenceTemplate.Restriction.AddActivityRestriction(activityRestriction);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.ActivityEndTimeLimitation.Should().Be(activityRestriction.EndTimeLimitation.StartTimeString + "-" + activityRestriction.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapActivityWorkTimeLimitation()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, new PreferenceRestrictionTemplate(), "name", Color.Red);
			var activityRestriction = new ActivityRestriction(new Activity(" "))
			{
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			extendedPreferenceTemplate.Restriction.AddActivityRestriction(activityRestriction);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.ActivityTimeLimitation.Should().Be(activityRestriction.WorkTimeLimitation.StartTimeString + "-" + activityRestriction.WorkTimeLimitation.EndTimeString);
		}
	}
}