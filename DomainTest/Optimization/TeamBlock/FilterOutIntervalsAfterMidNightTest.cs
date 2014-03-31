using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
    [TestFixture ]
    public class FilterOutIntervalsAfterMidNightTest
    {
        private IFilterOutIntervalsAfterMidNight _target;
        private MockRepository _mock;
        private ISkillStaffPeriod _skillStaffPeriod1;
        private ISkillStaffPeriod _skillStaffPeriod2;
        private ISkillStaffPeriod _skillStaffPeriod3;
        private ISkillStaffPeriod _skillStaffPeriod4;
        private TimeZoneInfo _timeZoneInfo;
        private TimeZoneInfo _oldTimeZoneValue;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            TimeZoneInfo tz = (TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time"));
            _oldTimeZoneValue = TimeZoneGuard.Instance.TimeZone;
            TimeZoneGuard.Instance.TimeZone = tz;

            _skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod2 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod3 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod4 = _mock.StrictMock<ISkillStaffPeriod>();
            _timeZoneInfo = tz;
            _target = new FilterOutIntervalsAfterMidNight();
        }

        [TearDown]
        public void TearDown()
        {
            TimeZoneGuard.Instance.TimeZone = _oldTimeZoneValue;
        }

        [Test]
        public void ShouldReturnMissingIntervalsAfterMidNight()
        {
            DateOnly today = new DateOnly(2014, 03, 27);
            IList<ISkillStaffPeriod> skillStaffPeriodList = new ISkillStaffPeriod[] { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3, _skillStaffPeriod4 };
            using (_mock.Record())
            {
                Expect.Call(_skillStaffPeriod1.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 21, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 22, 0, 0, DateTimeKind.Utc))));
                Expect.Call(_skillStaffPeriod2.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 22, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 23, 0, 0, DateTimeKind.Utc))));
                Expect.Call(_skillStaffPeriod3.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 23, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 28, 0, 0, 0, DateTimeKind.Utc))));
                Expect.Call(_skillStaffPeriod4.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 28, 0, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 28, 1, 0, 0, DateTimeKind.Utc))));
            }
            using (_mock.Playback())
            {
                var result = _target.Filter(skillStaffPeriodList, today);
                Assert.AreEqual(3, result.Count);
                Assert.AreEqual(_skillStaffPeriod1 , result[0]);
                Assert.AreEqual(_skillStaffPeriod2 , result[1]);
                Assert.AreEqual(_skillStaffPeriod3 , result[2]);
            }
        }

        [Test]
        public void ShouldReturnAllIntervalsIfAllAreOnSameDay()
        {
            DateOnly today = new DateOnly(2014, 03, 27);
            IList<ISkillStaffPeriod> skillStaffPeriodList = new ISkillStaffPeriod[] { _skillStaffPeriod1, _skillStaffPeriod2, _skillStaffPeriod3, _skillStaffPeriod4 };
            using (_mock.Record())
            {
                Expect.Call(_skillStaffPeriod1.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 21, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 22, 0, 0, DateTimeKind.Utc))));
                Expect.Call(_skillStaffPeriod2.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 22, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 23, 0, 0, DateTimeKind.Utc))));
                Expect.Call(_skillStaffPeriod3.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 23, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 28, 0, 0, 0, DateTimeKind.Utc))));
                Expect.Call(_skillStaffPeriod4.Period).Return(new DateTimePeriod(
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 0, 0, 0, DateTimeKind.Utc)),
                    _timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2014, 03, 27, 1, 0, 0, DateTimeKind.Utc))));
            }
            using (_mock.Playback())
            {
                var result = _target.Filter(skillStaffPeriodList, today);
                Assert.AreEqual(4, result.Count);
                Assert.AreEqual(_skillStaffPeriod1, result[0]);
                Assert.AreEqual(_skillStaffPeriod2, result[1]);
                Assert.AreEqual(_skillStaffPeriod3, result[2]);
                Assert.AreEqual(_skillStaffPeriod4, result[3]);
            }
        }
    }

    
}
