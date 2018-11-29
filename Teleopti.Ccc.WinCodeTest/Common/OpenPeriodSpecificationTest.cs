using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class OpenPeriodSpecificationTest
    {
        private OpenPeriodSpecification _target;

        [Test]
        public void ShouldAllowShortPeriod()
        {
            _target = new OpenPeriodSpecification(14);
            Assert.IsTrue(_target.IsSatisfiedBy(new DateOnlyPeriod(2011, 1, 1, 2011, 1, 14)));
        }

        [Test]
        public void ShouldForbidLongPeriod()
        {
            _target = new OpenPeriodSpecification(14);
            Assert.IsFalse(_target.IsSatisfiedBy(new DateOnlyPeriod(2011, 1, 1, 2011, 1, 15)));
        }
    }
}
