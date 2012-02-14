using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.ShiftCreator
{
    /// <summary>
    /// Unit testing for class ActivityNormalGridViewAdapter
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 6/15/2008
    /// </remarks>
    [TestFixture]
    public class AbsolutePositionViewTest
    {
        #region Variables

        private AbsolutePositionView _target, _target1, _target2;
        private ActivityNormalExtender _base;
        private Activity _activity;
        private TimePeriodWithSegment _activityLengthWithSegment;
        private TimePeriodWithSegment _activityPositionWithSegment;
        private WorkShiftRuleSet _workShiftRuleSet;
        private IList<IActivity> _activities;

        #endregion

        #region SetUp and TearDown

        /// <summary>
        /// Initilaizes the test.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [SetUp]
        public void TestInit()
        {
            _activity = new Activity("Test");

            _activities = new List<IActivity>();
            IActivity activityOne = ActivityFactory.CreateActivity("Test");
            _activities.Add(activityOne);
            IActivity activityTwo = ActivityFactory.CreateActivity("A2");
            _activities.Add(activityTwo);
            IActivity activityThree = ActivityFactory.CreateActivity("A3");
            _activities.Add(activityThree);


            _activityLengthWithSegment = new TimePeriodWithSegment(2, 0, 5, 0, 180);
            _activityPositionWithSegment = new TimePeriodWithSegment(3, 0, 4, 0, 60);
            _workShiftRuleSet = WorkShiftRuleSetFactory.Create();

            _base =
                new ActivityRelativeEndExtender(_activity, _activityLengthWithSegment, _activityPositionWithSegment);
            ((IWorkShiftExtender)_base).SetParent(_workShiftRuleSet);

            _target = new AbsolutePositionView(_base, _activities);

            _target1 = new AbsolutePositionView();
            _target1.ContainedEntity = new ActivityAbsoluteStartExtender(_activity, _activityLengthWithSegment, _activityPositionWithSegment);

            _target2 = new AbsolutePositionView();
            _target2.ContainedEntity = new ActivityRelativeStartExtender(_activity, _activityLengthWithSegment, _activityPositionWithSegment);
        }

        /// <summary>
        /// Disposes the test.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [TearDown]
        public void TestDispose()
        {
            _base = null;
            _target = null;
        }

        #endregion

        #region Tests

        /// <summary>
        /// Verifies the type of class.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyTypeOfClass()
        {
            Type actualValue = typeof(ActivityRelativeEndExtender);
            Type expectedValue = _target.TypeOfClass;

            Assert.AreEqual(expectedValue, actualValue);

            // To test other execution paths of TypeOfClass

            actualValue = typeof(ActivityAbsoluteStartExtender);
            expectedValue = _target1.TypeOfClass;

            Assert.AreEqual(expectedValue, actualValue);

            actualValue = typeof(ActivityRelativeStartExtender);
            expectedValue = _target2.TypeOfClass;

            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the current extender.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyCurrentExtender()
        {
            Assert.AreEqual(_target.CurrentExtender, _base);
        }

        /// <summary>
        /// Verifies the description.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyDescription()
        {
            string actualValue = "TestDescription";
            _target.Description = actualValue;
            string expectedValue = _target.Description;
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the grouping activity.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyGroupingActivity()
        {
            _target.GroupingActivity = "Test";
            Assert.AreEqual("Test", _target.GroupingActivity);
        }

        /// <summary>
        /// Verifies the length of the activity.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyActivityLength()
        {
            TimePeriodWithSegment actualValue = _activityLengthWithSegment;
            _target.ActivityLength = actualValue;

            TimePeriodWithSegment expectedValue = _target.ActivityLength;
            Assert.AreEqual(expectedValue, actualValue);

        }

        /// <summary>
        /// Verifies the activity position.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyActivityPosition()
        {
            TimePeriodWithSegment actualValue = _activityPositionWithSegment;
            _target.ActivityPosition = actualValue;

            TimePeriodWithSegment expectedValue = _target.ActivityPosition;
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the start segment.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyStartSegment()
        {
            TimeSpan actualValue = _activityLengthWithSegment.Period.StartTime;
            _target.StartSegment = actualValue;

            TimeSpan expectedValue = _target.StartSegment;
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the start segment for activity position.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyStartSegmentForActivityPosition()
        {
            TimeSpan actualValue = _activityPositionWithSegment.Period.StartTime;
            _target.StartSegmentForActivityPosition = actualValue;

            TimeSpan expectedValue = _target.StartSegmentForActivityPosition;          
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the segment.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifySegment()
        {
            TimeSpan actualValue = _activityLengthWithSegment.Segment;
            _target.Segment = actualValue;

            TimeSpan expectedValue = _target.Segment;
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the segment for activity position.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifySegmentForActivityPosition()
        {
            TimeSpan actualValue = _activityPositionWithSegment.Segment;
            _target.SegmentForActivityPosition = actualValue;

            TimeSpan expectedValue = _target.SegmentForActivityPosition;
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the start time.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyStartTime()
        {
            TimeSpan actualValue = _activityLengthWithSegment.Period.StartTime;
            _target.StartTime = actualValue;
            _target.StartTime = actualValue;
            TimeSpan expectedValue = _target.StartTime;
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the start time for activity position.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyStartTimeForActivityPosition()
        {
            TimeSpan actualValue = _activityPositionWithSegment.Period.StartTime;
            _target.StartTimeForActivityPosition = actualValue;

            TimeSpan expectedValue = _target.StartTimeForActivityPosition;
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the end time.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyEndTime()
        {
            TimeSpan actualValue = _activityLengthWithSegment.Period.EndTime;
            _target.EndTime = actualValue;

            TimeSpan expectedValue = _target.EndTime;
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the end time for activity position.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyEndTimeForActivityPosition()
        {
            TimeSpan actualValue = _activityPositionWithSegment.Period.EndTime;
            _target.EndTimeForActivityPosition = actualValue;

            TimeSpan expectedValue = _target.EndTimeForActivityPosition;
            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the parent.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyParent()
        {
            IWorkShiftRuleSet actualValue = _workShiftRuleSet;
            IWorkShiftRuleSet expectedValue = (IWorkShiftRuleSet)_target.Parent;

            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the current work shift rule.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyCurrentWorkShiftRule()
        {
            WorkShiftRuleSet actualValue = _workShiftRuleSet;
            WorkShiftRuleSet expectedValue = _target.CurrentWorkShiftRule;

            Assert.AreEqual(expectedValue, actualValue);
        }

        /// <summary>
        /// Verifies the work shift rule.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyWorkShiftRule()
        {
            WorkShiftRuleSet actualValue = _workShiftRuleSet;
            _target.WorkShiftRule = actualValue;

            WorkShiftRuleSet expectedValue = _target.WorkShiftRule;
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void VerifyRuleSet()
        {
            WorkShiftRuleSet actualValue = _workShiftRuleSet;
            _target.RuleSet = actualValue;

            WorkShiftRuleSet expectedValue = _target.RuleSet;
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void VerifyIsAutoPosition()
        {
            bool val = true;
            _target.IsAutoPosition = true;
            bool expected = _target.IsAutoPosition;
            Assert.AreEqual(val, expected);
        }

        [Test]
        public void VerifyCVCount()
        {
            byte? _setter = 5;
            _target.CVCount = _setter;
            object _getter = _target.CVCount;
            Assert.AreEqual(null, _getter);
        }

        [Test]
        public void VerifyTypeClass()
        {
            Type actualValue = typeof(ActivityRelativeEndExtender);
            Type expectedValue = _target.TypeClass;

            Assert.AreEqual(expectedValue, actualValue);

            actualValue = typeof(ActivityAbsoluteStartExtender);
            expectedValue = _target1.TypeClass;

            Assert.AreEqual(expectedValue, actualValue);

            actualValue = typeof(ActivityRelativeStartExtender);
            expectedValue = _target2.TypeClass;

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void VerifyCurrentActivity()
        {
            _target.CurrentActivity = "Test";
            Assert.AreEqual("Test", _target.CurrentActivity);
        }

        [Test]
        public void VerifyALSegment()
        {
            TimeSpan actualValue = _activityLengthWithSegment.Segment;
            _target.ALSegment = actualValue;

            TimeSpan expectedValue = _target.ALSegment;
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void VerifyALMinTime()
        {
            TimeSpan actualValue = _activityLengthWithSegment.Period.StartTime;
            _target.ALMinTime = actualValue;

            TimeSpan expectedValue = _target.ALMinTime;
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void VerifyALMaxTime()
        {
            TimeSpan actualValue = _activityLengthWithSegment.Period.EndTime;
            _target.ALMaxTime = actualValue;

            TimeSpan expectedValue = _target.ALMaxTime;
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void VerifyAPSegment()
        {
            TimeSpan? actualValue = _activityPositionWithSegment.Segment;
            _target.APSegment = (TimeSpan) actualValue;

            TimeSpan? expectedValue = _target.APSegment;
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void VerifyAPStartTime()
        {
            TimeSpan? actualValue = _activityPositionWithSegment.Period.StartTime;
            _target.APStartTime = actualValue;

            TimeSpan? expectedValue = _target.APStartTime;
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void VerifyAPEndTime()
        {
            TimeSpan? actualValue = _activityPositionWithSegment.Period.EndTime;
            _target.APEndTime = actualValue;

            TimeSpan? expectedValue = _target.APEndTime;
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void VerifyTypeChanged()
        {
            int expected = 1985;
            _target.TypeChanged += _target_TypeChanged;
            _target.IsAutoPosition = true;
            _target.TypeChanged -= _target_TypeChanged;
            Assert.AreEqual(expected, value);
        }

        private int value;
        private void _target_TypeChanged(object sender, CombinedViewTypeChangeEventArgs e)
        {
            value = 1985;
        }

        [Test]
        public void VerifyWorkShiftExtender()
        {
            
        }

        [Test]
        public void VerifyWorkShiftRuleSet()
        {
            WorkShiftRuleSet _setter = WorkShiftRuleSetFactory.Create();
            this._target.WorkShiftRuleSet = _setter;
            WorkShiftRuleSet _getter = this._target.WorkShiftRuleSet;
            Assert.AreEqual(_setter, _getter);
        }

        [Test]
        public void VerifyValidate()
        {
            bool returnValue = false;

            //this._target.ALMinTime = new TimeSpan(1);
            //this._target.ALMaxTime = new TimeSpan(2);
            //returnValue = this._target.Validate();
            //Assert.AreEqual(true, returnValue);

            //this._target.ALSegment = new TimeSpan(1);
            //returnValue = this._target.Validate();
            //Assert.AreEqual(true, returnValue);

            //this._target.ALSegment = new TimeSpan(1);

            //this._target.APStartTime = new TimeSpan(1);
            //this._target.APEndTime = new TimeSpan(2);
            //returnValue = this._target.Validate();
            //Assert.AreEqual(true, returnValue);

            //this._target.APSegment = new TimeSpan(1);
            //returnValue = this._target.Validate();
            //Assert.AreEqual(true, returnValue);

            //this._target.APSegment = new TimeSpan(1);

            //Assert.AreEqual(true, this._target.Validate());

            //this._target.EndTime = new TimeSpan(1, 0, 0);
            //this._target.StartTime = TimeSpan.Zero;

            //this._target.StartTimeForActivityPosition = TimeSpan.Zero;



            this._target.ALMinTime = new TimeSpan(0);
            returnValue = this._target.Validate();
            Assert.AreEqual(false, returnValue);

            this._target.ALMinTime = new TimeSpan(1);
            this._target.ALMaxTime = new TimeSpan(0);
            returnValue = this._target.Validate();
            Assert.AreEqual(false, returnValue);

            this._target.ALMinTime = new TimeSpan(5);
            this._target.ALMaxTime = new TimeSpan(4);
            returnValue = this._target.Validate();
            Assert.AreEqual(false, returnValue);

            this._target.ALMinTime = new TimeSpan(5);
            this._target.ALMaxTime = new TimeSpan(6);
            this._target.ALSegment = new TimeSpan(0);
            returnValue = this._target.Validate();
            Assert.AreEqual(false, returnValue);

            this._target.ALMinTime = new TimeSpan(5);
            this._target.ALMaxTime = new TimeSpan(6);
            this._target.ALSegment = new TimeSpan(15);
            this._target.APStartTime = new TimeSpan(6);
            this._target.APEndTime = new TimeSpan(5);
            returnValue = this._target.Validate();
            Assert.AreEqual(false, returnValue);

            this._target.ALMinTime = new TimeSpan(5);
            this._target.ALMaxTime = new TimeSpan(6);
            this._target.ALSegment = new TimeSpan(15);
            this._target.APStartTime = new TimeSpan(6);
            this._target.APStartTime = new TimeSpan(7);
            this._target.APSegment = new TimeSpan(0);
            returnValue = this._target.Validate();
            Assert.AreEqual(false, returnValue);

            this._target.ALMinTime = new TimeSpan(5);
            this._target.ALMaxTime = new TimeSpan(6);
            this._target.ALSegment = new TimeSpan(15);
            this._target.APStartTime = new TimeSpan(6);
            this._target.APStartTime = new TimeSpan(7);
            this._target.APSegment = new TimeSpan(0);
            returnValue = this._target.Validate();
            Assert.AreEqual(false, returnValue);

            
        }

        #endregion
    }
}
