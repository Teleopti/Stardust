#region Imports

using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;

#endregion

namespace Teleopti.Ccc.WinCodeTest.Settings
{

    /// <summary>
    /// Represtns the test class for the absence view.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-12-03
    /// </remarks>
    [TestFixture]
    public class AbsenceViewTest
    {

 
        /// <summary>
        /// Holds teh absence instance for the absnece view
        /// </summary>
        private IAbsence _absence;

        /// <summary>
        /// Holds the absence view instance.
        /// </summary>
        private AbsenceView _absenceView;
        


        /// <summary>
        /// Inits the test.
        /// </summary>
        /// <remarks>
        /// Created by: Savani irashaN
        /// Created date: 2008-12-03
        /// </remarks>
        [SetUp]
        public void InitTest()
        {
            _absence = new Absence();
            _absence.SetId(Guid.NewGuid());
            _absence.InContractTime = true;
            _absence.InPaidTime = true;
            _absence.InWorkTime = true;
            _absence.Description = new Description("a", "b");
            _absence.DisplayColor = Color.DarkTurquoise;
            _absence.Requestable = true;
            _absence.Priority = 11;
            _absence.PayrollCode = "aabb";
            _absenceView = new AbsenceView(_absence, true);
        }
        


        /// <summary>
        /// Disposes the test.
        /// </summary>
        /// <remarks>
        /// Created by: SavaniN
        /// Created date: 2008-12-03
        /// </remarks>
        [TearDown]
        public void DisposeTest()
        {
            _absence = null;
            _absenceView = null;
        }

        [Test]
        public void VerifyPropertiesAreTransferredToView()
        {
            Assert.AreEqual(_absence.InContractTime, _absenceView.InContractTime);
            Assert.AreEqual(_absence.InPaidTime, _absenceView.InPaidTime);
            Assert.AreEqual(_absence.InWorkTime, _absenceView.InWorkTime);
            Assert.AreEqual(_absence.Description, _absenceView.Description);
            Assert.AreEqual(_absence.DisplayColor, _absenceView.DisplayColor);
            Assert.AreEqual(_absence.GetOrFillWithBusinessUnit_DONTUSE(), _absenceView.BusinessUnit);
            Assert.AreEqual(_absence.Requestable, _absenceView.Requestable);
            Assert.AreEqual(_absence.Priority, _absenceView.Priority);
            Assert.AreEqual(_absence.PayrollCode,_absenceView.PayrollCode );
            Assert.IsTrue(_absenceView.IsTrackerDisabled);
            Assert.AreEqual(_absenceView.ContainedEntity, _absenceView.ContainedOriginalEntity);
            Assert.AreNotSame(_absenceView.ContainedEntity, _absenceView.ContainedOriginalEntity);

            
        }


        [Test]
        public void VerifyCanGetUpdateTimeText()
        {
            Assert.AreEqual(string.Empty, _absenceView.UpdatedTimeInUserPerspective);
        }

        /// <summary>
        /// Verifies that properties can set.
        /// </summary>
        /// <remarks>
        /// Created by: SavaniN
        /// Created date: 2008-12-03
        /// </remarks>
        [Test]
        public void TestCanSetProperties()
        {
            Description description = new Description("Test Absence", "TA");

            _absenceView.DisplayColor = Color.BlueViolet;
            _absenceView.Description = description;
            _absenceView.Priority = 50;
            _absenceView.InWorkTime = false;
            _absenceView.InPaidTime = false;
            _absenceView.InContractTime = false;
            _absenceView.Requestable = false;
            _absenceView.PayrollCode = "ddee";

            Assert.AreEqual(Color.BlueViolet.ToArgb(), _absenceView.DisplayColor.ToArgb());
            Assert.AreEqual("Test Absence", _absenceView.Description.Name);
            Assert.AreEqual("TA", _absenceView.Description.ShortName);
            Assert.AreEqual(50, _absenceView.Priority);
            Assert.AreEqual(string.Empty, _absenceView.UpdatedBy);
            Assert.AreEqual(_absenceView.UpdatedOn, _absence.UpdatedOn);
            Assert.AreEqual("ddee",_absenceView.PayrollCode );

            Assert.IsFalse(_absenceView.Requestable);
            Assert.AreEqual(description, _absenceView.Description);
            Assert.IsFalse(_absenceView.InWorkTime);
            Assert.IsFalse(_absenceView.InPaidTime);
            Assert.IsFalse(_absenceView.InContractTime);
            Assert.IsFalse(_absenceView.Requestable);
        }

        /// <summary>
        /// Verifies the values can change.
        /// </summary>
        /// <remarks>
        /// Created by: SavaniN
        /// Created date: 2008-12-03
        /// </remarks>
        [Test]
        public void VerifyValuesCanChange()
        {
            bool getValue;
            bool setValue;

            getValue = _absenceView.InContractTime;
            setValue = !getValue;
            _absenceView.InContractTime = setValue;

            getValue = _absenceView.InContractTime;
            Assert.AreEqual(setValue, getValue);

            Assert.AreEqual(_absenceView.Tracker.Description, TrackerView.DefaultTracker.Description);

            TrackerView trackerView = new TrackerView(Tracker.CreateDayTracker());

            _absenceView.Tracker = trackerView;
            Assert.AreEqual(_absenceView.Tracker.Description, trackerView.Description);

            trackerView.Tracker = TrackerView.DefaultTracker;
            _absenceView.Tracker = trackerView;
            Assert.IsNull(_absenceView.ContainedEntity.Tracker);
        }

        [Test]
        public void VerifyAfterMerge()
        {
            IAbsence updatedAbsence = _absence.EntityClone();

            updatedAbsence.Description = new Description("new fine desc");

            _absenceView.UpdateAfterMerge(updatedAbsence);

            Assert.AreEqual(_absenceView.ContainedEntity, _absenceView.ContainedOriginalEntity);
            Assert.AreNotSame(_absenceView.ContainedEntity, _absenceView.ContainedOriginalEntity);
            Assert.AreEqual(_absenceView.Description, updatedAbsence.Description);
        }

        [Test]
        public void VerifyDiscardChanges()
        {
            _absenceView.Description = new Description("something...");
            _absenceView.ResetAbsenceState(null, _absenceView.IsTrackerDisabled);

            Assert.AreEqual(_absenceView.Description, _absence.Description);
            Assert.AreEqual(_absenceView.ContainedEntity, _absenceView.ContainedOriginalEntity);
            Assert.AreNotSame(_absenceView.ContainedEntity, _absenceView.ContainedOriginalEntity);


            IAbsence origAbsence = _absenceView.ContainedOriginalEntity;
            origAbsence.Tracker = Tracker.CreateTimeTracker();
            _absenceView = new AbsenceView(origAbsence, true);

            _absenceView.ResetAbsenceState(_absenceView.ContainedEntity, _absenceView.IsTrackerDisabled);

            Assert.IsTrue(_absenceView.IsTrackerDisabled);
            Assert.AreEqual(_absenceView.ContainedEntity, _absenceView.ContainedOriginalEntity);
            Assert.AreNotSame(_absenceView.ContainedEntity, _absenceView.ContainedOriginalEntity);
            Assert.AreEqual(_absenceView.ContainedEntity, origAbsence);
        }
    }

}
