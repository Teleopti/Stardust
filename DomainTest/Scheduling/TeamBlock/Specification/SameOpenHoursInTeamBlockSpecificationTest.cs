using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Specification
{
    [TestFixture]
    public class SameOpenHoursInTeamBlockSpecificationTest
    {
        private BaseLineData _baseLineData;
        private ISameOpenHoursInTeamBlockSpecification _target;
        private MockRepository _mock;
        private ISkillDay _skillDay1;
        private ISkillDay _skillDay2;
        private ISkillDay _skillDay3;
        private ITeamBlockInfo _teamBlockInfo;
        private ISkill _skill1;
        private readonly DateOnly _today = new DateOnly(2013, 10, 17);
        private ISchedulingResultStateHolder _scheduleResultStartHolder;
        private ISkillStaffPeriod _sampleSkillStaffPeriod;
	    private IOpenHourForDate _openHourForDate;
	    private ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;

        [SetUp]
        public void Setup()
        {
            _baseLineData = new BaseLineData(_today);
            _mock = new MockRepository();

            _scheduleResultStartHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
	        _openHourForDate = _mock.StrictMock<IOpenHourForDate>();
			_createSkillIntervalDataPerDateAndActivity = _mock.StrictMock<ICreateSkillIntervalDataPerDateAndActivity>();
            _skillDay1 = _mock.StrictMock<ISkillDay>();
            _skillDay2 = _mock.StrictMock<ISkillDay>();
            _skillDay3 = _mock.StrictMock<ISkillDay>();
            _teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
            _skill1 = _mock.StrictMock<ISkill>();
            _sampleSkillStaffPeriod = _mock.StrictMock<ISkillStaffPeriod>();
			_target = new SameOpenHoursInTeamBlockSpecification(_openHourForDate, _createSkillIntervalDataPerDateAndActivity, _scheduleResultStartHolder);

        }

        private void expectCallForDay(ISkillDay skillDay, ISkill skill, IActivity activity)
        {
            var readOnlyListOfSkillInterval =
                   new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>{_sampleSkillStaffPeriod});
            Expect.Call(skillDay.Skill).Return(skill);
            Expect.Call(skill.Activity).Return(activity);
            Expect.Call(skillDay.SkillStaffPeriodCollection).Return(readOnlyListOfSkillInterval).IgnoreArguments().Repeat.AtLeastOnce();
        }

        private IList<ISkillIntervalData> generateIntervalForDay(DateOnly startDate, int startHour, int endHour)
        {
            var result = new List<ISkillIntervalData>();
            for (int i = startHour; i < endHour; i++)
            {
                var dateTimePeriod = new DateTimePeriod(new DateTime(startDate.Year, startDate.Month, startDate.Day, i, 0, 0, DateTimeKind.Utc), new DateTime(startDate.Year, startDate.Month, startDate.Day, i + 1, 0, 0, DateTimeKind.Utc));
                result.Add(new SkillIntervalData(dateTimePeriod, 10, 11, 12, 13, 14));
            }
            return result;
        }

        

        [Test]
        public void HasDifferentOpenHourForTeamBlock()
        {
            var day1Interval = generateIntervalForDay(new DateOnly(2013, 10, 17), 8, 11);
            var day2Interval = generateIntervalForDay(new DateOnly(2013, 10, 18), 9, 11);
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
				Expect.Call(_skillDay1.CurrentDate).Return(new DateOnly(2013, 10, 17));
				Expect.Call(_skillDay1.CurrentDate).Return(new DateOnly(2013, 10, 17));
				Expect.Call(_skillDay2.CurrentDate).Return(new DateOnly(2013, 10, 18));
				Expect.Call(_skillDay3.CurrentDate).Return(new DateOnly(2013, 10, 19));
                expectCallForDay(_skillDay1, _skill1, _baseLineData.Activity1);
                expectCallForDay(_skillDay1, _skill1, _baseLineData.Activity1);
                expectCallForDay(_skillDay2, _skill1, _baseLineData.Activity1);
                expectCallForDay(_skillDay3, _skill1, _baseLineData.Activity1);


            }
            Assert.IsFalse(_target.IsSatisfiedBy(_teamBlockInfo ));
        }

        [Test]
        public void HasSameOpenHourForTeamBlock()
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
				Expect.Call(_skillDay1.CurrentDate).Return(new DateOnly(2013, 10, 17));
				Expect.Call(_skillDay1.CurrentDate).Return(new DateOnly(2013, 10, 17));
				Expect.Call(_skillDay2.CurrentDate).Return(new DateOnly(2013, 10, 18));
				Expect.Call(_skillDay3.CurrentDate).Return(new DateOnly(2013, 10, 19));
                expectCallForDay(_skillDay1, _skill1, _baseLineData.Activity1);
                expectCallForDay(_skillDay1, _skill1, _baseLineData.Activity1);
                expectCallForDay(_skillDay2, _skill1, _baseLineData.Activity1);
                expectCallForDay(_skillDay3, _skill1, _baseLineData.Activity1);
            }
            Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo ));
        }

        [Test]
        public void HasSingleSkillDayWithNoSkillStaffPeriod()
        {
            var day1Interval = generateIntervalForDay(new DateOnly(2013, 10, 17), 8, 11);

            var skillIntervalList = new List<ISkillIntervalData>();
            skillIntervalList.AddRange(day1Interval);
            IList<ISkillDay> skillDays = new List<ISkillDay> { _skillDay1 };

            using (_mock.Record())
            {
                Expect.Call(_teamBlockInfo.BlockInfo).Return(_baseLineData.BlockOfThreeDays);
                Expect.Call(
                    _scheduleResultStartHolder.SkillDaysOnDateOnly(
                        _baseLineData.BlockOfThreeDays.BlockPeriod.DayCollection())).Return(skillDays);

                var readOnlyListOfSkillInterval =
                  new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> ());
                Expect.Call(_skillDay1 .Skill).Return(_skill1);
                Expect.Call(_skill1.Activity).Return(_baseLineData.Activity1);
                Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(readOnlyListOfSkillInterval).IgnoreArguments().Repeat.AtLeastOnce();

            }
            Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
        }
    }

    
}
