using System;
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
    public class SkillDayPeriodIntervalDataTest
    {
        private ISkillDayPeriodIntervalData _target;
        private MockRepository _mock;
        private ISkillDay _skillDay1;
        private ISkillDay _skillDay2;
        private ISchedulingResultStateHolder _schedulingResultStateHolder  ;
        private List<ISkillDay> _skillDayList;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _skillDay1 = _mock.StrictMock<ISkillDay>();
            _skillDay2 = _mock.StrictMock<ISkillDay>();
            _skillDayList = new List<ISkillDay>() {_skillDay1, _skillDay2};
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _target = new SkillDayPeriodIntervalData(new IntervalDataMedianCalculator(), _schedulingResultStateHolder);
        }


        [Test]
        public void ShouldCreateIntervalsFromSkillDay()
        {
            var date = DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date, DateTimeKind.Utc);
            var result = new Dictionary<TimeSpan, ISkillIntervalData>
                             {
                                 {
                                     date.TimeOfDay,
                                     new SkillIntervalData(new DateTimePeriod(date, date.AddMinutes(15)), 6.0, 0, 0, 0,0)
                                     },
                                 {
                                     date.AddMinutes(15).TimeOfDay,
                                     new SkillIntervalData(new DateTimePeriod(date.AddMinutes(15), date.AddMinutes(30)), 5.5, 0, 0, 0, 0)},
                                 {
                                     date.AddMinutes(30).TimeOfDay,
                                     new SkillIntervalData(new DateTimePeriod(date.AddMinutes(30), date.AddMinutes(45)), 1.0, 0, 0, 0, 0)},
                                 {
                                     date.AddMinutes(45).TimeOfDay,
                                     new SkillIntervalData(new DateTimePeriod(date.AddMinutes(45), date.AddMinutes(60)), 3.5, 0, 0, 0, 0)}
                             };

            var skillstaffperiod1 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod2 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod3 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod4 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod5 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod6 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod7 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod8 = _mock.StrictMock<ISkillStaffPeriod>();

            var skill = _mock.StrictMock<ISkill>();

            using (_mock.Record())
            {
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
                    Return(_skillDayList);
                Expect.Call(_skillDay1.Skill).Return(skill);
                Expect.Call(skill.DefaultResolution).Return(15);
                Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>() { skillstaffperiod1, skillstaffperiod2, skillstaffperiod3, skillstaffperiod4 }));
                Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>() { skillstaffperiod5, skillstaffperiod6, skillstaffperiod7, skillstaffperiod8 }));

                Expect.Call(skillstaffperiod1.Period).Return(new DateTimePeriod(date.AddMinutes(0), date.AddMinutes(15))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod1.AbsoluteDifference).Return(4);

                Expect.Call(skillstaffperiod2.Period).Return(new DateTimePeriod(date.AddMinutes(15), date.AddMinutes(30))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod2.AbsoluteDifference).Return(10);

                Expect.Call(skillstaffperiod3.Period).Return(new DateTimePeriod(date.AddMinutes(30), date.AddMinutes(45))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod3.AbsoluteDifference).Return(1);

                Expect.Call(skillstaffperiod4.Period).Return(new DateTimePeriod(date.AddMinutes(45), date.AddMinutes(60))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod4.AbsoluteDifference).Return(2);

                var tomorrow = date.AddDays(1);

                Expect.Call(skillstaffperiod5.Period).Return(new DateTimePeriod(tomorrow.AddMinutes(0), tomorrow.AddMinutes(15))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod5.AbsoluteDifference).Return(8);

                Expect.Call(skillstaffperiod6.Period).Return(new DateTimePeriod(tomorrow.AddMinutes(15), tomorrow.AddMinutes(30))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod6.AbsoluteDifference).Return(1);

                Expect.Call(skillstaffperiod7.Period).Return(new DateTimePeriod(tomorrow.AddMinutes(30), tomorrow.AddMinutes(45))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod7.AbsoluteDifference).Return(1);

                Expect.Call(skillstaffperiod8.Period).Return(new DateTimePeriod(tomorrow.AddMinutes(45), tomorrow.AddMinutes(60))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod8.AbsoluteDifference).Return(5);
            }
            using(_mock.Playback())
            {
                var calculatedResult = _target.GetIntervalDistribution(new List<DateOnly>( ));

                Assert.AreEqual(result[date.TimeOfDay].ForecastedDemand, calculatedResult[date.TimeOfDay].ForecastedDemand);
                Assert.AreEqual(result[date.AddMinutes(15).TimeOfDay].ForecastedDemand, calculatedResult[date.AddMinutes(15).TimeOfDay].ForecastedDemand);
                Assert.AreEqual(result[date.AddMinutes(30).TimeOfDay].ForecastedDemand, calculatedResult[date.AddMinutes(30).TimeOfDay].ForecastedDemand);
                Assert.AreEqual(result[date.AddMinutes(45).TimeOfDay].ForecastedDemand, calculatedResult[date.AddMinutes(45).TimeOfDay].ForecastedDemand);
            }
        }
        
    }

    
}
