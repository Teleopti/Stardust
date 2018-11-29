using System;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceTemplateViewModelMappingTest
	{
		private ExtendedPreferenceTemplateMapper target;

		[SetUp]
		public void Setup()
		{
			target = new ExtendedPreferenceTemplateMapper();
		}

		[Test]
		public void ShouldMapId()
		{
			var template = new PreferenceRestrictionTemplate();
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, template, "name", Color.Red).WithId();
			
			var result = target.Map(extendedPreferenceTemplate);

			result.Value.Should().Be.EqualTo(extendedPreferenceTemplate.Id.ToString());
		}

		[Test]
		public void ShouldMapName()
		{
			var template = new PreferenceRestrictionTemplate();
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, template, "name", Color.Red);
			var result = target.Map(extendedPreferenceTemplate);

			result.Text.Should().Be.EqualTo("name");
		}

		[Test]
		public void ShouldMapColor()
		{
			var template = new PreferenceRestrictionTemplate();
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, template, "name", Color.Red);
			var result = target.Map(extendedPreferenceTemplate);

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

			var result = target.Map(extendedPreferenceTemplate);

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

			var result = target.Map(extendedPreferenceTemplate);

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

			var result = target.Map(extendedPreferenceTemplate);

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

			var result = target.Map(extendedPreferenceTemplate);

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

			var result = target.Map(extendedPreferenceTemplate);

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

			var result = target.Map(extendedPreferenceTemplate);

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

			var result = target.Map(extendedPreferenceTemplate);

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
			var result = target.Map(extendedPreferenceTemplate);

			result.ActivityPreferenceId.Should().Be(id);
		}

		[Test]
		public void ShouldMapNoneWhenNoActivity()
		{
			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(null, new PreferenceRestrictionTemplate(), "name", Color.Red);

			var result = target.Map(extendedPreferenceTemplate);

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

			var result = target.Map(extendedPreferenceTemplate);

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

			var result = target.Map(extendedPreferenceTemplate);

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

			var result = target.Map(extendedPreferenceTemplate);

			result.ActivityMinimumTime.Should().Be(activityRestriction.WorkTimeLimitation.StartTimeString);
			result.ActivityMaximumTime.Should().Be(activityRestriction.WorkTimeLimitation.EndTimeString);
		}
	}
}