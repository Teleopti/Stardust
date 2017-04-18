using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class GridClassTypeTest
    {
        private GridClassType _target;

        [SetUp]
        public void Setup()
        {
            _target = new GridClassType("Test", typeof(ActivityAbsoluteStartExtender));
        }

        [Test]
        public void VerifyCanAccessProperties()
        {
            Assert.AreEqual("Test", _target.Name);
            Assert.AreEqual(typeof(ActivityAbsoluteStartExtender), _target.ClassType);
        }
    }
}
