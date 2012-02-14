using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.ShiftCreator
{
    [TestFixture]
    public class DaysOfWeekViewTest
    {
        #region Variables

        private DaysOfWeekView _target;
        private WorkShiftRuleSet _base;
        #endregion

        #region SetUp and TearDown
        [SetUp]
        public void TestInit()
        {
            _base = WorkShiftRuleSetFactory.Create();
            ((IEntity)_base).SetId(Guid.NewGuid());

            _target = new DaysOfWeekView(_base);

        }

        [TearDown]
        public void TestDispose()
        {
            _base = null;
            _target = null;
        }
        #endregion

        #region Tests
        /// <summary>
        /// Sets the Id.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        [Test]
        public void VerifyGetIndex()
        {

            Assert.IsNotNull(_target.Id);

        }


        /// <summary>
        /// Verifies the week day view grid properties.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        [Test]
        public void VerifyWeekdayViewGridProperties()
        {
            Assert.IsNotNull(_target.RuleSet);
            Assert.IsNotNull(_target.Accessibility);
            Assert.IsNotNull(_target.Sunday);
            Assert.IsNotNull(_target.Monday);
            Assert.IsNotNull(_target.Tuesday);
            Assert.IsNotNull(_target.Wednesday);
            Assert.IsNotNull(_target.Thursday);
            Assert.IsNotNull(_target.Friday);
            Assert.IsNotNull(_target.Saturday);

        }

        /// <summary>
        /// Verifies the set accessibility.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        [Test]
        public void VerifySetAccessibility()
        {
            DefaultAccessibility setValue = new DefaultAccessibility();
            _target.Accessibility = setValue;

            DefaultAccessibility getValue = _target.Accessibility;

            Assert.AreEqual(setValue, getValue);

        }
        /// <summary>
        /// Verifies the set day of week.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
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


        #endregion

    }
}
