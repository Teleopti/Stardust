using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
    [TestFixture]
    public class LocateMissingIntervalsIfMidNightBreakTest
    {
        private ILocateMissingIntervalsIfMidNightBreak _target;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private MockRepository _mock;
        private ISkillDay _skillDay1;
        private ISkillDay _skillDay2;
        private ISkill _skill1;
        private ISkill _skill2;
        private ISkillStaffPeriod _skillStaffPeriod1;
        private ISkillStaffPeriod _skillStaffPeriod2;
        private ISkillStaffPeriod _skillStaffPeriod3;
        private ISkillStaffPeriod _skillStaffPeriod4;
        private TimeZoneInfo _timeZoneInfo;
        private TimeZoneInfo _tz;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _tz = (TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time"));
            
            _skillDay1 = _mock.StrictMock<ISkillDay>();
            _skillDay2 = _mock.StrictMock<ISkillDay>();
            _skill1 = _mock.StrictMock<ISkill>();
            _skill2 = _mock.StrictMock<ISkill>();
            _skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod2 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod3 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod4 = _mock.StrictMock<ISkillStaffPeriod>();
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _timeZoneInfo = _tz;
            _target = new LocateMissingIntervalsIfMidNightBreak(()=>_schedulingResultStateHolder);
        }

       [Test]
        public void ShouldReturnEmptyListIfNoIntervalFound()
        {
            DateOnly today = new DateOnly(2014, 03, 27);
            IList<ISkillDay> skillDayList = new ISkillDay[] { _skillDay1, _skillDay2 };
            var skillStaffPeriodList = new ISkillStaffPeriod[] { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3, _skillStaffPeriod4 };
            using (_mock.Record())
            {
                Expect.Call(_skill2.MidnightBreakOffset).Return(TimeSpan.FromHours(2));
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new DateOnly[] { today.AddDays(-1) }))
                    .Return(skillDayList);
                Expect.Call(_skillDay1.Skill).Return(_skill1);
                Expect.Call(_skillDay2.Skill).Return(_skill2);
                Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(skillStaffPeriodList);
                Expect.Call(_skillStaffPeriod1.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 10, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 11, 0, 0, DateTimeKind.Utc))));
                Expect.Call(_skillStaffPeriod2.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 11, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 12, 0, 0, DateTimeKind.Utc))));
                Expect.Call(_skillStaffPeriod3.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 20, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 21, 0, 0, DateTimeKind.Utc))));
                Expect.Call(_skillStaffPeriod4.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 21, 0, 0, DateTimeKind.Utc )),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 22, 0, 0, DateTimeKind.Utc ))));
            }
            using (_mock.Playback())
            {
                var result = _target.GetMissingSkillStaffPeriods(today, _skill2, _tz);
                Assert.AreEqual(0, result.Count);
            }
        }

        [Test]
        public void ShouldFilterIntervalsThatAreAfterMidNight()
        {
            DateOnly today = new DateOnly(2014, 03, 27);
            IList<ISkillDay> skillDayList = new ISkillDay[] { _skillDay1, _skillDay2 };
            var skillStaffPeriodList = new ISkillStaffPeriod[] { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3, _skillStaffPeriod4 };
            
            using (_mock.Record())
            {
                Expect.Call(_skill2.MidnightBreakOffset).Return(TimeSpan.FromHours(2));
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new DateOnly[] { today.AddDays(-1) }))
                    .Return(skillDayList);
                Expect.Call(_skillDay1.Skill).Return(_skill1);
                Expect.Call(_skillDay2.Skill).Return(_skill2);
                Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(skillStaffPeriodList);
                Expect.Call(_skillStaffPeriod1.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 10, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 11, 0, 0, DateTimeKind.Utc))));
                Expect.Call(_skillStaffPeriod2.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 21, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 22, 0, 0, DateTimeKind.Utc))));
                Expect.Call(_skillStaffPeriod3.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 22, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 26, 23, 0, 0, DateTimeKind.Utc))));
                Expect.Call(_skillStaffPeriod4.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 0, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 1, 0, 0, DateTimeKind.Utc))));
            }
            using (_mock.Playback())
            {
                var result = _target.GetMissingSkillStaffPeriods(today, _skill2, _tz);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(_skillStaffPeriod4, result[0]);
            }
        }
    }

    
}
