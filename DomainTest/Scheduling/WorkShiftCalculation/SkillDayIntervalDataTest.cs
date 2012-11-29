﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class SkillDayIntervalDataTest
    {
        private ISkillDayPeriodIntervalData _target;
        private MockRepository _mock;
        private ISkillDay _skillDay1;
        private ISkillDay _skillDay2;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _skillDay1 = _mock.StrictMock<ISkillDay>();
            _skillDay2 = _mock.StrictMock<ISkillDay>();
            var skillDayList = new List<ISkillDay>() {_skillDay1, _skillDay2};
            _target = new SkillDayPeriodPeriodIntervalData(skillDayList);
        }

        [Test]
        public void ShouldCreateIntervalsFromSkillDay()
        {
            var today = new DateTime(2012,11,28,0,0,0,DateTimeKind.Utc );
            var result = new Dictionary<TimeSpan, ISkillIntervalData>();
            result.Add(today.AddMinutes(15).TimeOfDay , new SkillIntervalData(6.0, 0, 0, 0));
            result.Add(today.AddMinutes(30).TimeOfDay, new SkillIntervalData(5.5, 0, 0, 0));
            result.Add(today.AddMinutes(45).TimeOfDay, new SkillIntervalData(1.0, 0, 0, 0));
            result.Add(today.AddMinutes(60).TimeOfDay, new SkillIntervalData(3.5, 0, 0, 0));

            var skillstaffperiod1 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod2 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod3 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod4 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod5 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod6 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod7 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod8 = _mock.StrictMock<ISkillStaffPeriod>();

            using (_mock.Record())
            {
                Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>() 
                                    { skillstaffperiod1, skillstaffperiod2, skillstaffperiod3, skillstaffperiod4}));
                Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>() 
                                    { skillstaffperiod5, skillstaffperiod6, skillstaffperiod7, skillstaffperiod8 }));

                Expect.Call(skillstaffperiod1.Period).Return(new DateTimePeriod(today.AddMinutes(15), today.AddMinutes(30))).Repeat.AtLeastOnce() ;
                Expect.Call(skillstaffperiod1.AbsoluteDifference).Return(4);

                Expect.Call(skillstaffperiod2.Period).Return(new DateTimePeriod(today.AddMinutes(30), today.AddMinutes(45))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod2.AbsoluteDifference).Return(10);

                Expect.Call(skillstaffperiod3.Period).Return(new DateTimePeriod(today.AddMinutes(45), today.AddMinutes(60))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod3.AbsoluteDifference).Return(1);

                Expect.Call(skillstaffperiod4.Period).Return(new DateTimePeriod(today.AddMinutes(60), today.AddMinutes(75))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod4.AbsoluteDifference).Return(2);

                var tomorrow = today.AddDays(1);

                Expect.Call(skillstaffperiod5.Period).Return(new DateTimePeriod(tomorrow.AddMinutes(15), tomorrow.AddMinutes(30))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod5.AbsoluteDifference).Return(8);

                Expect.Call(skillstaffperiod6.Period).Return(new DateTimePeriod(tomorrow.AddMinutes(30), tomorrow.AddMinutes(45))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod6.AbsoluteDifference).Return(1);

                Expect.Call(skillstaffperiod7.Period).Return(new DateTimePeriod(tomorrow.AddMinutes(45), tomorrow.AddMinutes(60))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod7.AbsoluteDifference).Return(1);

                Expect.Call(skillstaffperiod8.Period).Return(new DateTimePeriod(tomorrow.AddMinutes(60), tomorrow.AddMinutes(75))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod8.AbsoluteDifference).Return(5);
            }

            var calculatedResult = _target.GetIntervalDistribution();
            Assert.AreEqual(result[today.AddMinutes(15).TimeOfDay].CurrentDemand , calculatedResult[today.AddMinutes(15).TimeOfDay].CurrentDemand );
            Assert.AreEqual(result[today.AddMinutes(30).TimeOfDay].CurrentDemand, calculatedResult[today.AddMinutes(30).TimeOfDay].CurrentDemand);
            Assert.AreEqual(result[today.AddMinutes(45).TimeOfDay].CurrentDemand, calculatedResult[today.AddMinutes(45).TimeOfDay].CurrentDemand);
            Assert.AreEqual(result[today.AddMinutes(60).TimeOfDay].CurrentDemand, calculatedResult[today.AddMinutes(60).TimeOfDay].CurrentDemand);
        }

        [Test]
        public void ShouldCreateIntervalsFromSkillDayWithNullValues()
        {
            var today = new DateTime(2012, 11, 28, 0, 0, 0, DateTimeKind.Utc);
            var result = new Dictionary<TimeSpan, ISkillIntervalData>();
            result.Add(today.AddMinutes(15).TimeOfDay, new SkillIntervalData(6.0, 0, 0, 0));
            result.Add(today.AddMinutes(30).TimeOfDay, new SkillIntervalData(5.5, 0, 0, 0));
            result.Add(today.AddMinutes(45).TimeOfDay, new SkillIntervalData(1.0, 0, 0, 0));
            result.Add(today.AddMinutes(60).TimeOfDay, new SkillIntervalData(2, 0, 0, 0));

            var skillstaffperiod1 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod2 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod3 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod4 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod5 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod6 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod7 = _mock.StrictMock<ISkillStaffPeriod>();

            using (_mock.Record())
            {
                Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>() { skillstaffperiod1, skillstaffperiod2, skillstaffperiod3, skillstaffperiod4 }));
                Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>() { skillstaffperiod5, skillstaffperiod6, skillstaffperiod7 }));

                Expect.Call(skillstaffperiod1.Period).Return(new DateTimePeriod(today.AddMinutes(15), today.AddMinutes(30))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod1.AbsoluteDifference).Return(4);

                Expect.Call(skillstaffperiod2.Period).Return(new DateTimePeriod(today.AddMinutes(30), today.AddMinutes(45))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod2.AbsoluteDifference).Return(10);

                Expect.Call(skillstaffperiod3.Period).Return(new DateTimePeriod(today.AddMinutes(45), today.AddMinutes(60))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod3.AbsoluteDifference).Return(1);

                Expect.Call(skillstaffperiod4.Period).Return(new DateTimePeriod(today.AddMinutes(60), today.AddMinutes(75))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod4.AbsoluteDifference).Return(2);

                var tomorrow = today.AddDays(1);

                Expect.Call(skillstaffperiod5.Period).Return(new DateTimePeriod(tomorrow.AddMinutes(15), tomorrow.AddMinutes(30))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod5.AbsoluteDifference).Return(8);

                Expect.Call(skillstaffperiod6.Period).Return(new DateTimePeriod(tomorrow.AddMinutes(30), tomorrow.AddMinutes(45))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod6.AbsoluteDifference).Return(1);

                Expect.Call(skillstaffperiod7.Period).Return(new DateTimePeriod(tomorrow.AddMinutes(45), tomorrow.AddMinutes(60))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod7.AbsoluteDifference).Return(1);

                //Expect.Call(skillstaffperiod8.Period).Return(new DateTimePeriod(tomorrow.AddMinutes(60), tomorrow.AddMinutes(75))).Repeat.AtLeastOnce();
                //Expect.Call(skillstaffperiod8.AbsoluteDifference).Return(5);
            }

            var calculatedResult = _target.GetIntervalDistribution();
            Assert.AreEqual(result[today.AddMinutes(15).TimeOfDay].CurrentDemand, calculatedResult[today.AddMinutes(15).TimeOfDay].CurrentDemand);
            Assert.AreEqual(result[today.AddMinutes(30).TimeOfDay].CurrentDemand, calculatedResult[today.AddMinutes(30).TimeOfDay].CurrentDemand);
            Assert.AreEqual(result[today.AddMinutes(45).TimeOfDay].CurrentDemand, calculatedResult[today.AddMinutes(45).TimeOfDay].CurrentDemand);
            Assert.AreEqual(result[today.AddMinutes(60).TimeOfDay].CurrentDemand, calculatedResult[today.AddMinutes(60).TimeOfDay].CurrentDemand);
        }

    }

    
}
