using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Overtime;


namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class OvertimePeriodValueMapperTest
    {
        private OvertimePeriodValueMapper _target;

        [SetUp]
        public void Setup()
        {
            _target = new OvertimePeriodValueMapper();
        }
        [Test]
        public void ShouldMap()
        {
            DateTimePeriod period = new DateTimePeriod(2014, 02, 26, 2014, 02, 26);
            var overtimeSkillIntervalData = new OvertimeSkillIntervalData(period, 10, 5);
            var overtimePeriodValue = _target.Map(new List<IOvertimeSkillIntervalData> { overtimeSkillIntervalData });
            Assert.AreEqual(period, overtimePeriodValue[0].Period);
            Assert.AreEqual(-0.5, overtimePeriodValue[0].Value);
        }
    }
}