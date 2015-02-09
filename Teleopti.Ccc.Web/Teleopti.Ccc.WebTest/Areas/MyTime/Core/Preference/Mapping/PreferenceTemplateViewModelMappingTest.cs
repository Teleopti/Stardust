using System;
using System.Drawing;
using System.Globalization;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
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
		public void ShouldMapId()
		{
			var extendedPreferenceTemplate = MockRepository.GenerateStub<ExtendedPreferenceTemplate>();
			var id = Guid.NewGuid();
			extendedPreferenceTemplate.Stub(x => x.Id).Return(id);
			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.Value.Should().Be.EqualTo(id.ToString());
		}

		[Test]
		public void ShouldMapName()
		{
			var template = new PreferenceRestrictionTemplate();
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, template, "name", Color.Red);
			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.Text.Should().Be.EqualTo("name");
		}

		[Test]
		public void ShouldMapColor()
		{
			var template = new PreferenceRestrictionTemplate();
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, template, "name", Color.Red);
			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.Color.Should().Be.EqualTo(Color.Red.ToHtml());
		}

		[Test]
		public void ShouldMapShiftCategoryPreference()
		{
			var shiftCategory = MockRepository.GenerateStub<ShiftCategory>();
			var id = Guid.NewGuid();
			shiftCategory.Stub(x => x.Id).Return(id);
			var template = new PreferenceRestrictionTemplate { ShiftCategory = shiftCategory };
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, template, "name", Color.Red);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.PreferenceId.Should().Be(id);
		}

		[Test]
		public void ShouldMapDayOffTemplatePreference()
		{
			var dayoff = MockRepository.GenerateStub<DayOffTemplate>();
			var id = Guid.NewGuid();
			dayoff.Stub(x => x.Id).Return(id);
			var template = new PreferenceRestrictionTemplate { DayOffTemplate = dayoff };
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, template, "name", Color.Red);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.PreferenceId.Should().Be(id);
		}


		[Test]
		public void ShouldMapAbsencePreference()
		{
			var absence = MockRepository.GenerateMock<Absence>();
			var id = Guid.NewGuid();
			absence.Stub(x=>x.Id).Return(id);
			var template = new PreferenceRestrictionTemplate { Absence = absence };
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, template, "name", Color.Red);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.PreferenceId.Should().Be(id);
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

			result.EarliestStartTime.Should().Be(extendedPreferenceTemplate.Restriction.StartTimeLimitation.StartTimeString);
			result.LatestStartTime.Should().Be(extendedPreferenceTemplate.Restriction.StartTimeLimitation.EndTimeString);
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

			result.EarliestEndTime.Should().Be(extendedPreferenceTemplate.Restriction.EndTimeLimitation.StartTimeString);
			result.LatestEndTime.Should().Be(extendedPreferenceTemplate.Restriction.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapEndTimeLimitationNextDay()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(
				null, new PreferenceRestrictionTemplate
				{
					EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(25), TimeSpan.FromHours(27))
				}, "name", Color.Red);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.EarliestEndTime.Should().Be(TimeHelper.TimeOfDayFromTimeSpan(extendedPreferenceTemplate.Restriction.EndTimeLimitation.StartTime.Value.Add(TimeSpan.FromDays(-1)), CultureInfo.CurrentCulture));
			result.EarliestEndTimeNextDay.Should().Be.True();
			result.LatestEndTime.Should().Be(TimeHelper.TimeOfDayFromTimeSpan(extendedPreferenceTemplate.Restriction.EndTimeLimitation.EndTime.Value.Add(TimeSpan.FromDays(-1)), CultureInfo.CurrentCulture));
			result.LatestEndTimeNextDay.Should().Be.True();
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

			result.MinimumWorkTime.Should().Be(extendedPreferenceTemplate.Restriction.WorkTimeLimitation.StartTimeString);
			result.MaximumWorkTime.Should().Be(extendedPreferenceTemplate.Restriction.WorkTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapActivity()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, new PreferenceRestrictionTemplate(), "name", Color.Red);

			var activity = MockRepository.GenerateStub<Activity>();
			var id = Guid.NewGuid();
			activity.Stub(x => x.Id).Return(id);
			extendedPreferenceTemplate.Restriction.AddActivityRestriction(new ActivityRestriction(activity));
			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.ActivityPreferenceId.Should().Be(id);
		}

		[Test]
		public void ShouldMapNoneWhenNoActivity()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, new PreferenceRestrictionTemplate(), "name", Color.Red);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.ActivityPreferenceId.Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldMapActivityStartTimeLimitation()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, new PreferenceRestrictionTemplate(), "name", Color.Red);
			var activityRestriction = new ActivityRestriction(new Activity("a"))
			{
				StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9))
			};
			extendedPreferenceTemplate.Restriction.AddActivityRestriction(activityRestriction);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.ActivityEarliestStartTime.Should().Be(activityRestriction.StartTimeLimitation.StartTimeString);
			result.ActivityLatestStartTime.Should().Be(activityRestriction.StartTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapActivityEndTimeLimitation()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, new PreferenceRestrictionTemplate(), "name", Color.Red);
			var activityRestriction = new ActivityRestriction(new Activity("a"))
			{
				EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(9))
			};
			extendedPreferenceTemplate.Restriction.AddActivityRestriction(activityRestriction);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.ActivityEarliestEndTime.Should().Be(activityRestriction.EndTimeLimitation.StartTimeString);
			result.ActivityLatestEndTime.Should().Be(activityRestriction.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapActivityWorkTimeLimitation()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, new PreferenceRestrictionTemplate(), "name", Color.Red);
			var activityRestriction = new ActivityRestriction(new Activity("a"))
			{
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			extendedPreferenceTemplate.Restriction.AddActivityRestriction(activityRestriction);

			var result = Mapper.Map<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>(extendedPreferenceTemplate);

			result.ActivityMinimumTime.Should().Be(activityRestriction.WorkTimeLimitation.StartTimeString);
			result.ActivityMaximumTime.Should().Be(activityRestriction.WorkTimeLimitation.EndTimeString);
		}
	}
}