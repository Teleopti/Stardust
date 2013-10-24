﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class OpenHourRestrictionForTeamBlockTest
    {
        private BaseLineData _baseLineData;
        private IOpenHourRestrictionForTeamBlock _target;
        private MockRepository _mock;
        private ISchedulingResultStateHolder _scheduleResultStartHolder;
        private ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;
        private ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodMapper;
        private ISkillDay _skillDay1;
        private ISkillDay _skillDay2;
        private ISkillDay _skillDay3;
        private ISkillDay _skillDay4;
        private ISkillDay _skillDay5;
        private ISkillDay _skillDay6;
        private ITeamBlockInfo _teamBlockInfo;
        private ISkill _skill1;
        private ISkill _skill2;
        private readonly DateOnly _today = new DateOnly(2013,10,17);
        private ISkillStaffPeriod _sampleSkillStaffPeriod;

        [SetUp]
        public void Setup()
        {
            _baseLineData = new BaseLineData(_today);
            _mock = new MockRepository();

            _scheduleResultStartHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _skillIntervalDataOpenHour = new SkillIntervalDataOpenHour();
            _skillStaffPeriodMapper = _mock.StrictMock<ISkillStaffPeriodToSkillIntervalDataMapper>();
            _skillDay1 = _mock.StrictMock<ISkillDay>();
            _skillDay2 = _mock.StrictMock<ISkillDay>();
            _skillDay3 = _mock.StrictMock<ISkillDay>();
            _skillDay4 = _mock.StrictMock<ISkillDay>();
            _skillDay5 = _mock.StrictMock<ISkillDay>();
            _skillDay6 = _mock.StrictMock<ISkillDay>();
            _teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
            _skill1 = _mock.StrictMock<ISkill>();
            _skill2 = _mock.StrictMock<ISkill>();
            _sampleSkillStaffPeriod = _mock.StrictMock<ISkillStaffPeriod>();
            _target = new OpenHourRestrictionForTeamBlock(_scheduleResultStartHolder, _skillIntervalDataOpenHour, _skillStaffPeriodMapper);

        }

        private void expectCallForDay(IList<ISkillIntervalData> dayInterval, ISkillDay  skillDay, ISkill skill, IActivity activity)
        {
            var readOnlyListOfSkillInterval =
                   new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>{_sampleSkillStaffPeriod});
            Expect.Call(skillDay.Skill).Return(skill);
            Expect.Call(skill.Activity).Return(activity);
            Expect.Call(skillDay.SkillStaffPeriodCollection).Return(readOnlyListOfSkillInterval).IgnoreArguments().Repeat.AtLeastOnce();
            Expect.Call(_skillStaffPeriodMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>())).IgnoreArguments()
                  .Return(dayInterval);
        }


        [Test]
        public void DifferentOpenHourForBlock()
        {
           
            var day1Interval = generateIntervalForDay(new DateOnly(2013, 10, 17), 8, 11);
            var day2Interval = generateIntervalForDay(new DateOnly(2013, 10, 18), 9, 11);
            var day3Interval = generateIntervalForDay(new DateOnly(2013, 10, 19), 9, 10);

            var skillIntervalList = new List<ISkillIntervalData>();
            skillIntervalList.AddRange(day1Interval);
            skillIntervalList.AddRange(day2Interval);
            skillIntervalList.AddRange(day3Interval);
            IList<ISkillDay> skillDays = new List<ISkillDay> {_skillDay1, _skillDay2, _skillDay3};
           

            using (_mock.Record() )
            {
                Expect.Call(_teamBlockInfo.BlockInfo).Return(_baseLineData.BlockOfThreeDays);
                Expect.Call(
                    _scheduleResultStartHolder.SkillDaysOnDateOnly(
                        _baseLineData.BlockOfThreeDays.BlockPeriod.DayCollection())).Return(skillDays);
                
                expectCallForDay(day1Interval,_skillDay1,_skill1,_baseLineData.Activity1  );
                expectCallForDay(day2Interval,_skillDay2,_skill1,_baseLineData.Activity1  );
                expectCallForDay(day3Interval,_skillDay3,_skill1,_baseLineData.Activity1  );
                  
            }
            var result = _target.GetOpenHoursPerActivity( _teamBlockInfo);
            Assert.AreEqual(result.Count(),1);
            Assert.AreEqual(result[_baseLineData.Activity1].StartTime, TimeSpan.FromHours(9 ));
            Assert.AreEqual(result[_baseLineData.Activity1].EndTime,TimeSpan.FromHours(10 ));
            
        }

        [Test]
        public void SameOpenHourForBlock()
        {

            var day1Interval = generateIntervalForDay(new DateOnly(2013, 10, 17), 8, 11);
            var day2Interval = generateIntervalForDay(new DateOnly(2013, 10, 18), 8, 11);
            var day3Interval = generateIntervalForDay(new DateOnly(2013, 10, 19), 8, 11);

            var skillIntervalList = new List<ISkillIntervalData>();
            skillIntervalList.AddRange(day1Interval);
            skillIntervalList.AddRange(day2Interval);
            skillIntervalList.AddRange(day3Interval);
            IList<ISkillDay> skillDays = new List<ISkillDay> { _skillDay1, _skillDay2, _skillDay3 };
            
            using (_mock.Record())
            {
                Expect.Call(_teamBlockInfo.BlockInfo).Return(_baseLineData.BlockOfThreeDays);
                Expect.Call(
                    _scheduleResultStartHolder.SkillDaysOnDateOnly(
                        _baseLineData.BlockOfThreeDays.BlockPeriod.DayCollection())).Return(skillDays);

                expectCallForDay(day1Interval, _skillDay1, _skill1, _baseLineData.Activity1);
                expectCallForDay(day2Interval, _skillDay2, _skill1, _baseLineData.Activity1);
                expectCallForDay(day3Interval, _skillDay3, _skill1, _baseLineData.Activity1);
                
            }
            var result = _target.GetOpenHoursPerActivity(_teamBlockInfo);
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result[_baseLineData.Activity1].StartTime, TimeSpan.FromHours(8));
            Assert.AreEqual(result[_baseLineData.Activity1].EndTime, TimeSpan.FromHours(11));

        }

        [Test]
        public void OpenHourForBlockTwoActivities()
        {

            var day1Interval = generateIntervalForDay(new DateOnly(2013, 10, 17), 8, 12);
            var day2Interval = generateIntervalForDay(new DateOnly(2013, 10, 18), 9, 11);
            var day3Interval = generateIntervalForDay(new DateOnly(2013, 10, 19), 9, 10);
            var day4Interval = generateIntervalForDay(new DateOnly(2013, 10, 17), 9, 11);
            var day5Interval = generateIntervalForDay(new DateOnly(2013, 10, 18), 8, 12);
            var day6Interval = generateIntervalForDay(new DateOnly(2013, 10, 19), 8, 11);

            var skillIntervalList = new List<ISkillIntervalData>();
            skillIntervalList.AddRange(day1Interval);
            skillIntervalList.AddRange(day2Interval);
            skillIntervalList.AddRange(day3Interval);
            IList<ISkillDay> skillDays = new List<ISkillDay> { _skillDay1, _skillDay2, _skillDay3, _skillDay4, _skillDay5, _skillDay6 };
            
            using (_mock.Record())
            {
                Expect.Call(_teamBlockInfo.BlockInfo).Return(_baseLineData.BlockOfThreeDays);
                Expect.Call(
                    _scheduleResultStartHolder.SkillDaysOnDateOnly(
                        _baseLineData.BlockOfThreeDays.BlockPeriod.DayCollection())).Return(skillDays);
                
                expectCallForDay(day1Interval, _skillDay1, _skill1, _baseLineData.Activity1);
                expectCallForDay(day2Interval, _skillDay2, _skill1, _baseLineData.Activity1);
                expectCallForDay(day3Interval, _skillDay3, _skill1, _baseLineData.Activity1);
                expectCallForDay(day4Interval, _skillDay4, _skill2, _baseLineData.Activity2);
                expectCallForDay(day5Interval, _skillDay5, _skill2, _baseLineData.Activity2);
                expectCallForDay(day6Interval, _skillDay6, _skill2, _baseLineData.Activity2);

            }
            var result = _target.GetOpenHoursPerActivity(_teamBlockInfo);
            Assert.AreEqual(result.Count(), 2);
            Assert.AreEqual(result[_baseLineData.Activity1].StartTime, TimeSpan.FromHours(9));
            Assert.AreEqual(result[_baseLineData.Activity1].EndTime, TimeSpan.FromHours(10));
            Assert.AreEqual(result[_baseLineData.Activity2].StartTime, TimeSpan.FromHours(9));
            Assert.AreEqual(result[_baseLineData.Activity2].EndTime, TimeSpan.FromHours(11));

        }

        [Test]
        public void ShouldNotContinueWithEmptySkillStaff()
        {


            IList<ISkillDay> skillDays = new List<ISkillDay> { _skillDay1, _skillDay2, _skillDay3 };
            var readOnlyListOfSkillInterval =
                    new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>());

            using (_mock.Record())
            {
                Expect.Call(_teamBlockInfo.BlockInfo).Return(_baseLineData.BlockOfThreeDays);
                Expect.Call(
                    _scheduleResultStartHolder.SkillDaysOnDateOnly(
                        _baseLineData.BlockOfThreeDays.BlockPeriod.DayCollection())).Return(skillDays);

                Expect.Call(_skillDay1.Skill).Return(_skill1);
                Expect.Call(_skill1.Activity).Return(_baseLineData.Activity1);
                Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(readOnlyListOfSkillInterval);
                Expect.Call(_skillStaffPeriodMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>()))
                      .Return(new List<ISkillIntervalData>());

                Expect.Call(_skillDay2.Skill).Return(_skill1);
                Expect.Call(_skill1.Activity).Return(_baseLineData.Activity1);
                Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(readOnlyListOfSkillInterval).IgnoreArguments();
                Expect.Call(_skillStaffPeriodMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>()))
                      .Return(new List<ISkillIntervalData>());

                Expect.Call(_skillDay3.Skill).Return(_skill1);
                Expect.Call(_skill1.Activity).Return(_baseLineData.Activity1);
                Expect.Call(_skillDay3.SkillStaffPeriodCollection).Return(readOnlyListOfSkillInterval).IgnoreArguments();
                Expect.Call(_skillStaffPeriodMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>()))
                      .Return(new List<ISkillIntervalData>());

            }
            var result = _target.GetOpenHoursPerActivity(_teamBlockInfo);
            Assert.AreEqual(result.Count(), 0);

        }

        private IList<ISkillIntervalData> generateIntervalForDay(DateOnly startDate, int startHour, int endHour)
        {
            var result = new List<ISkillIntervalData>();
            for (int i = startHour; i < endHour; i++ )
            {
                var dateTimePeriod = new DateTimePeriod(new DateTime(startDate.Year ,startDate.Month,startDate.Day ,i,0,0,DateTimeKind.Utc ), new DateTime(startDate.Year ,startDate.Month,startDate.Day ,i+1,0,0,DateTimeKind.Utc ));
                result.Add(new SkillIntervalData(dateTimePeriod, 10, 11, 12, 13, 14));
            }
            return result;
        }
        

    }

    
}
