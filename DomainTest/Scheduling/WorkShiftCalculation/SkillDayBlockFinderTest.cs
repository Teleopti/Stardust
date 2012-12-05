﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class SkillDayBlockFinderTest
    {
        private ISkillDayBlockFinder _target;
        private SchedulingOptions  _schedulingOptions;
        private MockRepository _mock;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _schedulingOptions = new SchedulingOptions();
            
            
        }

        [Test]
        public void FindSkillDayFromBlockUsingPeriod()
        {
            _schedulingOptions.UsePeriodAsBlock = true;
            _target = new SkillDayBlockFinder(_schedulingOptions, _schedulingResultStateHolder);
            var date = DateTime.UtcNow ;
            
            var scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
            var scheduleDateTimePeriod = _mock.StrictMock<IScheduleDateTimePeriod>();
            var dateTimePeriod = new DateTimePeriod(date, date.AddDays(2));

            using (_mock.Record())
            {
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
                Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod);
                Expect.Call(scheduleDateTimePeriod.LoadedPeriod()).Return(dateTimePeriod);
                
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.ExtractSkillDays(date), new List<DateOnly> { new DateOnly(date), new DateOnly(date.AddDays(1)), new DateOnly(date.AddDays(2)) });    
            }
        }

        [Test]
        public void FindSkillDayFromBlockUsingTwoDaysOff()
        {
            _schedulingOptions.UseCalenderWeekAsBlock = false;
            _schedulingOptions.UsePeriodAsBlock = false;
            _schedulingOptions.UseTwoDaysOffAsBlock = true;
            _target = new SkillDayBlockFinder(_schedulingOptions, _schedulingResultStateHolder);
            var date = DateTime.UtcNow;
            
            var scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
            var scheduleDateTimePeriod = _mock.StrictMock<IScheduleDateTimePeriod>();
            var dateTimePeriod = new DateTimePeriod(date, date.AddDays(4));
            var scheduleDay1 = _mock.StrictMock<IScheduleDay>(); 
            var scheduleDayList1 = new List<IScheduleDay>(){scheduleDay1};
            var scheduleDay2 = _mock.StrictMock<IScheduleDay>(); 
            var scheduleDayList2 = new List<IScheduleDay>(){scheduleDay2};
            using (_mock.Record())
            {
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.AtLeastOnce();
                Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod);
                Expect.Call(scheduleDateTimePeriod.LoadedPeriod()).Return(dateTimePeriod);
                Expect.Call(scheduleDictionary.SchedulesForDay(new DateOnly(date))).Return(new List<IScheduleDay>(){});
                Expect.Call(scheduleDictionary.SchedulesForDay(new DateOnly(date.AddDays(1)))).Return(new List<IScheduleDay>(){});
                Expect.Call(scheduleDictionary.SchedulesForDay(new DateOnly(date.AddDays(2)))).Return(scheduleDayList1);
                Expect.Call(scheduleDictionary.SchedulesForDay(new DateOnly(date.AddDays(3)))).Return(scheduleDayList2);
                Expect.Call(scheduleDictionary.SchedulesForDay(new DateOnly(date.AddDays(4)))).Return(new List<IScheduleDay>() { });
                Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
                Expect.Call(scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff);
                
                //Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).Return(new List<ISkillDay> { skillDay1, skillDay2 }).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.ExtractSkillDays(date), new List<DateOnly> { new DateOnly(date), new DateOnly(date.AddDays(1)) });
            }
        }

        [Test]
        public void FindSkillDayFromBlockUsingCalenderWeek()
        {
            _schedulingOptions.UseCalenderWeekAsBlock  = true;
            _schedulingOptions.UsePeriodAsBlock = false;
            _schedulingOptions.UseTwoDaysOffAsBlock = false;
            _target = new SkillDayBlockFinder(_schedulingOptions, _schedulingResultStateHolder);
            var startDate = new DateTime(2012, 11, 1, 0, 0, 0, DateTimeKind.Utc);

            var scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
            var scheduleDateTimePeriod = _mock.StrictMock<IScheduleDateTimePeriod>();
            var dateTimePeriod = new DateTimePeriod(startDate, startDate.AddDays(11));

            using (_mock.Record())
            {
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
                Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod);
                Expect.Call(scheduleDateTimePeriod.LoadedPeriod()).Return(dateTimePeriod);
                
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.ExtractSkillDays(startDate), new List<DateOnly> { new DateOnly(startDate), new DateOnly(startDate.AddDays(1)) });
            }
        }
    }

    
}
