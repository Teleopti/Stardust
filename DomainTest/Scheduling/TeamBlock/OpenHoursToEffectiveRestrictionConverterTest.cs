using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class OpenHoursToEffectiveRestrictionConverterTest
	{
		private MockRepository _mocks;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private OpenHoursToEffectiveRestrictionConverter _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_groupPersonSkillAggregator = _mocks.StrictMock<IGroupPersonSkillAggregator>();
			_target = new OpenHoursToEffectiveRestrictionConverter(_schedulingResultStateHolder, _groupPersonSkillAggregator);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldConvertOpenHoursToRestriction()
		{
			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
			var person1 = PersonFactory.CreatePerson("bill");
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
			var skill = SkillFactory.CreateSkill("skill1");
			var skillDay1 = SkillDayFactory.CreateSkillDay(skill, dateOnly.Date,
														   new ReadOnlyCollection<TimePeriod>(new List<TimePeriod>
		                                                       {
			                                                       new TimePeriod(11, 0, 18, 0)
		                                                       }));

			var skillDay2 = SkillDayFactory.CreateSkillDay(skill, dateOnly.AddDays(1).Date,
														   new ReadOnlyCollection<TimePeriod>(new List<TimePeriod>
		                                                       {
			                                                       new TimePeriod(10, 0, 17, 30)
		                                                       }));
			var skillDays = new List<ISkillDay> { skillDay1, skillDay2 };
			
			var scheduleRange1 = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(dateList)).Return(skillDays);
				Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(groupPerson, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)))).Return(new[] { skillDay1.Skill });
				Expect.Call(groupPerson.GroupMembers)
					  .Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person1 }))
					  .Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(scheduleDictionary[person1]).Return(scheduleRange1).Repeat.Twice();
				Expect.Call(scheduleRange1.ScheduledDay(dateOnly)).Return(scheduleDay1);
				Expect.Call(scheduleDay1.SignificantPart())
					  .Return(SchedulePartView.MainShift);
				Expect.Call(scheduleRange1.ScheduledDay(dateOnly.AddDays(1))).Return(scheduleDay2);
				Expect.Call(scheduleDay2.SignificantPart())
					  .Return(SchedulePartView.MainShift);
			}

			using (_mocks.Playback())
			{
				var result = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11), null),
											 new EndTimeLimitation(null, TimeSpan.FromHours(17.5)),
											 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

				var restriction = _target.Convert(groupPerson, dateList);

				Assert.That(restriction, Is.EqualTo(result));
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldAggregateDaysWithoutDayOff()
		{
			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
			var person1 = PersonFactory.CreatePerson("bill");
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
			var skill = SkillFactory.CreateSkill("skill1");
			var skillDay1 = SkillDayFactory.CreateSkillDay(skill, dateOnly.Date,
														   new ReadOnlyCollection<TimePeriod>(new List<TimePeriod>
		                                                       {
			                                                       new TimePeriod(11, 0, 18, 0)
		                                                       }));

			var skillDay2 = SkillDayFactory.CreateSkillDay(skill, dateOnly.AddDays(1).Date,
														   new ReadOnlyCollection<TimePeriod>(new List<TimePeriod>
		                                                       {
			                                                       new TimePeriod(10, 0, 17, 30)
		                                                       }));
			var skillDays = new List<ISkillDay> { skillDay1, skillDay2 };
			var scheduleRange1 = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(dateList)).Return(skillDays);
				Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(groupPerson, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)))).Return(new[] { skillDay1.Skill });
				Expect.Call(groupPerson.GroupMembers)
					  .Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person1 }))
					  .Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(scheduleDictionary[person1]).Return(scheduleRange1).Repeat.Twice();
				Expect.Call(scheduleRange1.ScheduledDay(dateOnly)).Return(scheduleDay1);
				Expect.Call(scheduleDay1.SignificantPart())
					  .Return(SchedulePartView.MainShift);
				Expect.Call(scheduleRange1.ScheduledDay(dateOnly.AddDays(1))).Return(scheduleDay2);
				Expect.Call(scheduleDay2.SignificantPart())
					  .Return(SchedulePartView.DayOff);
			}

			using (_mocks.Playback())
			{
				var result = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11), null),
											 new EndTimeLimitation(null, TimeSpan.FromHours(18)),
											 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

				var restriction = _target.Convert(groupPerson, dateList);

				Assert.That(restriction, Is.EqualTo(result));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldAggregateOpenHoursFromOpenSkills()
		{
			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
			var person1 = PersonFactory.CreatePerson("bill");
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
			var skill = SkillFactory.CreateSkill("skill1");
			var skillDay1 = SkillDayFactory.CreateSkillDay(skill, dateOnly.Date,
														   new ReadOnlyCollection<TimePeriod>(new List<TimePeriod>
		                                                       {
			                                                       new TimePeriod(11, 0, 18, 0)
		                                                       }));

			var skillDay2 = SkillDayFactory.CreateSkillDay(skill, dateOnly.AddDays(1).Date,
														   new ReadOnlyCollection<TimePeriod>(new List<TimePeriod>
		                                                       {
			                                                       new TimePeriod(10, 0, 17, 30)
		                                                       }));
			var skillDay3 = SkillDayFactory.CreateSkillDay(dateOnly.AddDays(2).Date,
														   new ReadOnlyCollection<TimePeriod>(new List<TimePeriod>
				                                               {
					                                               new TimePeriod(13, 0, 15, 30)
				                                               }));
			var skillDays = new List<ISkillDay> { skillDay1, skillDay2, skillDay3 };

			var scheduleRange1 = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(dateList)).Return(skillDays);
				Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(groupPerson, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)))).Return(new[] { skillDay1.Skill });
				Expect.Call(groupPerson.GroupMembers)
					  .Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person1 }))
					  .Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(scheduleDictionary[person1]).Return(scheduleRange1).Repeat.Twice();
				Expect.Call(scheduleRange1.ScheduledDay(dateOnly)).Return(scheduleDay1);
				Expect.Call(scheduleDay1.SignificantPart())
					  .Return(SchedulePartView.MainShift);
				Expect.Call(scheduleRange1.ScheduledDay(dateOnly.AddDays(1))).Return(scheduleDay2);
				Expect.Call(scheduleDay2.SignificantPart())
					  .Return(SchedulePartView.MainShift);
			}

			using (_mocks.Playback())
			{
				var result = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11), null),
											 new EndTimeLimitation(null, TimeSpan.FromHours(17.5)),
											 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

				var restriction = _target.Convert(groupPerson, dateList);

				Assert.That(restriction, Is.EqualTo(result));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldNotAggregateDaysWithoutDayOff()
		{
			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
			var person1 = PersonFactory.CreatePerson("bill");
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
			var skill = SkillFactory.CreateSkill("skill1");
			var skillDay1 = SkillDayFactory.CreateSkillDay(skill, dateOnly.Date,
														   new ReadOnlyCollection<TimePeriod>(new List<TimePeriod>
		                                                       {
			                                                       new TimePeriod(11, 0, 18, 0)
		                                                       }));

			var skillDay2 = SkillDayFactory.CreateSkillDay(skill, dateOnly.AddDays(1).Date,
														   new ReadOnlyCollection<TimePeriod>(new List<TimePeriod>
		                                                       {
			                                                       new TimePeriod(10, 0, 17, 30)
		                                                       }));
			var skillDays = new List<ISkillDay> { skillDay1, skillDay2 };
			var scheduleRange1 = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(dateList)).Return(skillDays);
				Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(groupPerson, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)))).Return(new List<ISkill>());
				Expect.Call(groupPerson.GroupMembers)
					  .Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person1 }))
					  .Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(scheduleDictionary[person1]).Return(scheduleRange1).Repeat.Twice();
				Expect.Call(scheduleRange1.ScheduledDay(dateOnly)).Return(scheduleDay1);
				Expect.Call(scheduleDay1.SignificantPart())
					  .Return(SchedulePartView.MainShift);
				Expect.Call(scheduleRange1.ScheduledDay(dateOnly.AddDays(1))).Return(scheduleDay2);
				Expect.Call(scheduleDay2.SignificantPart())
					  .Return(SchedulePartView.DayOff);
			}

			using (_mocks.Playback())
			{
				var restriction = _target.Convert(groupPerson, dateList);

				Assert.That(restriction, Is.Null);
			}
		}

        [Test]
        public void ShouldNotContinueIfGroupPersonIsNull()
        {
            _target.Convert(null, new List<DateOnly>());
        }

        [Test]
        public void ShouldNotContinueIfDateOnlyListIsNull()
        {
            var groupPerson = _mocks.StrictMock<IGroupPerson>();
            _target.Convert(groupPerson, null);
        }

	}
}
