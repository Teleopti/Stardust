using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class OvertimeLengthDeciderTest
    {
        private IOvertimeLengthDecider _target;
        private MockRepository _mocks;
        private ISkillResolutionProvider _skillResolutionProvider;
        private IOvertimeSkillStaffPeriodToSkillIntervalDataMapper _overtimeSkillStaffPeriodToSkillIntervalDataMapper;
        private IOvertimeSkillIntervalDataDivider _overtimeSkillIntervalDataDivider;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private DateTime _date;
        private ISkillDay _skillDay1;
        private IPerson _person;
        private ISkill _skill1;
        private ISkill _skill2;
        private ICalculateBestOvertime _calculateBestOvertime;
        private OvertimePeriodValueMapper _overtimePeriodValueMapper;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _date = DateTime.SpecifyKind(DateOnly.MinValue.Date, DateTimeKind.Utc);
            _skillResolutionProvider = _mocks.StrictMock<ISkillResolutionProvider>();
            _overtimeSkillStaffPeriodToSkillIntervalDataMapper = _mocks.StrictMock<IOvertimeSkillStaffPeriodToSkillIntervalDataMapper>();
            _overtimeSkillIntervalDataDivider = _mocks.StrictMock<IOvertimeSkillIntervalDataDivider>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _skillDay1 = _mocks.StrictMock<ISkillDay>();
            _skill1 = SkillFactory.CreateSkill("1");
            _skill2 = SkillFactory.CreateSkill("2");
            _person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue,
                                                                                    new List<ISkill> { _skill1, _skill2 });
            _calculateBestOvertime = _mocks.StrictMock<ICalculateBestOvertime>();
            _overtimePeriodValueMapper = new OvertimePeriodValueMapper();
            _target = new OvertimeLengthDecider(_skillResolutionProvider, _overtimeSkillStaffPeriodToSkillIntervalDataMapper,
                                                _overtimeSkillIntervalDataDivider, _schedulingResultStateHolder, _calculateBestOvertime, _overtimePeriodValueMapper);
        }

        [Test]
        public void ShouldBeZeroIfPersonHasNoSkillOfOneActivity()
        {
            var result = _target.Decide(_person, DateOnly.Today, new DateTime(),
                                        ActivityFactory.CreateActivity("No Match Acitivity"),
                                        new MinMax<TimeSpan>(TimeSpan.FromHours(1), TimeSpan.FromHours(2)));
            Assert.That(result, Is.EqualTo(TimeSpan.Zero));
        }


        [Test]
        public void ShouldNotExtendWhenRelativeDifferenceIsPositive()
        {
            var skillIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(_date, _date.AddMinutes(15)), 0, 1);
            var skillIntervalData2 = new OvertimeSkillIntervalData(new DateTimePeriod(_date.AddMinutes(15), _date.AddMinutes(30)), 0, 1);
            var skillIntervalData3 = new OvertimeSkillIntervalData(new DateTimePeriod(_date.AddMinutes(30), _date.AddMinutes(45)), 0, 2);
            var skillIntervalData4 = new OvertimeSkillIntervalData(new DateTimePeriod(_date.AddMinutes(45), _date.AddMinutes(60)), 0, 3);
            var skillIntervalData5 = new OvertimeSkillIntervalData(new DateTimePeriod(_date.AddMinutes(60), _date.AddMinutes(75)), 2, 4);
            var skillIntervalData6 = new OvertimeSkillIntervalData(new DateTimePeriod(_date.AddMinutes(75), _date.AddMinutes(90)), 1, 5);
            var skillIntervalData7 = new OvertimeSkillIntervalData(new DateTimePeriod(_date.AddMinutes(90), _date.AddMinutes(105)), -3, 6);
            MinMax<TimeSpan> duration = new MinMax<TimeSpan>(TimeSpan.FromHours(1), TimeSpan.FromHours(2));
            var skillIntervalDataList = new[]
                {
                    skillIntervalData1, skillIntervalData2, skillIntervalData3, skillIntervalData4, skillIntervalData5,
                    skillIntervalData6, skillIntervalData7
                };

            using (_mocks.Record())
            {
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
                              Return(new List<ISkillDay> { _skillDay1 });

                Expect.Call(_skillResolutionProvider.MinimumResolution(new List<ISkill>())).IgnoreArguments().Return(15);
                Expect.Call(_overtimeSkillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>())).IgnoreArguments().Return(
                     skillIntervalDataList).Repeat.AtLeastOnce();
                Expect.Call(_overtimeSkillIntervalDataDivider.SplitSkillIntervalData(new List<IOvertimeSkillIntervalData>(), 15)).IgnoreArguments().
                         Return(skillIntervalDataList).Repeat.AtLeastOnce();
                Expect.Call(_skillDay1.Skill).Return(_skill1);
                Expect.Call(_calculateBestOvertime.GetBestOvertime(duration, new List<OvertimePeriodValue>(), _date, 15))
                      .IgnoreArguments()
                      .Return(TimeSpan.Zero);
            }
            using (_mocks.Playback())
            {

                var resultLength = _target.Decide(_person, new DateOnly(_date), _date, _skill1.Activity,
                                                                  duration);

                Assert.That(resultLength, Is.EqualTo(TimeSpan.Zero));
            }
        }

        [Test]
        public void ShouldNotExtendWhenSpecifiedDurationIsZero()
        {

            var skillIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(_date, _date.AddMinutes(15)), 0, 1);
            var skillIntervalDataList = new List<IOvertimeSkillIntervalData>() { skillIntervalData1 };
            MinMax<TimeSpan> duration = new MinMax<TimeSpan>(TimeSpan.FromHours(0), TimeSpan.FromHours(0));
            using (_mocks.Record())
            {
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
                              Return(new List<ISkillDay> { _skillDay1 });

                Expect.Call(_skillResolutionProvider.MinimumResolution(new List<ISkill>())).IgnoreArguments().Return(15);
                Expect.Call(_overtimeSkillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>())).IgnoreArguments().Return(
                     skillIntervalDataList).Repeat.AtLeastOnce();
                Expect.Call(_overtimeSkillIntervalDataDivider.SplitSkillIntervalData(new List<IOvertimeSkillIntervalData>(), 15)).IgnoreArguments().
                         Return(skillIntervalDataList).Repeat.AtLeastOnce();
                Expect.Call(_skillDay1.Skill).Return(_skill1);

                Expect.Call(_calculateBestOvertime.GetBestOvertime(duration, new List<OvertimePeriodValue>(), _date, 15))
                      .IgnoreArguments()
                      .Return(TimeSpan.Zero);
            }
            using (_mocks.Playback())
            {
                var resultLength = _target.Decide(_person, new DateOnly(_date), _date, _skill1.Activity, duration);

                Assert.That(resultLength, Is.EqualTo(TimeSpan.Zero));
            }
        }

        [Test]
        public void ShouldReturnZeroIfNoForecastIsAvailable()
        {

            var skillIntervalDataList = new List<IOvertimeSkillIntervalData>();
            MinMax<TimeSpan> duration = new MinMax<TimeSpan>(TimeSpan.FromHours(0), TimeSpan.FromHours(0));
            using (_mocks.Record())
            {
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
                              Return(new List<ISkillDay> { _skillDay1 });

                Expect.Call(_skillResolutionProvider.MinimumResolution(new List<ISkill>())).IgnoreArguments().Return(15);
                Expect.Call(_overtimeSkillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>())).IgnoreArguments().Return(
                     skillIntervalDataList).Repeat.AtLeastOnce();
                Expect.Call(_skillDay1.Skill).Return(_skill1);
            }
            using (_mocks.Playback())
            {
                var resultLength = _target.Decide(_person, new DateOnly(_date), _date, _skill1.Activity, duration);

                Assert.That(resultLength, Is.EqualTo(TimeSpan.Zero));
            }
        }
    }
}
