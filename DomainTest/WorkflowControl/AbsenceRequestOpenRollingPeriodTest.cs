using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;


namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class AbsenceRequestOpenRollingPeriodTest : AbsenceRequestOpenPeriodTest
    {
        [Test]
        public void CanSetAndGetPeriod()
        {
            AbsenceRequestOpenRollingPeriod absenceRequestOpenRollingPeriod = (AbsenceRequestOpenRollingPeriod)Target;
            MinMax<int> daysMinMax = new MinMax<int>(1, 5);
            absenceRequestOpenRollingPeriod.BetweenDays = daysMinMax;
            Assert.AreEqual(daysMinMax, absenceRequestOpenRollingPeriod.BetweenDays);
            DateOnly viewpointDateOnly = new DateOnly(2010, 07, 07);
            DateOnlyPeriod expectedDateOnlyPeriod = new DateOnlyPeriod(2010, 07, 08, 2010, 07, 12);
            DateOnlyPeriod dateOnlyPeriod = Target.GetPeriod(viewpointDateOnly);
            Assert.AreEqual(expectedDateOnlyPeriod, dateOnlyPeriod);
        }

        protected override IAbsenceRequestOpenPeriod CreateInstance()
        {
            return new AbsenceRequestOpenRollingPeriod();
        }
    }
}
