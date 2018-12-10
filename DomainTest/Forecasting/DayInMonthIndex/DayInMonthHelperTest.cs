using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.DayInMonthIndex;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting.DayInMonthIndex
{
    [TestFixture]
    public class DayInMonthHelperTest
    {
        [Test]
        public void ShouldMakeThirtyDaysMonth()
        {
            Assert.That(DayInMonthHelper.DayIndex(new DateOnly(2011,3,19)),Is.EqualTo(18));
            Assert.That(DayInMonthHelper.DayIndex(new DateOnly(2011,4,1)),Is.EqualTo(1));
            Assert.That(DayInMonthHelper.DayIndex(new DateOnly(2011,2,28)),Is.EqualTo(30));
            Assert.That(DayInMonthHelper.DayIndex(new DateOnly(2012,2,28)),Is.EqualTo(29));
            Assert.That(DayInMonthHelper.DayIndex(new DateOnly(2012,2,29)),Is.EqualTo(30));
            Assert.That(DayInMonthHelper.DayIndex(new DateOnly(2011,12,31)),Is.EqualTo(30));
        }
    }
}