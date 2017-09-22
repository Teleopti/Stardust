using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture, SetUICulture("en-US")]
    public class AccessibilityDateViewModelTest
    {
        #region Variables

        private IAccessibilityDateViewModel _target;

        #endregion

        #region Setup and Teardown

        [SetUp]
        public void TestInit()
        {
            _target = new AccessibilityDateViewModel(WorkShiftRuleSetFactory.Create(), new DateTime(2001, 1, 1, 1, 1, 0, DateTimeKind.Utc));
        }

        [TearDown]
        public void TestDispose()
        {
            _target = null;
        }

        #endregion

        #region Tests

        [Test]
        public void VerifyDatesViewGridProperties()
        {
            Assert.IsNotNull(_target.WorkShiftRuleSet);
            Assert.IsNotNull(_target.Accessibility);
            Assert.IsNotNull(_target.Date);
            Assert.IsNotNull(_target.WorkShiftRuleSetName);
            Assert.IsNotNull(_target.AccessibilityText);
        }

        [Test]
        public void VerifySetDate()
        {
            DateTime setValue = new DateTime(2001, 1, 1);
            _target.Date = setValue;

            DateTime getValue = _target.Date;

            Assert.AreEqual(setValue, getValue);

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

        #endregion
    }
}
