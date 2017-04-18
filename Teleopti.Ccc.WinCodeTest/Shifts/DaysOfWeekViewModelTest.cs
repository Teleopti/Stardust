using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture, SetUICulture("en-US")]
    public class DaysOfWeekViewModelTest
    {
        private IDaysOfWeekViewModel _target;
        private IWorkShiftRuleSet _ruleSet;

        [SetUp]
        public void TestInit()
        {
            _ruleSet = WorkShiftRuleSetFactory.Create();
            _target = new DaysOfWeekViewModel(_ruleSet);
            _target.WorkShiftRuleSetName = new Description("Mahinda Rajapaksha", "Mahi");
        }

        [Test]
        public void VerifyWeekdayViewGridProperties()
        {
            Assert.IsNotNull(_target.WorkShiftRuleSet);
            Assert.AreEqual(_target.WorkShiftRuleSetName, new Description("Mahinda Rajapaksha", "Mahi"));
            Assert.IsNotNull(_target.Accessibility);
            Assert.IsNotNull(_target.Sunday);
            Assert.IsNotNull(_target.Monday);
            Assert.IsNotNull(_target.Tuesday);
            Assert.IsNotNull(_target.Wednesday);
            Assert.IsNotNull(_target.Thursday);
            Assert.IsNotNull(_target.Friday);
            Assert.IsNotNull(_target.Saturday);
        }

        [Test]
        public void VerifyAccessibilityText()
        {
            Assert.AreEqual(DefaultAccessibility.Excluded, _target.Accessibility);
            Assert.AreEqual("No", _target.AccessibilityText);
            _target.WorkShiftRuleSet.DefaultAccessibility = DefaultAccessibility.Excluded;
            Assert.AreEqual(DefaultAccessibility.Included, _target.Accessibility);
            Assert.AreEqual("Yes", _target.AccessibilityText);
        }
        [Test]
        public void VerifyAccessibility()
        {
            _target.WorkShiftRuleSet.DefaultAccessibility = DefaultAccessibility.Excluded;
            Assert.AreEqual(DefaultAccessibility.Included, _target.Accessibility);
            _target.WorkShiftRuleSet.DefaultAccessibility = DefaultAccessibility.Included;
            Assert.AreEqual(DefaultAccessibility.Excluded, _target.Accessibility);
        }

        [Test]
        public void VerifySetDayOfWeek()
        {
            _target.Sunday = true;
            Assert.AreEqual(_target.Sunday, true);
            _target.Sunday = false;
            Assert.AreEqual(_target.Sunday, false);

            _target.Monday = true;
            Assert.AreEqual(_target.Monday, true);
            _target.Monday = false;
            Assert.AreEqual(_target.Monday, false);

            _target.Tuesday = true;
            Assert.AreEqual(_target.Tuesday, true);
            _target.Tuesday = false;
            Assert.AreEqual(_target.Tuesday, false);

            _target.Wednesday = true;
            Assert.AreEqual(_target.Wednesday, true);
            _target.Wednesday = false;
            Assert.AreEqual(_target.Wednesday, false);

            _target.Thursday = true;
            Assert.AreEqual(_target.Thursday, true);
            _target.Thursday = false;
            Assert.AreEqual(_target.Thursday, false);

            _target.Friday = true;
            Assert.AreEqual(_target.Friday, true);
            _target.Friday = false;
            Assert.AreEqual(_target.Friday, false);
            
            _target.Saturday = true;
            Assert.AreEqual(_target.Saturday, true);
            _target.Saturday = false;
            Assert.AreEqual(_target.Saturday, false);
        }

        [Test]
        public void VerifyValidate()
        {
            Assert.IsTrue(_target.Validate());
        }
    }
}
