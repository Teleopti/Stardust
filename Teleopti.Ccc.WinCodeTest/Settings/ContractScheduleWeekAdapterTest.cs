using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;

namespace Teleopti.Ccc.WinCodeTest.Settings
{
    [TestFixture]
    public class ContractScheduleWeekAdapterTest
    {

        #region Fields - Instance Member

        ContractScheduleWeek _containedEntity;

        private ContractScheduleWeekAdapter _targetAdapter;

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - ContractScheduleWeekAdapterTest Members

        #endregion

        #endregion

        #region Events - Instance Member

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - AvailabilityRestrictionViewTest Members

        [SetUp]
        public void Init()
        {
           _containedEntity = new ContractScheduleWeek();

           _containedEntity.Add(DayOfWeek.Sunday, false);
           _containedEntity.Add(DayOfWeek.Monday, false);
           _containedEntity.Add(DayOfWeek.Tuesday, false);
           _containedEntity.Add(DayOfWeek.Wednesday, false);
           _containedEntity.Add(DayOfWeek.Thursday, false);
           _containedEntity.Add(DayOfWeek.Friday, false);
           _containedEntity.Add(DayOfWeek.Saturday, false);

            _targetAdapter = new ContractScheduleWeekAdapter(_containedEntity);
        }

        [Test]
        public void VerifyCanSetTargetEntity()
        {
            Assert.AreEqual(_containedEntity, _targetAdapter.ContainedEntity);
        }


        [Test]
        public void VerifyCanSetSunday()
        {
            _targetAdapter.Sunday = true;
            Assert.AreEqual(true , _targetAdapter.Sunday);
        }

        [Test]
        public void VerifyCanSetMonday()
        {
            _targetAdapter.Monday = true;
            Assert.AreEqual(true, _targetAdapter.Monday);
        }

        [Test]
        public void VerifyCanSetTuesday()
        {
            _targetAdapter.Tuesday = true;
            Assert.AreEqual(true, _targetAdapter.Tuesday);
        }

        [Test]
        public void VerifyCanSetWednesday()
        {
            _targetAdapter.Wednesday = true;
            Assert.AreEqual(true, _targetAdapter.Wednesday);
        }

        [Test]
        public void VerifyCanSetThursday()
        {
            _targetAdapter.Thursday = true;
            Assert.AreEqual(true, _targetAdapter.Thursday);
        }

        [Test]
        public void VerifyCanSetFriday()
        {
            _targetAdapter.Friday = true;
            Assert.AreEqual(true, _targetAdapter.Friday);
        }

        [Test]
        public void VerifyCanSetSaturday()
        {
            _targetAdapter.Saturday = true;
            Assert.AreEqual(true, _targetAdapter.Saturday);
        }

        #endregion

        #endregion
    }
}
