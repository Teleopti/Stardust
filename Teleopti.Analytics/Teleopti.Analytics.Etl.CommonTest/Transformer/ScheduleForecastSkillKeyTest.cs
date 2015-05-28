using System;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class ScheduleForecastSkillKeyTest
    {
        private IScheduleForecastSkillKey _target;
        readonly DateTime _startDateTime = new DateTime(2009, 9, 8, 22, 30, 0);
        const int intervalId = 2;
        readonly Guid _skillCode = Guid.NewGuid();
        readonly Guid _scenarioCode = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            _target = new ScheduleForecastSkillKey(_startDateTime, intervalId, _skillCode, _scenarioCode);
        }

        [Test]
        public void VerifyCanAccessProperties()
        {
            Assert.AreEqual(_startDateTime, _target.StartDateTime);
            Assert.AreEqual(intervalId, _target.IntervalId);
            Assert.AreEqual(_scenarioCode, _target.ScenarioCode);
            Assert.AreEqual(_skillCode, _target.SkillCode);
        }

        [Test]
        public void VerifyKeyIsCompatibleWithScheduleForecastSkill()
        {
            IScheduleForecastSkillKey scheduleForecastSkill = new ScheduleForecastSkill(_target.StartDateTime, _target.IntervalId,
                                                                                    _target.SkillCode,
                                                                                    _target.ScenarioCode);
            Assert.AreEqual(_target.StartDateTime, scheduleForecastSkill.StartDateTime);
            Assert.AreEqual(_target.IntervalId, scheduleForecastSkill.IntervalId);
            Assert.AreEqual(_target.SkillCode, scheduleForecastSkill.SkillCode);
            Assert.AreEqual(_target.ScenarioCode, scheduleForecastSkill.ScenarioCode);
        }
    }
}
