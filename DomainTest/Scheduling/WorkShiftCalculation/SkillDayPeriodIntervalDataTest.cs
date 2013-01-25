using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
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
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private List<ISkillDay> _skillDayList;
    	private ISkillIntervalDataSkillFactorApplyer _factorApplyer;

    	[SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _skillDay1 = _mock.StrictMock<ISkillDay>();
            _skillDay2 = _mock.StrictMock<ISkillDay>();
            _skillDayList = new List<ISkillDay>() {_skillDay1, _skillDay2};
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
        	_factorApplyer = _mock.StrictMock<ISkillIntervalDataSkillFactorApplyer>();
            _target = new SkillDayPeriodIntervalData(_factorApplyer, new IntervalDataMedianCalculator(), _schedulingResultStateHolder);
        }


        [Test]
        public void ShouldCreateIntervalsFromSkillDay()
        {
            var date = DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date, DateTimeKind.Utc);
        	var skillIntervalData1 = new SkillIntervalData(new DateTimePeriod(date, date.AddMinutes(15)), 6.0, 0, 0, 0, 0);
        	var skillIntervalData2 = new SkillIntervalData(new DateTimePeriod(date, date.AddMinutes(15)), 12.0, 0, 0, 0, 0);
            var activity1 = ActivityFactory.CreateActivity("phone1");
            var activity2 = ActivityFactory.CreateActivity("phone2");

			var intervalData1 = new Dictionary<TimeSpan, ISkillIntervalData>
                             {
                                 {
                                     date.TimeOfDay,
                                     skillIntervalData1
                                     }
                             };	
			var intervalData2 = new Dictionary<TimeSpan, ISkillIntervalData>
                             {
                                 {
                                     date.TimeOfDay,
                                     skillIntervalData2
                                     }
                             };
        	var activityIntervalData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>
        	                           	{
        	                           		{activity1, intervalData1},
											{activity2, intervalData2}
        	                           	};

            var skillstaffperiod1 = _mock.StrictMock<ISkillStaffPeriod>();
            var skillstaffperiod2 = _mock.StrictMock<ISkillStaffPeriod>();

        	var skill1 = SkillFactory.CreateSkill("skill1");
        	skill1.Activity = activity1;
        	skill1.DefaultResolution = 15;
			var skill2 = SkillFactory.CreateSkill("skill2");
        	skill2.Activity = activity2;
        	skill2.DefaultResolution = 15;
        	
            using (_mock.Record())
            {
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
                    Return(_skillDayList);
                Expect.Call(_skillDay1.Skill).Return(skill1).Repeat.AtLeastOnce();
            	Expect.Call(_skillDay2.Skill).Return(skill2).Repeat.AtLeastOnce();
            	Expect.Call(_factorApplyer.ApplyFactors(new SkillIntervalData(new DateTimePeriod(), 0, 0, 0, null, null),
            	                                        skill1)).IgnoreArguments().Return(skillIntervalData1).Repeat.Times(192);
				Expect.Call(_factorApplyer.ApplyFactors(new SkillIntervalData(new DateTimePeriod(), 0, 0, 0, null, null),
            	                                        skill2)).IgnoreArguments().Return(skillIntervalData2).Repeat.Times(192);
                Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillstaffperiod1})).Repeat.AtLeastOnce();
                Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillstaffperiod2})).Repeat.AtLeastOnce();

                Expect.Call(skillstaffperiod1.Period).Return(new DateTimePeriod(date.AddMinutes(0), date.AddMinutes(15))).Repeat.AtLeastOnce();
            	Expect.Call(skillstaffperiod1.AbsoluteDifference).Return(4);

				Expect.Call(skillstaffperiod2.Period).Return(new DateTimePeriod(date.AddMinutes(0), date.AddMinutes(15))).Repeat.AtLeastOnce();
                Expect.Call(skillstaffperiod2.AbsoluteDifference).Return(8);
            }
            using(_mock.Playback())
            {
				var calculatedResult = _target.GetIntervalDistribution(new List<DateOnly> { new DateOnly(date),  new DateOnly(date.AddDays(1)) });
            	Assert.That(activityIntervalData.Count, Is.EqualTo(2));
				Assert.That(calculatedResult[activity1][date.TimeOfDay].ForecastedDemand, Is.EqualTo(activityIntervalData[activity1][date.TimeOfDay].ForecastedDemand));
                Assert.That(calculatedResult[activity2][date.TimeOfDay].ForecastedDemand, Is.EqualTo(activityIntervalData[activity2][date.TimeOfDay].ForecastedDemand));
            }
        }
    }
}
