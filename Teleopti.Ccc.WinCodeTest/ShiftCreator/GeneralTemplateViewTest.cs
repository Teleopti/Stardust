using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.ShiftCreator;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.WinCodeTest.ShiftCreator
{
    /// <summary>
    /// Test Class for TemplateViewGrid
    /// </summary>
    /// <remarks>
    /// Created by: Aruna Priyankara Wickrama
    /// Created date: 2008-05-13
    /// </remarks>
    [TestFixture]
    public class GeneralTemplateViewTest
    {
        #region Variables

        private GeneralTemplateView _target;
        private WorkShiftRuleSet _base;
        private IList<IActivity> _activities;

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
            _activities = new List<IActivity>();
            IActivity activityOne = ActivityFactory.CreateActivity("Test");
            _activities.Add(activityOne);
            IActivity activityTwo = ActivityFactory.CreateActivity("A2");
            _activities.Add(activityTwo);
            IActivity activityThree = ActivityFactory.CreateActivity("A3");
            _activities.Add(activityThree);

            _target = new GeneralTemplateView(WorkShiftRuleSetFactory.Create(), _activities, 15);
            _base = WorkShiftRuleSetFactory.Create();
            _base.Description = new Description("Test");
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
        /// Verifies the read only properties.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 8/3/2008
        /// </remarks>
        [Test]
        public void VerifyReadOnlyProperties()
        {
            Assert.IsNotNull(_target.RuleSet);
        }

        /// <summary>
        /// Verifies the rule set.
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-22
        /// </remarks>
        [Test]
        public void VerifyRuleSet()
        {
            const string ruleSetName = "Hada mama udi yathe, ape mama bimin yathe!!!";
            _target.NotifyParent += Target_NotifyParent;
            _target.RuleSet = ruleSetName;
            Assert.AreSame(ruleSetName, _target.RuleSet);
        }

        private static void Target_NotifyParent(object sender, RuleSetRenameEventArgs e)
        {
            
        }

        /// <summary>
        /// Verifies the set base activity.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        [Test]
        public void VerifySetBaseActivity()
        {
            const string activity = "A1";
            _target.BaseActivity = activity;
            Assert.AreSame(activity, _target.BaseActivity);
        }

        /// <summary>
        /// Varifies the set category.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        [Test]
        public void VerifySetCategory()
        {
            ShiftCategory setValue = new ShiftCategory("Test Category");
            _target.Category = setValue;

            // Test get method
            IShiftCategory getValue = _target.Category;

            // Perform Assert Tests
            Assert.AreSame(setValue, getValue);

        }

        /// <summary>
        /// Verifies the set category.
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-25
        /// </remarks>
        [Test]
        public void VerifyAccessibility()
        {
            const string setter = "Included";
            _target.Accessibility = setter;

            // Perform Assert Tests
            Assert.AreEqual(setter, _target.Accessibility);
        }

        /// <summary>
        /// Verifies the set start period start time.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        [Test]
        public void VerifySetStartPeriodStartTime()
        {
            TimeSpan setValue = new TimeSpan(1, 0, 0, 0, 0);
            _target.StartPeriodStartTime = setValue;

            // Test get method
            TimeSpan getValue = _target.StartPeriodStartTime;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);

        }

        [Test]
        public void VerifyRuleSetId()
        {
            Assert.IsNull(_target.RuleSetId);
        }

        /// <summary>
        /// Verifies the set start period end time.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        [Test]
        public void VerifySetStartPeriodEndTime()
        {

            _target.StartPeriodEndTime = TimeSpan.MaxValue;

            // Test get method
            TimeSpan getValue = _target.StartPeriodEndTime;

            // Perform Assert Tests
            Assert.AreEqual(TimeSpan.MaxValue, getValue);

        }

        /// <summary>
        /// Verifies the set start period segment.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        [Test]
        public void VerifySetStartPeriodSegment()
        {
            _target.StartPeriodSegment = TimeSpan.MaxValue;

            // Test get method
            TimeSpan getValue = _target.StartPeriodSegment;

            // Perform Assert Tests
            Assert.AreEqual(TimeSpan.MaxValue, getValue);

        }

        /// <summary>
        /// Verifies the set end period start time.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        [Test]
        public void VerifySetEndPeriodStartTime()
        {
            TimeSpan setValue = new TimeSpan(0, 1, 0, 0);
            _target.EndPeriodStartTime = setValue;

            // Test get method
            TimeSpan getValue = _target.EndPeriodStartTime;

            // Perform Assert Tests
            Assert.AreEqual(getValue, getValue);
        }

        /// <summary>
        /// Verifies the set end period end time.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        [Test]
        public void VerifySetEndPeriodEndTime()
        {
            _target.EndPeriodEndTime = TimeSpan.MaxValue;

            // Test get method
            TimeSpan getValue = _target.EndPeriodEndTime;

            // Perform Assert Tests
            Assert.AreEqual(TimeSpan.MaxValue.Hours, getValue.Hours);
            Assert.AreEqual(TimeSpan.MaxValue.Minutes, getValue.Minutes);
            Assert.AreEqual(TimeSpan.MaxValue.Seconds, getValue.Seconds);
            Assert.AreEqual(TimeSpan.MaxValue.Milliseconds, getValue.Milliseconds);
        }

        /// <summary>
        /// Verifies the set end period segment.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 2008-05-13
        /// </remarks>
        [Test]
        public void VerifySetEndPeriodSegment()
        {
            TimeSpan setValue = new TimeSpan(1);
            _target.EndPeriodSegment = setValue;

            // Test get method
            TimeSpan getValue = _target.EndPeriodSegment;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        /// <summary>
        /// Verifies the set working start time.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 6/16/2008
        /// </remarks>
        [Test]
        public void VerifySetWorkingStartTime()
        {
            TimeSpan setValue = new TimeSpan(1, 0, 0);
            _target.WorkingStartTime = setValue;
            _target.WorkingEndTime = new TimeSpan(2, 0, 0);

            //Test
            TimeSpan getValue = _target.WorkingStartTime;

            //Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        /// <summary>
        /// Verifies the set working End time.
        /// </summary>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 6/16/2008
        /// </remarks>
        [Test]
        public void VerifySetWorkingEndTime()
        {
            TimeSpan setValue = new TimeSpan(2, 0, 0);
            _target.WorkingStartTime = new TimeSpan(1, 0, 0);
            _target.WorkingEndTime = setValue;

            //Test
            TimeSpan getValue = _target.WorkingEndTime;

            //Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        /// <summary>
        /// Verifies the set WorkingSegment.
        /// </summary>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 22/08/2008
        /// </remarks>
        [Test]
        public void VerifySetWorkingSegment()
        {
            TimeSpan setValue = new TimeSpan(1);
            _target.WorkingSegment = setValue;

            TimeSpan getValue = _target.WorkingSegment;

            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyValidate()
        {
            _target.StartPeriodStartTime = new TimeSpan(0);
            bool returnValue = _target.Validate();
            Assert.AreEqual(true, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(1, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(0);
            returnValue = _target.Validate();
            Assert.AreEqual(false, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(5, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(4, 0, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(false, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(5, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(6, 0, 0);
            _target.StartPeriodSegment = TimeSpan.Zero;
            returnValue = _target.Validate();
            Assert.AreEqual(false, returnValue);

            _target.EndPeriodStartTime = new TimeSpan(3, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(2, 0, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(false, returnValue);

            _target.EndPeriodStartTime = new TimeSpan(3, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(4, 0, 0);
            _target.EndPeriodSegment = TimeSpan.Zero;
            returnValue = _target.Validate();
            Assert.AreEqual(false, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(5, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(6, 0, 0);
            _target.StartPeriodSegment = new TimeSpan(0, 15, 0);
            _target.EndPeriodStartTime = new TimeSpan(15, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(16, 0, 0);
            _target.EndPeriodSegment = new TimeSpan(0, 15, 0);

            _target.WorkingStartTime = new TimeSpan(10, 0, 0);
            _target.WorkingEndTime = new TimeSpan(9, 0, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(false, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(5, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(6, 0, 0);
            _target.StartPeriodSegment = new TimeSpan(0, 15, 0);
            _target.EndPeriodStartTime = new TimeSpan(15, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(16, 0, 0);
            _target.EndPeriodSegment = new TimeSpan(0, 15, 0);

            _target.WorkingStartTime = new TimeSpan(8, 0, 0);
            _target.WorkingEndTime = new TimeSpan(9, 0, 0);
            _target.WorkingSegment = TimeSpan.Zero;
            returnValue = _target.Validate();
            Assert.AreEqual(false, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(5, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(6, 0, 0);
            _target.EndPeriodStartTime = new TimeSpan(4, 0, 0);
            _target.EndPeriodEndTime = TimeSpan.Zero;
            _target.WorkingSegment = new TimeSpan(0, 15, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(true, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(5, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(6, 0, 0);
            _target.EndPeriodStartTime = new TimeSpan(15, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(16, 0, 0);
            _target.WorkingSegment = new TimeSpan(0, 15, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(true, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(5, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(6, 0, 0);
            _target.EndPeriodStartTime = new TimeSpan(13, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(14, 0, 0);
            _target.WorkingSegment = new TimeSpan(0, 15, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(true, returnValue);

            _target.EndPeriodEndTime = new TimeSpan(3, 0, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(false, returnValue);
        }

        #endregion

    }
}
