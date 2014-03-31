using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    /// <summary>
    /// Tests for the Activity class
    /// </summary>
    [TestFixture, SetUICulture("en-GB")]
    public class ActivityTest
    {
        private IActivity _target;
        private List<IActivity> _list;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _target = new Activity("TestActivity");
            _list = new List<IActivity>();
        }
        [Test]
        public void VerifySetWorkTimeAndPaidTime()
        {
            _target.InWorkTime = true;
            _target.InPaidTime = true;

            Assert.IsTrue(_target.InWorkTime);
            Assert.IsTrue(_target.InPaidTime);
        }

        /// <summary>
        /// Verify that new and properties work
        /// </summary>
        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual("TestActivity", _target.Description.Name);
            Assert.AreEqual(0, _target.DisplayColor.ToArgb());
            Assert.IsNull(_target.UpdatedBy);
            Assert.IsNull(_target.UpdatedOn);
            Assert.IsTrue(_target.InContractTime);
        }

        [Test]
        public void CanSetIsReadyTime()
        {
            _target.InReadyTime = true;
            Assert.AreEqual(true, _target.InReadyTime);
        }

        /// <summary>
        /// Verifies the name of the can set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        [Test]
        public void VerifyCanSetName()
        {
            _target.Description = new Description("test");

            Assert.AreEqual("test",_target.Description.Name);
            Assert.AreEqual("test", _target.Name);
        }

        /// <summary>
        /// Verifies the protected constructor works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        [Test]
        public void VerifyProtectedConstructorWorks()
        {
            MockRepository mocks = new MockRepository();
            Activity internalActivity = mocks.StrictMock<Activity>();
            Assert.IsNotNull(internalActivity);
        }

        /// <summary>
        /// Verifies the unpaid property works.
        /// </summary>
        [Test]
        public void VerifyInContractTime()
        {
            bool getValue;
            bool setValue;

            getValue = _target.InContractTime;
            setValue = !getValue;
            _target.InContractTime = setValue;

            getValue = _target.InContractTime;
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyToString()
        {
            Assert.AreEqual("TestActivity, Activity, no id", _target.ToString());
        }

        [Test]
        public void VerifyCanSetGetTracker()
        {
            _target.Tracker = Tracker.CreateDayTracker();
            Assert.AreEqual(_target.Tracker,Tracker.CreateDayTracker());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifySort()
        {
            IActivity a = new Activity("a");
            IActivity b = new Activity("b");
            IActivity c = new Activity("c");

            _list.Add(b);
            _list.Add(c);
            _list.Add(a);

            _list.Sort(new ActivitySorter());

            Assert.AreSame(a, _list[0]);
            Assert.AreSame(b, _list[1]);
            Assert.AreSame(c, _list[2]);
        }

        [Test]
        public void VerifyShortBreakLunch()
        {
            Assert.AreEqual(ReportLevelDetail.None, _target.ReportLevelDetail);
            _target.ReportLevelDetail = ReportLevelDetail.Lunch;
            Assert.AreEqual(ReportLevelDetail.Lunch, _target.ReportLevelDetail);
        }

		[Test]
		public void ShouldHaveRequiresSeat()
		{
			Assert.That(_target.RequiresSeat,Is.False);
			_target.RequiresSeat = true;
			Assert.That(_target.RequiresSeat, Is.True);
		}

        [Test]
        public void VerifyDeletedText()
        {
            _target.SetDeleted();
            Assert.AreEqual("TestActivity <Deleted>", _target.Description.ToString());
        }

        [Test]
        public void ShouldTruncateDescriptionWhenTooLongWithDeletedText()
        {
            _target.Description = new Description("a".PadRight(50,'a'));
            _target.SetDeleted();
            Assert.AreEqual(" <Deleted>".PadLeft(50,'a'), _target.Description.ToString());
        }
    }
}