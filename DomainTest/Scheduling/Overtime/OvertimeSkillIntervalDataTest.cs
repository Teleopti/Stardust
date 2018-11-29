using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Overtime;


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
