using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class ScheduleOvertimeServiceTest
	{
		private ScheduleOvertimeService _target;
		private MockRepository _mock;
		private IScheduleDay _scheduleDay;
		private IOvertimePreferences _overtimePreferences;
		private IOvertimeLengthDecider _overtimeLengthDecider;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private DateOnly _dateOnly;
		private INewBusinessRuleCollection _rules;
		private IScheduleTagSetter _scheduleTagSetter;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private TimeZoneInfo _timeZoneInfo;
		private IPerson _person;
		private IPersonPeriod _personPeriod;
		private ISkill _skill;
		private DateTimePeriod _dateTimePeriod;
		private IActivity _activity;
		private ISkillStaffPeriodHolder _skillStaffPeriodHolder;
		private IMultiplicatorDefinitionSet _multiplicatorDefinitionSet;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_overtimePreferences = new OvertimePreferences();
			_overtimeLengthDecider = _mock.StrictMock<IOvertimeLengthDecider>();
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_schedulePartModifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_dateOnly = new DateOnly(2014, 1, 1);
			_rules = NewBusinessRuleCollection.Minimum();
			_scheduleTagSetter = _mock.StrictMock<IScheduleTagSetter>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_target = new ScheduleOvertimeService(_overtimeLengthDecider, _schedulePartModifyAndRollbackService, _schedulingResultStateHolder);
			_timeZoneInfo = TimeZoneInfo.Utc;
			_person = PersonFactory.CreatePerson("person");
			_activity = ActivityFactory.CreateActivity("activity");
			_skill = SkillFactory.CreateSkill("skill");
			_skill.Activity = _activity;
			_personPeriod = PersonPeriodFactory.CreatePersonPeriodWithSkills(_dateOnly, new[] {_skill});
			_person.AddPersonPeriod(_personPeriod);
			_dateTimePeriod = new DateTimePeriod(2014, 1, 1, 2014, 1, 2);
			_overtimePreferences.SkillActivity = _activity;
			_skillStaffPeriodHolder = _mock.StrictMock<ISkillStaffPeriodHolder>();
			_multiplicatorDefinitionSet = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			_personPeriod.PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(_multiplicatorDefinitionSet);
		}

		[Test,]
		public void ShouldSchedulePersonOnDay()
		{
			var period = new DateTimePeriod(2014, 1, 1, 8, 2014, 1, 1, 10);
			var skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period, new Task(), new ServiceAgreement());
			IList<ISkillStaffPeriod> skillStaffPeriods = new List<ISkillStaffPeriod>{skillStaffPeriod};

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_overtimeLengthDecider.Decide(_person, _dateOnly, _scheduleDay, _activity, new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero), new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero), false)).Return(new List<DateTimePeriod> {_dateTimePeriod});
				Expect.Call(() => _scheduleDay.CreateAndAddOvertime(_activity, _dateTimePeriod, null));
				Expect.Call(_schedulePartModifyAndRollbackService.ClearModificationCollection);
				Expect.Call((()=>_schedulePartModifyAndRollbackService.ModifyStrictly(_scheduleDay, _scheduleTagSetter, _rules)));
				Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, false)).Return(true);
				Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly.AddDays(1), null, false)).Return(true);
				Expect.Call(_schedulingResultStateHolder.SkillStaffPeriodHolder).Return(_skillStaffPeriodHolder).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skill, _dateTimePeriod)).Return(skillStaffPeriods).Repeat.AtLeastOnce().IgnoreArguments();
			}

			using (_mock.Playback())
			{
				var result = _target.SchedulePersonOnDay(_scheduleDay, _overtimePreferences, _resourceCalculateDelayer, _dateOnly, _rules, _scheduleTagSetter, _timeZoneInfo);
				Assert.IsTrue(result);
				Assert.AreEqual(4, _rules.Count);
			}
		}

		[Test]
		public void ShouldSkipWhenNoSuitablePeriods()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_overtimeLengthDecider.Decide(_person, _dateOnly, _scheduleDay, _activity, new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero), new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero), false)).Return(new List<DateTimePeriod>());
			}

			using (_mock.Playback())
			{
				var res = _target.SchedulePersonOnDay(_scheduleDay, _overtimePreferences, _resourceCalculateDelayer, _dateOnly, _rules, _scheduleTagSetter, _timeZoneInfo);
				Assert.IsFalse(res);
			}	
		}

		[Test]
		public void ShouldSkipWhenNoMultiplicatorDefinitionSetOfTypeOvertime()
		{
			_personPeriod.PersonContract.Contract.RemoveMultiplicatorDefinitionSetCollection(_multiplicatorDefinitionSet);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
			}

			using (_mock.Playback())
			{
				var res = _target.SchedulePersonOnDay(_scheduleDay, _overtimePreferences, _resourceCalculateDelayer, _dateOnly, _rules, _scheduleTagSetter, _timeZoneInfo);
				Assert.IsFalse(res);
			}
		}

		[Test]
		public void ShouldSkipWhenNoPersonPeriod()
		{
			_person.RemoveAllPersonPeriods();

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
			}

			using (_mock.Playback())
			{
				var res = _target.SchedulePersonOnDay(_scheduleDay, _overtimePreferences, _resourceCalculateDelayer, _dateOnly, _rules, _scheduleTagSetter, _timeZoneInfo);
				Assert.IsFalse(res);
			}	
		}

		[Test]
		public void ShouldSkipWhenNoPersonContract()
		{
			_person.PersonPeriodCollection[0].PersonContract = null;

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
			}

			using (_mock.Playback())
			{
				var result = _target.SchedulePersonOnDay(_scheduleDay, _overtimePreferences, _resourceCalculateDelayer, _dateOnly, _rules, _scheduleTagSetter, _timeZoneInfo);
				Assert.IsFalse(result);
			}		
		}

		[Test]
		public void ShouldRollbackWhenRmsGetsHigher()
		{
			var period = new DateTimePeriod(2014, 1, 1, 8, 2014, 1, 1, 10);
			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period, new Task(), new ServiceAgreement());
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period, new Task(), new ServiceAgreement());
			skillStaffPeriod2.SetCalculatedResource65(10);
			IList<ISkillStaffPeriod> skillStaffPeriods1 = new List<ISkillStaffPeriod> { skillStaffPeriod1 };
			IList<ISkillStaffPeriod> skillStaffPeriods2 = new List<ISkillStaffPeriod> { skillStaffPeriod2 };

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_overtimeLengthDecider.Decide(_person, _dateOnly, _scheduleDay, _activity, new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero), new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero), false)).Return(new List<DateTimePeriod> { _dateTimePeriod });
				Expect.Call(() => _scheduleDay.CreateAndAddOvertime(_activity, _dateTimePeriod, null));
				Expect.Call(_schedulePartModifyAndRollbackService.ClearModificationCollection);
				Expect.Call((() => _schedulePartModifyAndRollbackService.ModifyStrictly(_scheduleDay, _scheduleTagSetter, _rules)));
				Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, false)).Return(true).Repeat.Twice();
				Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly.AddDays(1), null, false)).Return(true).Repeat.Twice();
				Expect.Call(_schedulingResultStateHolder.SkillStaffPeriodHolder).Return(_skillStaffPeriodHolder).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skill, _dateTimePeriod)).Return(skillStaffPeriods1).Repeat.Twice().IgnoreArguments();
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skill, _dateTimePeriod)).Return(skillStaffPeriods2).Repeat.Twice().IgnoreArguments();
				Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
			}

			using (_mock.Playback())
			{
				var result = _target.SchedulePersonOnDay(_scheduleDay, _overtimePreferences, _resourceCalculateDelayer, _dateOnly, _rules, _scheduleTagSetter, _timeZoneInfo);
				Assert.IsFalse(result);
			}	
		}
	}
}
