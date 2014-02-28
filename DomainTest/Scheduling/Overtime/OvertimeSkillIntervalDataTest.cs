using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class OvertimeSkillIntervalDataTest
    {
        private IOvertimeSkillIntervalData _target;
        private DateTimePeriod _dtp;

        [SetUp]
        public void Setup()
        {
            _dtp = new DateTimePeriod(2012, 11, 28, 2012, 11, 28);

        }

        [Test]
        public void ShouldReturnNegativeRelativeDifference()
        {
            _target = new OvertimeSkillIntervalData(_dtp, 6, -2);
            Assert.AreEqual(0.33, Math.Round(_target.RelativeDifference(), 2));
        }

        [Test]
        public void ShouldReturnRelativeDifference()
        {
            _target = new OvertimeSkillIntervalData(_dtp, 3, 8);
            Assert.AreEqual(-2.667, Math.Round(_target.RelativeDifference(), 3));
        }

        [Test]
        public void ShouldReturnForcastedDemand()
        {
            _target = new OvertimeSkillIntervalData(_dtp, 5.8, 8);
            Assert.AreEqual(5.8, Math.Round(_target.ForecastedDemand, 1));
        }

        [Test]
        public void ShouldReturnCurrentDemand()
        {
            _target = new OvertimeSkillIntervalData(_dtp, 5.8, 7.5);
            Assert.AreEqual(7.5, Math.Round(_target.CurrentDemand, 1));
        }
    }
}
