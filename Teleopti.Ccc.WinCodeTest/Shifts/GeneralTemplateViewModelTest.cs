using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class GeneralTemplateViewModelTest
    {
        #region Variables

        private GeneralTemplateViewModel _target;

        #endregion

        #region Setup and Teardown

        [SetUp]
        public void TestInit()
        {
            _target = new GeneralTemplateViewModel(WorkShiftRuleSetFactory.Create(), 15);
        }

        [TearDown]
        public void TestDispose()
        {
            _target = null;
        }

        #endregion

        #region Tests

        [Test]
        public void VerifyReadOnlyProperties()
        {
            Assert.IsNotNull(_target.WorkShiftRuleSet);
        }

        /// <summary>
        /// Verifies the rule set.
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-22
        /// </remarks>
        //[Test]
        //public void VerifyRuleSet()
        //{
        //    const string ruleSetName = "Hada mama udi yathe, ape mama bimin yathe!!!";
        //    _target.NotifyParent += Target_NotifyParent;
        //    _target.RuleSet = ruleSetName;
        //    Assert.AreSame(ruleSetName, _target.RuleSet);
        //}

        //private static void Target_NotifyParent(object sender, RuleSetRenameEventArgs e)
        //{

        //}

        [Test]
        public void VerifySetBaseActivity()
        {
            IActivity activity = ActivityFactory.CreateActivity("Builing");
            _target.BaseActivity = activity;
            Assert.AreSame(activity, _target.BaseActivity);
        }

        [Test]
        public void VerifySetCategory()
        {
            IShiftCategory setValue = new ShiftCategory("Test Category");
            _target.Category = setValue;
            IShiftCategory getValue = _target.Category;
            Assert.AreSame(setValue, getValue);

        }

		[Test]
		public void ShouldSetGetOnlyForRestrictions()
		{
			_target.OnlyForRestrictions = true;
			Assert.IsTrue(_target.OnlyForRestrictions);
		}

        [Test]
        public void VerifyAccessibility()
        {
            _target.Accessibility = Enum.GetName(typeof(DefaultAccessibility), DefaultAccessibility.Included);
            Assert.AreEqual(Enum.GetName(typeof(DefaultAccessibility), DefaultAccessibility.Included), _target.Accessibility);
        }

        [Test]
        public void VerifySetStartPeriodStartTime()
        {
            TimeSpan setValue = new TimeSpan(1, 0, 0, 0, 0);
            _target.StartPeriodStartTime = setValue;
            TimeSpan getValue = _target.StartPeriodStartTime;
            Assert.AreEqual(setValue, getValue);

        }

        [Test]
        public void VerifySetStartPeriodEndTime()
        {
            _target.StartPeriodEndTime = TimeSpan.MaxValue;
            TimeSpan getValue = _target.StartPeriodEndTime;
            Assert.AreEqual(TimeSpan.MaxValue, getValue);
        }

        [Test]
        public void VerifySetStartPeriodSegment()
        {
            _target.StartPeriodSegment = TimeSpan.MaxValue;
            TimeSpan getValue = _target.StartPeriodSegment;
            Assert.AreEqual(TimeSpan.MaxValue, getValue);
        }
        
        [Test]
        public void VerifySetEndPeriodStartTime()
        {
            TimeSpan setValue = new TimeSpan(0, 1, 0, 0);
            _target.EndPeriodStartTime = setValue;
            TimeSpan getValue = _target.EndPeriodStartTime;
            Assert.AreEqual(getValue, getValue);
        }

        [Test]
        public void VerifySetEndPeriodEndTime()
        {
            _target.EndPeriodEndTime = TimeSpan.MaxValue;
            TimeSpan getValue = _target.EndPeriodEndTime;
            Assert.AreEqual(TimeSpan.MaxValue.Hours, getValue.Hours);
            Assert.AreEqual(TimeSpan.MaxValue.Minutes, getValue.Minutes);
            Assert.AreEqual(TimeSpan.MaxValue.Seconds, getValue.Seconds);
            Assert.AreEqual(TimeSpan.MaxValue.Milliseconds, getValue.Milliseconds);
        }

        [Test]
        public void VerifySetEndPeriodSegment()
        {
            TimeSpan setValue = new TimeSpan(1);
            _target.EndPeriodSegment = setValue;
            TimeSpan getValue = _target.EndPeriodSegment;
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifySetWorkingStartTime()
        {
            TimeSpan setValue = new TimeSpan(1, 0, 0);
            _target.WorkingStartTime = setValue;
            _target.WorkingEndTime = new TimeSpan(2, 0, 0);
            TimeSpan getValue = _target.WorkingStartTime;
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifySetWorkingEndTime()
        {
            TimeSpan setValue = new TimeSpan(2, 0, 0);
            _target.WorkingStartTime = new TimeSpan(1, 0, 0);
            _target.WorkingEndTime = setValue;
            TimeSpan getValue = _target.WorkingEndTime;
            Assert.AreEqual(setValue, getValue);
        }

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


            //Borde vara false
            _target.StartPeriodStartTime = new TimeSpan(5, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(6, 0, 0);
            _target.EndPeriodStartTime = new TimeSpan(4, 0, 0);
            _target.EndPeriodEndTime = TimeSpan.Zero;
            _target.WorkingSegment = new TimeSpan(0, 15, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(false, returnValue);

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

            _target.StartPeriodStartTime = new TimeSpan(5, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(6, 0, 0);
            _target.EndPeriodStartTime = new TimeSpan(4, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(6, 0, 0);
            _target.WorkingSegment = new TimeSpan(0, 15, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(false, returnValue);


            //Borde vara false
            _target.StartPeriodStartTime = new TimeSpan(21, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(22, 0, 0);
            _target.EndPeriodStartTime = new TimeSpan(20, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(1, 6, 0, 0);
            _target.WorkingSegment = new TimeSpan(0, 15, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(false, returnValue);

            //borde vara true
            _target.StartPeriodStartTime = new TimeSpan(5, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(6, 0, 0);
            _target.EndPeriodStartTime = new TimeSpan(6, 0, 0);
            _target.EndPeriodEndTime = TimeSpan.Zero;
            _target.WorkingSegment = new TimeSpan(0, 15, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(true, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(5, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(6, 0, 0);
            _target.EndPeriodStartTime = new TimeSpan(6, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(7, 0, 0);
            _target.WorkingSegment = new TimeSpan(0, 15, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(true, returnValue);

            //Borde vara false cannot start after midnight
            _target.StartPeriodStartTime = new TimeSpan(22, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(1, 0, 0, 0);
            _target.EndPeriodStartTime = new TimeSpan(1, 6, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(1, 7, 0, 0);
            _target.WorkingSegment = new TimeSpan(0, 15, 0);
            returnValue = _target.Validate();
            Assert.AreEqual(false, returnValue);
        }

        [Test]
        public void VerifyValidateEarlyStartTime()
        {
            _target.StartPeriodStartTime = new TimeSpan(0);
            bool returnValue = _target.ValidateEarlyStartTime();
            Assert.AreEqual(true, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(0, 23, 59, 59);
            returnValue = _target.ValidateEarlyStartTime();
            Assert.AreEqual(true, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(1, 0, 0, 0);
            returnValue = _target.ValidateEarlyStartTime();
            Assert.AreEqual(false, returnValue);
        }

        [Test]
        public void VerifyValidateLateStartTime()
        {
            _target.StartPeriodEndTime = new TimeSpan(0);
            bool returnValue = _target.ValidateLateStartTime();
            Assert.AreEqual(true, returnValue);

            _target.StartPeriodEndTime = new TimeSpan(0, 23, 59, 59);
            returnValue = _target.ValidateLateStartTime();
            Assert.AreEqual(true, returnValue);

            _target.StartPeriodEndTime = new TimeSpan(1, 0, 0, 0);
            returnValue = _target.ValidateLateStartTime();
            Assert.AreEqual(false, returnValue);
        }


        [Test]
        public void VerifyValidateEarlyEndTime()
        {

            _target.EndPeriodStartTime = new TimeSpan(0);
            bool returnValue = _target.ValidateEarlyEndTime();
            Assert.AreEqual(true, returnValue);

            _target.EndPeriodStartTime = new TimeSpan(1, 23, 59, 59);
            returnValue = _target.ValidateEarlyEndTime();
            Assert.AreEqual(true, returnValue);


            _target.EndPeriodStartTime = new TimeSpan(2, 0, 0, 0);
            returnValue = _target.ValidateEarlyEndTime();
            Assert.AreEqual(false, returnValue);
        }


        [Test]
        public void VerifyValidateLateEndTime()
        {

            _target.EndPeriodEndTime = new TimeSpan(0);
            bool returnValue = _target.ValidateLateEndTime();
            Assert.AreEqual(true, returnValue);

            _target.EndPeriodEndTime = new TimeSpan(1, 23, 59, 59);
            returnValue = _target.ValidateLateEndTime();
            Assert.AreEqual(true, returnValue);


            _target.EndPeriodEndTime = new TimeSpan(2, 0, 0, 0);
            returnValue = _target.ValidateLateEndTime();
            Assert.AreEqual(false, returnValue);
        }

        [Test]
        public void VerifyValidateStartTimes()
        {
            _target.StartPeriodStartTime = new TimeSpan(0);
            _target.StartPeriodEndTime = new TimeSpan(0);
            bool returnValue = _target.ValidateStartTimes();
            Assert.AreEqual(true, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(2,0,0);
            _target.StartPeriodEndTime = new TimeSpan(1,0,0);
             returnValue = _target.ValidateStartTimes();
            Assert.AreEqual(false, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(2, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(3, 0, 0);
            returnValue = _target.ValidateStartTimes();
            Assert.AreEqual(true, returnValue);
        }

        [Test]
        public void VerifyValidateEndTimes()
        {
            _target.EndPeriodStartTime = new TimeSpan(0);
            _target.EndPeriodEndTime = new TimeSpan(0);
            bool returnValue = _target.ValidateEndTimes();
            Assert.AreEqual(true, returnValue);

            _target.EndPeriodStartTime = new TimeSpan(1,2, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(1,1, 0, 0);
            returnValue = _target.ValidateEndTimes();
            Assert.AreEqual(false, returnValue);

            _target.EndPeriodStartTime = new TimeSpan(0, 2, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(0, 3, 0, 0);
            returnValue = _target.ValidateEndTimes();
            Assert.AreEqual(true, returnValue);

            _target.EndPeriodStartTime = new TimeSpan(1, 23, 59, 59);
            _target.EndPeriodEndTime = new TimeSpan(1,23, 59, 59);
            returnValue = _target.ValidateEndTimes();
            Assert.AreEqual(true, returnValue);

            _target.EndPeriodStartTime = new TimeSpan(2, 0, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(2, 0, 0, 0);
            returnValue = _target.ValidateEndTimes();
            Assert.AreEqual(false, returnValue);
        }



        [Test]
        public void VerifyValidateStartEndTimes()
        {
            _target.StartPeriodStartTime = new TimeSpan(0);
            _target.StartPeriodEndTime = new TimeSpan(0);
            _target.EndPeriodStartTime = new TimeSpan(0);
            _target.EndPeriodEndTime = new TimeSpan(0);
            bool returnValue = _target.ValidateStartEndTimes();
            Assert.AreEqual(true, returnValue);



            _target.StartPeriodStartTime = new TimeSpan( 23, 59, 59);
            _target.StartPeriodEndTime = new TimeSpan( 23, 59, 59);
            _target.EndPeriodStartTime = new TimeSpan(1, 0, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(1, 0, 0, 0);
            returnValue = _target.ValidateStartEndTimes();
            Assert.AreEqual(true, returnValue);


            _target.StartPeriodStartTime = new TimeSpan(0);
            _target.StartPeriodEndTime = new TimeSpan(23, 59, 59);
            _target.EndPeriodStartTime = new TimeSpan(23, 0,  0);
            _target.EndPeriodEndTime = new TimeSpan(1, 0, 0, 0);
            returnValue = _target.ValidateStartEndTimes();
            Assert.AreEqual(true, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(0);
            _target.StartPeriodEndTime = new TimeSpan(23, 59, 59);
            _target.EndPeriodStartTime = new TimeSpan(0);
            _target.EndPeriodEndTime = new TimeSpan(1, 0, 0, 0);
            returnValue = _target.ValidateStartEndTimes();
            Assert.AreEqual(true, returnValue);

            _target.StartPeriodStartTime = new TimeSpan(4, 0, 0);
            _target.StartPeriodEndTime = new TimeSpan(20, 0, 0);
            _target.EndPeriodStartTime = new TimeSpan(5, 0, 0);
            _target.EndPeriodEndTime = new TimeSpan(19, 0, 0);
            returnValue = _target.ValidateStartEndTimes();
            Assert.AreEqual(false, returnValue);
        }


        [Test]
        public void VerifyValidateWorkingTimeLength()
        {

            _target.WorkingStartTime = new TimeSpan(0);
            _target.WorkingEndTime = new TimeSpan(0);
            bool returnValue = _target.ValidateWorkingTimeLength();
            Assert.AreEqual(false, returnValue);

            _target.WorkingStartTime = new TimeSpan(1,12,0,0);
            _target.WorkingEndTime = new TimeSpan(36,0,0);
            returnValue = _target.ValidateWorkingTimeLength();
            Assert.AreEqual(true, returnValue);

            _target.WorkingStartTime = new TimeSpan(36, 0, 1);
            returnValue = _target.ValidateWorkingTimeLength();
            Assert.AreEqual(true, returnValue);

            _target.WorkingSegment = TimeSpan.Zero;
            returnValue = _target.ValidateWorkingTimeLength();
            Assert.AreEqual(false, returnValue);


            _target.WorkingSegment = new TimeSpan(0, 15, 0);
            returnValue = _target.ValidateWorkingTimeLength();
            Assert.AreEqual(true, returnValue);

            _target.WorkingSegment = new TimeSpan(36, 0, 1);
            returnValue = _target.ValidateWorkingTimeLength();
            Assert.AreEqual(false, returnValue);

        }

        [Test]
        public void VerifyValidateWorkingTimes()
        {

            _target.WorkingStartTime = new TimeSpan(8,0,0);
            _target.WorkingEndTime = new TimeSpan(7,0,0);
            bool returnValue = _target.ValidateWorkingTimes();
            Assert.AreEqual(false, returnValue);

            _target.WorkingStartTime = new TimeSpan( 12, 0, 0);
            _target.WorkingEndTime = new TimeSpan(13, 0, 0);
            returnValue = _target.ValidateWorkingTimes();
            Assert.AreEqual(true, returnValue);

            _target.WorkingStartTime = new TimeSpan(36, 0, 1);
            returnValue = _target.ValidateWorkingTimeLength();
            Assert.AreEqual(true, returnValue);



        }
        #endregion
    }
}
