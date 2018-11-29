using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class RelativeBoostedDailyDifferencesByAllSkillsExtractorTest
    {
        private RelativeBoostedDailyDifferencesByAllSkillsExtractor _target;
        private MockRepository _mocks;
        private IList<ISkill> _skillList;
        private ISkill _skill1;
        private IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
        private ISkillExtractor _skillExtractor;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _dailySkillForecastAndScheduledValueCalculator = _mocks.StrictMock<IDailySkillForecastAndScheduledValueCalculator>();
            _skillExtractor = _mocks.StrictMock<ISkillExtractor>();
            _skillList = new List<ISkill>();
            _skill1 = SkillFactory.CreateSkill("skill1", SkillTypeFactory.CreateSkillType(), 15);
            _skillList.Add(_skill1);
        }

        [Test]
        public void VerifyValueOneDay()
        {
            DateOnly day = new DateOnly(2010, 4, 1);
            DateOnlyPeriod period = new DateOnlyPeriod(day, day);

            const int numberOfdays = 1;

            using (_mocks.Record())
            {
                Expect.Call(_skillExtractor.ExtractSkills())
                    .Return(_skillList);
                Expect.Call(
                    _dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(_skill1, day))
                    .Return(new ForecastScheduleValuePair());

            }
            IList<double?> ret;
            _target = new RelativeBoostedDailyDifferencesByAllSkillsExtractor(period, _dailySkillForecastAndScheduledValueCalculator, _skillExtractor);
            using (_mocks.Playback())
            {
                ret = _target.Values();
            }
            Assert.AreEqual(numberOfdays, ret.Count);
            Assert.IsNull(ret[0]);
        }

        [Test]
        public void VerifyValueMoreDays()
        {
            DateOnly day1 = new DateOnly(2010, 4, 1);
            DateOnly day2 = new DateOnly(2010, 4, 2);

            DateOnlyPeriod period = new DateOnlyPeriod(day1, day2);

            const int numberOfdays = 2;

            using (_mocks.Record())
            {
                Expect.Call(_skillExtractor.ExtractSkills())
                    .Return(_skillList).Repeat.Times(numberOfdays);
                Expect.Call(_dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(_skill1, day1))
                    .Return(new ForecastScheduleValuePair { ForecastValue = 10, ScheduleValue = 5 });
                Expect.Call(_dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(_skill1, day2))
                    .Return(new ForecastScheduleValuePair { ForecastValue = 10, ScheduleValue = 5 });

            }
            IList<double?> ret;
            _target = new RelativeBoostedDailyDifferencesByAllSkillsExtractor(period, _dailySkillForecastAndScheduledValueCalculator, _skillExtractor);
            using (_mocks.Playback())
            {
                ret = _target.Values();
            }
            Assert.AreEqual(numberOfdays, ret.Count);
            Assert.AreEqual(0.5d, ret[0]);
            Assert.AreEqual(0.5d, ret[1]);
        }
    }
}
        