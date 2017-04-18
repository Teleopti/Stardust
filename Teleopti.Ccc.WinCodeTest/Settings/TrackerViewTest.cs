#region Imports

using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.WinCodeTest.Settings
{

    /// <summary>
    /// Represents the test class for the tracker view.
    /// </summary>
    /// <remarks>
    /// Created by: SavaniN
    /// Created date: 2008-12-03
    /// </remarks>
    [TestFixture]
    public class TrackerViewTest
    {

        #region Fields - Instance Member

        /// <summary>
        /// Holds the tracker instance.
        /// </summary>
        private ITracker _tracker;

        /// <summary>
        /// Holds the tracker view for the tracker instance.
        /// </summary>
        private TrackerView _trackerView;

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - TrackerViewTest Members

        #endregion

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - TrackerViewTest Members

        #region Set Up

        /// <summary>
        /// Inits the test.
        /// </summary>
        /// <remarks>
        /// Created by: SavaniN
        /// Created date: 2008-12-03
        /// </remarks>
        [SetUp]
        public void InitTest()
        {
            _tracker = Tracker.CreateDayTracker();
            _trackerView = new TrackerView(_tracker);
        }

        #endregion

        #region Tear Down

        /// <summary>
        /// Disposes the test.
        /// </summary>
        /// <remarks>
        /// Created by: SavaniN
        /// Created date: 2008-12-03
        /// </remarks>
        [Test]
        public void DisposeTest()
        {
            _tracker = null;
            _trackerView = null;
        }

        #endregion

        #region Test

        /// <summary>
        /// Verifies that an instance can be created.
        /// </summary>
        /// <remarks>
        /// Created by: SavaniN
        /// Created date: 2008-12-03
        /// </remarks>
        [Test]
        public void TestCanCreate()
        {
            Assert.IsNotNull(_trackerView);
        }

        /// <summary>
        /// Tests that can read properties.
        /// </summary>
        /// <remarks>
        /// Created by: SavaniN
        /// Created date: 2008-12-03
        /// </remarks>
        [Test]
        public void TestCanReadProperties()
        {
            Assert.AreEqual(_trackerView.Description, _tracker.Description);
            Assert.AreEqual(_trackerView.Tracker, _tracker);

            _tracker = Tracker.CreateCompTracker();
            _trackerView.Tracker = _tracker;
            Assert.AreEqual(_trackerView.Tracker, Tracker.CreateCompTracker());

            Assert.AreEqual(TrackerView.DefaultTracker.Description.Name, UserTexts.Resources.DefaultTrackerDefaultDescription);
        }



        #endregion

        #endregion

        #endregion

    }

}
