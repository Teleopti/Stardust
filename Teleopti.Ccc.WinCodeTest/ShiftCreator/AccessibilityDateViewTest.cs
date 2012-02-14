using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.ShiftCreator;

namespace Teleopti.Ccc.WinCodeTest.ShiftCreator
{
    /// <summary>
    /// Test Class for DatesViewGridAdapter
    /// </summary>
    /// <remarks>
    /// Created by: Aruna Priyankara Wickrama
    /// Created date: 7/18/2008
    /// </remarks>
    [TestFixture]
    public class AccessibilityDateViewTest
    {
        #region Variables

        private AccessibilityDateView _target;
        private WorkShiftRuleSet _base;

        #endregion

        #region Setup and Teardown
        /// <summary>
        /// Tests the init.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        [SetUp]
        public void TestInit()
        {
            _target = new AccessibilityDateView(WorkShiftRuleSetFactory.Create(),new DateTime(2001,1,1,1,1,0,DateTimeKind.Utc));
            _base = WorkShiftRuleSetFactory.Create();
            _target.ContainedEntity = _base;
        }

        /// <summary>
        /// Tests the dispose.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        [TearDown]
        public void TestDispose()
        {
            _target = null;
        }

        #endregion

        #region Tests

        /// <summary>
        /// Verifies the dates view grid properties.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 7/18/2008
        /// </remarks>
        [Test]
        public void VerifyDatesViewGridProperties()
        {
            Assert.IsNotNull(_target.RuleSetName);
            Assert.IsNotNull(_target.Accessibility);
            Assert.IsNotNull(_target.Date);
            Assert.IsNotNull(_target.RuleSet);
        }

        /// <summary>
        /// Verifies the set date.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 7/18/2008
        /// </remarks>
        [Test]
        public void VerifySetDate()
        {
            DateTime setValue = new DateTime(2001, 1, 1, 1, 1, 0, DateTimeKind.Utc);
            _target.Date = setValue;

            DateTime getValue = _target.Date;
            
            Assert.AreEqual(setValue,getValue);

        }

        #endregion
    }
}
