using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
	public class DailySkillForecastAndScheduledValueCalculatorTest
    {
        private IDailySkillForecastAndScheduledValueCalculator _target;
        private MockRepository _mock;
        private ISkill _skill1;
        private IList<ISkill> _skillList;
        private ISchedulingResultStateHolder _stateHolder;
        private IList<ISkillStaffPeriod> _skillStaffPeriods;
        private ISkillStaffPeriod _skillStaffPeriod1;
        private ISkillStaffPeriod _skillStaffPeriod2;
        private ISkillStaffPeriodHolder _skillStaffPeriodHolder;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _skill1 = SkillFactory.CreateSkill("skill1", SkillTypeFactory.CreateSkillType(), 15);
            _skillList = new List<ISkill>{_skill1};
            _skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod2 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriodHolder = _mock.StrictMock<ISkillStaffPeriodHolder>();
            _stateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _target = new DailySkillForecastAndScheduledValueCalculator(()=>_stateHolder, new UtcTimeZone());
        }

        [Test]
        public void VerifyEmptyPeriodHolderReturnsValueWithZeroForecast()
        {
            _skillStaffPeriods = new List<ISkillStaffPeriod>();
            DateOnly dtDateOnly = new DateOnly(2010, 1, 2);
			DateTimePeriod dtPeriod = new DateTimePeriod(
					new DateTime(2010, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2010, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc));

			using (_mock.Record())
            {
                Expect.Call(_stateHolder.SkillStaffPeriodHolder).Return(_skillStaffPeriodHolder).Repeat.Any();
                Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, dtPeriod))
                    .Return(_skillStaffPeriods);
            }
            ForecastScheduleValuePair ret;
            using (_mock.Playback())
            {
                ret = _target.CalculateDailyForecastAndScheduleDataForSkill(_skill1, dtDateOnly);
            }
            Assert.AreEqual(0, ret.ForecastValue);
        }

        [Test]
        public void VerifyOneScheduleOnePeriod()
        {
            _skillStaffPeriods = new List<ISkillStaffPeriod>{_skillStaffPeriod1};
            
            const int skillStaffPeriod1FStaffMinutes = 10;
            const int skillStaffPeriod1PeriodMinutes = 10;

            DateOnly dtDateOnly = new DateOnly(2010, 1, 2);
			DateTimePeriod dtPeriod = new DateTimePeriod(
					new DateTime(2010, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2010, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc));
			DateTimePeriod schedulePeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 0, skillStaffPeriod1PeriodMinutes, 0, DateTimeKind.Utc));

            using (_mock.Record())
            {
                Expect.Call(_stateHolder.SkillStaffPeriodHolder)
                    .Return(_skillStaffPeriodHolder);
                Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, dtPeriod))
                    .Return(_skillStaffPeriods);
                Expect.Call(_skillStaffPeriod1.FStaffTime())
                    .Return(TimeSpan.FromMinutes(skillStaffPeriod1FStaffMinutes));
                Expect.Call(_skillStaffPeriod1.CalculatedResource)
                    .Return(1);
                Expect.Call(_skillStaffPeriod1.Period)
                    .Return(schedulePeriod);
                
            }
            ForecastScheduleValuePair ret;
            using (_mock.Playback())
            {
                ret = _target.CalculateDailyForecastAndScheduleDataForSkill(_skill1, dtDateOnly);
            }
            Assert.AreEqual(skillStaffPeriod1FStaffMinutes, ret.ForecastValue);
            Assert.AreEqual(skillStaffPeriod1PeriodMinutes, ret.ScheduleValue);
        }

        [Test]
        public void VerifyOneScheduleMorePeriods()
        {
            _skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2 };

            const int skillStaffPeriod1FStaffMinutes = 10;
            const int skillStaffPeriod2FStaffMinutes = 5;

            const int skillStaffPeriod1PeriodMinutes = 10;
            const int skillStaffPeriod2PeriodMinutes = 5;

            DateOnly dtDateOnly = new DateOnly(2010, 1, 2);
            DateTimePeriod dtPeriod = new DateTimePeriod(
                new DateTime(2010, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2010, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc));
            DateTimePeriod schedulePeriod1 = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 0, skillStaffPeriod1PeriodMinutes, 0, DateTimeKind.Utc));
            DateTimePeriod schedulePeriod2 = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 0, skillStaffPeriod2PeriodMinutes, 0, DateTimeKind.Utc));

            using (_mock.Record())
            {
                Expect.Call(_stateHolder.SkillStaffPeriodHolder)
                    .Return(_skillStaffPeriodHolder);
                Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, dtPeriod))
                    .Return(_skillStaffPeriods);
                Expect.Call(_skillStaffPeriod1.FStaffTime())
                    .Return(TimeSpan.FromMinutes(skillStaffPeriod1FStaffMinutes));
                Expect.Call(_skillStaffPeriod2.FStaffTime())
                    .Return(TimeSpan.FromMinutes(skillStaffPeriod2FStaffMinutes));
                Expect.Call(_skillStaffPeriod1.CalculatedResource)
                    .Return(1); // 1 is just to keep the value
                Expect.Call(_skillStaffPeriod2.CalculatedResource)
                    .Return(1); // 1 is just to keep the value
                Expect.Call(_skillStaffPeriod1.Period)
                    .Return(schedulePeriod1);
                Expect.Call(_skillStaffPeriod2.Period)
                    .Return(schedulePeriod2);

            }
            ForecastScheduleValuePair ret;
            using (_mock.Playback())
            {
                ret = _target.CalculateDailyForecastAndScheduleDataForSkill(_skill1, dtDateOnly);
            }
            Assert.AreEqual(skillStaffPeriod1FStaffMinutes + skillStaffPeriod2FStaffMinutes, ret.ForecastValue);
            Assert.AreEqual(skillStaffPeriod1PeriodMinutes + skillStaffPeriod2PeriodMinutes, ret.ScheduleValue);
        }
    }
}
