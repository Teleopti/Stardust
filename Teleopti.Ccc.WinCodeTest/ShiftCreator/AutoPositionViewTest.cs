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
    /// Test Class for AutoPositionGridViewAdapter
    /// </summary>
    /// <remarks>
    /// Created by: Viraj Siriwardana
    /// Created date: 2008-06-15
    /// </remarks>
    [TestFixture]
    public class AutoPositionViewTest
    {
        #region Variables

        private AutoPositionView _target;
        private IWorkShiftExtender _base;
        private Activity _defaultActivity;
        private readonly TimeSpan _defaultTimeSpan = new TimeSpan(8, 0, 9, 0, 10);
        private IList<IActivity> _activities;

        #endregion

        #region Setup and Teardown
        /// <summary>
        /// Tests the init.
        /// </summary>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-06-15
        /// </remarks>
        [SetUp]
        public void TestInit()
        {
            this._defaultActivity = ActivityFactory.CreateActivity("Test");
            
            _activities = new List<IActivity>();
            IActivity activityOne = ActivityFactory.CreateActivity("Test");
            _activities.Add(activityOne);
            IActivity activityTwo = ActivityFactory.CreateActivity("A2");
            _activities.Add(activityTwo);
            IActivity activityThree = ActivityFactory.CreateActivity("A3");
            _activities.Add(activityThree);

            TimePeriodWithSegment _timePeriodWithSegment = new TimePeriodWithSegment(8, 0, 9, 0, 10);
            TimeSpan _timeSpan = new TimeSpan(1);

            AutoPositionedActivityExtender _autoPositionedActivityExtender = new AutoPositionedActivityExtender(this._defaultActivity,
                                                                                                         _timePeriodWithSegment,
                                                                                                         _timeSpan);
            this._base = (IWorkShiftExtender) _autoPositionedActivityExtender;

            _target = new AutoPositionView(this._base, _activities);
            
        }

        /// <summary>
        /// Tests the dispose.
        /// </summary>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-06-15
        /// </remarks>
        [TearDown]
        public void TestDispose()
        {
            _target = null;
        }

        /// <summary>
        /// Verifies the GroupingActivity setter method
        /// </summary>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-06-15
        /// </remarks>
        [Test]
        public void SetGroupingActivity()
        {
            string getter = "test";
            this._target.GroupingActivity = getter;
            getter = _target.GroupingActivity;
            Assert.AreSame(getter, getter);
        }

        /// <summary>
        /// Verifies the Count setter method
        /// </summary>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-06-15
        /// </remarks>
        [Test]
        public void SetCount()
        {
            byte _setter = 5;
            this._target.Count = _setter;
            byte _getter = this._target.Count;
            Assert.AreEqual(_setter, _getter);
        }

        /// <summary>
        /// Verifies the Segment setter method
        /// </summary>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-06-15
        /// </remarks>
        [Test]
        public void SetSegment()
        {
            this._target.Segment = _defaultTimeSpan;
            Assert.AreEqual(_defaultTimeSpan, this._target.Segment);
        }

        /// <summary>
        /// Verifies the StartTime setter method
        /// </summary>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-06-15
        /// </remarks>
        [Test]
        public void SetStartTime()
        {
            TimeSpan timeSpan = new TimeSpan(1);
            this._target.StartTime = timeSpan;
            Assert.AreEqual(timeSpan, this._target.StartTime);
        }

        /// <summary>
        /// Verifies the EndTime setter method
        /// </summary>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-06-15
        /// </remarks>
        [Test]
        public void SetEndTime()
        {
            this._target.EndTime = _defaultTimeSpan;
            TimeSpan _getter = this._target.EndTime;
            Assert.AreEqual(_defaultTimeSpan, _getter);
        }

        /// <summary>
        /// Verifies the WorkShiftRule setter method
        /// </summary>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-06-15
        /// </remarks>
        [Test]
        public void SetWorkShiftRule()
        {
            WorkShiftRuleSet _setter = WorkShiftRuleSetFactory.Create();
            this._target.Parent = WorkShiftRuleSetFactory.Create();
            this._target.WorkShiftRule = _setter;
            WorkShiftRuleSet _getter = this._target.WorkShiftRule;
            Assert.AreEqual(_setter, _getter);
        }

        /// <summary>
        /// Verifies the Parent setter method
        /// </summary>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-06-15
        /// </remarks>
        [Test]
        public void SetParent()
        {
            WorkShiftRuleSet ruleSet = WorkShiftRuleSetFactory.Create();
            this._target.Parent = ruleSet;
            Assert.AreSame(ruleSet, this._target.Parent);
        }

        /// <summary>
        /// Verifies the Parent setter method
        /// </summary>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-06-15
        /// </remarks>
        [Test]
        public void SetCurrentWorkShiftRule()
        {
            WorkShiftRuleSet ruleSet = WorkShiftRuleSetFactory.Create();
            this._target.CurrentWorkShiftRule = ruleSet;
            Assert.AreSame(ruleSet, this._target.CurrentWorkShiftRule);
        }

        [Test]
        public void VerifyRuleSet()
        {
            WorkShiftRuleSet _setter = WorkShiftRuleSetFactory.Create();
            this._target.Parent = WorkShiftRuleSetFactory.Create();
            this._target.RuleSet = _setter;
            WorkShiftRuleSet _getter = this._target.RuleSet;
            Assert.AreEqual(_setter, _getter);
        }

        [Test]
        public void VerifyIsAutoPosition()
        {
            bool val = true;
            this._target.IsAutoPosition = true;
            bool expected = this._target.IsAutoPosition;
            Assert.AreEqual(val, expected);
        }

        [Test]
        public void VerifyCVCount()
        {
            byte? _setter = 5;
            this._target.CVCount = _setter;
            object _getter = this._target.CVCount;
            Assert.AreEqual(_setter, _getter);
        }

        [Test]
        public void VerifyTypeClass()
        {
            Type expected = _target.TypeClass;
            Assert.AreEqual(null, expected);

        }

        [Test]
        public void VerifyCurrentActivity()
        {
            _target.CurrentActivity = "Test";
            Assert.AreSame("Test", _target.CurrentActivity);
        }

        [Test]
        public void VerifyALSegment()
        {
            this._target.ALSegment = _defaultTimeSpan;
            TimeSpan expected = this._target.ALSegment;
            Assert.AreEqual(_defaultTimeSpan, expected);
        }

        [Test]
        public void VerifyALMinTime()
        {
            TimeSpan timeSpan = new TimeSpan(1);
            this._target.ALMinTime = timeSpan;
            TimeSpan expected = this._target.ALMinTime;
            Assert.AreEqual(timeSpan, expected);
        }

        [Test]
        public void VerifyALMaxTime()
        {
            this._target.ALMaxTime = _defaultTimeSpan;
            TimeSpan _getter = this._target.ALMaxTime;
            Assert.AreEqual(_defaultTimeSpan, _getter);
        }

        [Test]
        public void VerifyAPSegment()
        {
            this._target.APSegment = _defaultTimeSpan;
            TimeSpan? _getter = _target.APSegment;
            Assert.AreEqual(this._target.APSegment, _getter);
        }

        [Test]
        public void VerifyAPStartTime()
        {
            _target.APStartTime = _defaultTimeSpan;
            TimeSpan? _getter = _target.APStartTime;
            Assert.AreEqual(null, _getter);
        }

        [Test]
        public void VerifyAPEndTime()
        {
            _target.APEndTime = _defaultTimeSpan;
            TimeSpan? _getter = _target.APEndTime;
            Assert.AreEqual(null, _getter);
        }

        [Test]
        public void VerifyTypeChanged()
        {
            int expected = 1985;
            _target.TypeChanged += _target_TypeChanged;
            _target.IsAutoPosition = false;
            _target.TypeChanged -= _target_TypeChanged;
            Assert.AreEqual(expected, value);
        }

        private int value;
        private void _target_TypeChanged(object sender, CombinedViewTypeChangeEventArgs e)
        {
            CombinedViewType viewType = e.CombinedViewType;
            ICombinedView item = e.Item;
            if (viewType == CombinedViewType.AbsolutePosition)
            {
                item.ALMinTime = new TimeSpan(8);
            }
            else
            {
                item.ALMinTime = new TimeSpan(8);
            }
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

            this._target.StartTime = new TimeSpan(1);

            this._target.ALMaxTime = new TimeSpan(3);
            returnValue = this._target.Validate();
            Assert.AreEqual(false, returnValue);

            this._target.ALSegment = new TimeSpan(1);
            returnValue = this._target.Validate();
            Assert.AreEqual(false, returnValue);

            this._target.ALSegment = new TimeSpan(0, 15, 0);
            this._target.ALMaxTime = new TimeSpan(0, 30, 0);
            this._target.ALMinTime = new TimeSpan(0, 15, 0);
            returnValue = this._target.Validate();
            Assert.AreEqual(true, returnValue);

            this._target.Segment = new TimeSpan(1);
            returnValue = this._target.Validate();
            Assert.AreEqual(true, returnValue);

            this._target.StartTime = new TimeSpan(0);
            this._target.EndTime = new TimeSpan(1);

            this._target.ALMinTime = TimeSpan.Zero;
            returnValue = this._target.Validate();
            Assert.AreEqual(false, returnValue);
        }

        #endregion
    }
}
