using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class AbsolutePositionViewModelTest
    {
        private AbsolutePositionViewModel _target;
        private MockRepository _mockRepository;
        private ActivityNormalExtender _extender;
        private IWorkShiftRuleSet _ruleSet;
        private IActivity _activity;
        private Description _ruleSetName;
        private Description _newRuleSetName;

        private readonly TimePeriodWithSegment _alTimePeriodWithSegment = new TimePeriodWithSegment(8, 0, 9, 0, 15);
        private readonly TimePeriodWithSegment _apTimePeriodWithSegment = new TimePeriodWithSegment(17, 0, 18, 0, 15);

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _ruleSet = _mockRepository.StrictMock<IWorkShiftRuleSet>();
            _extender = _mockRepository.StrictMock<ActivityNormalExtender>();
            _activity = _mockRepository.StrictMock<IActivity>();

            _ruleSetName = new Description("test rule set", "test");
            _newRuleSetName = new Description("new rule set name", "new");
            TimePeriodWithSegment length = new TimePeriodWithSegment(0, 15, 0, 30, 15);
            TimePeriodWithSegment position = new TimePeriodWithSegment(8, 0, 17, 0, 15);

            using(_mockRepository.Record())
            {
                Expect
                    .On(_ruleSet)
                    .Call(_ruleSet.Description)
                    .Return(_ruleSetName).Repeat.Any();

                Expect
                    .On(_extender)
                    .Call(_extender.ActivityLengthWithSegment)
                    .Return(length).Repeat.Any();
                Expect
                    .On(_extender)
                    .Call(_extender.ActivityPositionWithSegment)
                    .Return(position).Repeat.Any();
                Expect
                    .On(_extender)
                    .Call(_extender.ExtendWithActivity)
                    .Return(_activity).Repeat.Any();

                _extender.ExtendWithActivity = _activity;
                LastCall.IgnoreArguments().Repeat.Any();
                _extender.ActivityLengthWithSegment = _alTimePeriodWithSegment;
                LastCall.IgnoreArguments().Repeat.Any();
                _extender.ActivityPositionWithSegment = _apTimePeriodWithSegment;
                LastCall.IgnoreArguments().Repeat.Any();
                _ruleSet.Description = _newRuleSetName;
                LastCall.IgnoreArguments().Repeat.Any();

                LastCall.PropertyBehavior().IgnoreArguments();
            }
            _mockRepository.ReplayAll();
            _target = new AbsolutePositionViewModel(_ruleSet, _extender);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCanReadWriteProperties()
        {
            IActivity newActivity = _mockRepository.StrictMock<IActivity>();
            _target.CurrentActivity = newActivity;
            Assert.AreEqual(newActivity, _target.CurrentActivity);

            _target.ActivityExtenderType = ActivityExtenderType.ActivityRelativeEndExtender;
            Assert.AreEqual(ActivityExtenderType.ActivityRelativeEndExtender, _target.ActivityExtenderType);

            Assert.IsNull(_target.Count);
            _target.Count = 123;

            _target.ALSegment = TimeSpan.FromHours(30);
            Assert.AreEqual(TimeSpan.FromHours(30), _target.ALSegment);
            _target.ALMaxTime = TimeSpan.FromHours(8);
            Assert.AreEqual(TimeSpan.FromHours(8), _target.ALMaxTime);
            _target.ALMinTime = TimeSpan.FromHours(8);
            Assert.AreEqual(TimeSpan.FromHours(8), _target.ALMinTime);

            _target.APSegment = TimeSpan.FromHours(45);
            Assert.AreEqual(TimeSpan.FromHours(45), _target.APSegment);
            _target.APStartTime = TimeSpan.FromHours(8);
            Assert.AreEqual(TimeSpan.FromHours(8), _target.APStartTime);
            _target.APEndTime = TimeSpan.FromHours(8);
            Assert.AreEqual(TimeSpan.FromHours(8), _target.APEndTime);

            _target.ActivityTypeChanged += ActivityTypeChanged;
            _target.IsAutoPosition = false;
            _target.ActivityTypeChanged -= ActivityTypeChanged;

            //Expect.On(_ruleSet).Call(_ruleSet.Description).Return(_ruleSetName).Repeat.Any();

            //Assert.AreEqual(_ruleSet, _target.WorkShiftRuleSet);
            //Assert.AreEqual(_ruleSetName, _target.WorkShiftRuleSetName);
            _target.WorkShiftRuleSetName = new Description("new rule set name", "new");

            _target.IsAutoPosition = false;
            Assert.AreEqual(false, _target.IsAutoPosition);
        }

        [Test]
        public void VerifyValidate()
        {
            bool returnValue = false;

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

            this._target.ALMinTime = TimeSpan.FromHours(8);
            this._target.ALMaxTime = TimeSpan.FromHours(9);
            this._target.ALSegment = TimeSpan.FromMinutes(15);
            this._target.APStartTime = TimeSpan.FromHours(17);
            this._target.APEndTime = TimeSpan.FromHours(18);
            this._target.APSegment = TimeSpan.FromMinutes(15);
            returnValue = this._target.Validate();
            Assert.AreEqual(true, returnValue);

            this._target.ALMinTime = TimeSpan.FromHours(8);
            this._target.ALMaxTime = TimeSpan.FromHours(9);
            this._target.ALSegment = TimeSpan.FromMinutes(15);
            this._target.APStartTime = TimeSpan.FromHours(17);
            this._target.APEndTime = TimeSpan.FromHours(18);
            this._target.APSegment = TimeSpan.FromMinutes(0);
            returnValue = this._target.Validate();
            Assert.AreEqual(false, returnValue);
        }

        private void ActivityTypeChanged(object sender, ActivityTypeChangedEventArgs e)
        {
            Assert.AreEqual(ActivityType.AbsolutePosition, e.ActivityType);
            Assert.AreEqual(_target, e.Item);
        }


    }
}
