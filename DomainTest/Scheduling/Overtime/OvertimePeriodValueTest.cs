using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Overtime;


namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class OvertimePeriodValueTest
    {
        private OvertimePeriodValue _target;

        [Test]
        public void ShouldMapPeriodValue()
        {
            var period = new DateTimePeriod(2014, 02, 26, 2014, 02, 26);
            const double value = 15.2;
            _target = new OvertimePeriodValue(period, value);
            Assert.AreEqual(period, _target.Period);
            Assert.AreEqual(value, _target.Value);
        }
    }
}