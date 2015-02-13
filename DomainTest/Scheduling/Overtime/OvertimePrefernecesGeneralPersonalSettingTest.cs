using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class OvertimePrefernecesGeneralPersonalSettingTest
	{
		private OvertimePreferencesGeneralPersonalSetting _target;
		private IScheduleTag _scheduleTag;
		private IActivity _activity;
		private IMultiplicatorDefinitionSet _definitionSet;

		[SetUp]
		public void Setup()
		{
			_activity = new Activity("test");
			_target = new OvertimePreferencesGeneralPersonalSetting();
			_scheduleTag = new ScheduleTag();
			_definitionSet = new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime);
		}

		[Test]
		public void ShouldMapAllTheProperties()
		{
			var scheduleTags = new List<IScheduleTag> {_scheduleTag};

			var overtimePreferences = new OvertimePreferences
			{
				ScheduleTag = _scheduleTag,
				SkillActivity = _activity,
				AllowBreakMaxWorkPerWeek = true,
				AllowBreakNightlyRest = true,
				AllowBreakWeeklyRest = false,
				AvailableAgentsOnly = true,
				ExtendExistingShift = true,
				SelectedTimePeriod = new TimePeriod(TimeSpan.FromHours(10), TimeSpan.FromHours(12)),
				OvertimeType = _definitionSet
			};

			_target.MapFrom(overtimePreferences);

			IOvertimePreferences targetOvertimePreferences = new OvertimePreferences();
			_target.MapTo(targetOvertimePreferences, scheduleTags, new List<IActivity> {_activity},
				new List<IMultiplicatorDefinitionSet> {_definitionSet});

			Assert.AreEqual(targetOvertimePreferences.ScheduleTag, _scheduleTag);
			Assert.AreEqual(targetOvertimePreferences.SkillActivity, _activity);
			Assert.IsTrue(targetOvertimePreferences.AllowBreakMaxWorkPerWeek);
			Assert.IsTrue(targetOvertimePreferences.AllowBreakNightlyRest);
			Assert.IsFalse(targetOvertimePreferences.AllowBreakWeeklyRest);
			Assert.IsTrue(targetOvertimePreferences.ExtendExistingShift);
			Assert.IsTrue(targetOvertimePreferences.AvailableAgentsOnly);
			Assert.AreEqual(targetOvertimePreferences.SelectedTimePeriod,
				new TimePeriod(TimeSpan.FromHours(10), TimeSpan.FromHours(12)));
			Assert.AreEqual(targetOvertimePreferences.OvertimeType, _definitionSet);

		}

		[Test]
		public void ShouldMapToDefaultIfNotInPersonalSetting()
		{
			IOvertimePreferences preferences = new OvertimePreferences();
			var scheduleTags = new List<IScheduleTag> {_scheduleTag};
			_target.MapTo(preferences, scheduleTags, new List<IActivity> {_activity},
				new List<IMultiplicatorDefinitionSet> {_definitionSet});

			Assert.AreEqual(_activity, preferences.SkillActivity);
			Assert.AreEqual(_definitionSet, preferences.OvertimeType);
			Assert.AreEqual(new TimePeriod(TimeSpan.FromHours(1), TimeSpan.FromHours(1)), preferences.SelectedTimePeriod);
		}
	}
}
