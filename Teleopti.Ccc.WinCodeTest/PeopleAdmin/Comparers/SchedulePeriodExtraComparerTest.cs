#region Imports

using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;


#endregion

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
    /// <summary>
    /// Test class for the <see cref="SchedulePeriodExtraComparer"/> class 
    /// </summary>
    [TestFixture]
    public class SchedulePeriodExtraComparerTest
    {
        #region Variables

        private SchedulePeriodModel _adapter1, _adapter2;
        private SchedulePeriodExtraComparer _target;
        private ISchedulePeriod _schedulePeriod1, _schedulePeriod2;
        private int _result;
        private IPerson _person1;
        private IPerson _person2;
        private DateOnly _from1;
        private DateOnly _from2;

        #endregion

        #region SetUp

        [SetUp]
        public void Setup()
        {
            _person1 = PersonFactory.CreatePerson("Person1");
            _person2 = PersonFactory.CreatePerson("Person2");

            _from1 = new DateOnly(2008, 1, 3);
            _from2 = new DateOnly(2010, 1, 3);

            _schedulePeriod1 = SchedulePeriodFactory.CreateSchedulePeriod(_from1);
            _schedulePeriod2 = SchedulePeriodFactory.CreateSchedulePeriod(_from2);

            _person1.AddSchedulePeriod(_schedulePeriod1);
            _person2.AddSchedulePeriod(_schedulePeriod2);

            _adapter1 = new SchedulePeriodModel(_from1, _person1, null);
            _adapter2 = new SchedulePeriodModel(_from2, _person2, null);
        }

        #endregion

        #region Test

        [Test]
        public void VerifyCompareMethodWithZeroAndPositive()
        {
            _schedulePeriod1.Extra = new TimeSpan(1, 1, 1);
            _schedulePeriod2.Extra = TimeSpan.Zero;

            // Calls the compares method
            _target = new SchedulePeriodExtraComparer();
            _result = _target.Compare(_adapter1, _adapter2);

            // Checks whether the roles are equal
            Assert.AreEqual(1, _result);
        }

        [Test]
        public void VerifyCompareMethodWithZeroAndNegative()
        {
            _schedulePeriod1.Extra = TimeSpan.Zero;
            _schedulePeriod2.Extra = new TimeSpan(-1, 1, 1);

            // Calls the compares method
            _target = new SchedulePeriodExtraComparer();
            _result = _target.Compare(_adapter1, _adapter2);

            // Checks whether the roles are equal
            Assert.AreEqual(1, _result);
        }

        [Test]
        public void VerifyCompareMethodAscending()
        {
            _schedulePeriod1.Extra = new TimeSpan(-1, 1, 1);
            _schedulePeriod2.Extra = new TimeSpan(2, 1, 1);

            // Calls the compares method
            _target = new SchedulePeriodExtraComparer();
            _result = _target.Compare(_adapter1, _adapter2);

            // Checks whether the roles are equal
            Assert.AreEqual(-1, _result);
        }

        [Test]
        public void VerifyCompareMethodDescending()
        {
            _schedulePeriod1.Extra = new TimeSpan(2, 1, 1);
            _schedulePeriod2.Extra = new TimeSpan(1, 1, 1);

            // Calls the compares method
            _target = new SchedulePeriodExtraComparer();
            _result = _target.Compare(_adapter1, _adapter2);

            // Checks whether the roles are equal
            Assert.AreEqual(1, _result);
        }

        [Test]
        public void VerifyCompareMethodSame()
        {
            _schedulePeriod1.Extra = new TimeSpan(1, 1, 1);
            _schedulePeriod2.Extra = new TimeSpan(1, 1, 1);

            // Calls the compares method
            _target = new SchedulePeriodExtraComparer();
            _result = _target.Compare(_adapter1, _adapter2);

            // Checks whether the roles are equal
            Assert.AreEqual(0, _result);
        }

        #endregion
    }
}
