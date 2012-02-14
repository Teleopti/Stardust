using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class TimeDefinitionTest
    {
        [Test]
        public void ShouldGetSecondsPerMinute()
        {
            Assert.AreEqual(60, TimeDefinition.SecondsPerMinute);
        }
        
        [Test]
        public void ShouldGetMinutesPerHour()
        {
            Assert.AreEqual(60, TimeDefinition.MinutesPerHour);
        }
        
        [Test]
        public void ShouldGetHoursPerDay()
        {
            Assert.AreEqual(24, TimeDefinition.HoursPerDay);
        }
        
    }
}
