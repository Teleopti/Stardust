using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class AutoPositionViewModelTest
    {
        private AutoPositionViewModel _target;
        private MockRepository _mockRepository;
        private IWorkShiftRuleSet _ruleSet;
        private AutoPositionedActivityExtender _extender;
        private IActivity _activity;
        private Description _ruleSetName;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _ruleSet = _mockRepository.StrictMock<IWorkShiftRuleSet>();
            _extender = _mockRepository.StrictMock<AutoPositionedActivityExtender>();
            _activity = _mockRepository.StrictMock<IActivity>();

            _ruleSetName = new Description("test rule set", "test");
            TimePeriodWithSegment length = new TimePeriodWithSegment(0, 15, 0, 30, 15);
            //TimePeriodWithSegment position = new TimePeriodWithSegment(8, 0, 17, 0, 15);

            using (_mockRepository.Record())
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
                    .Call(_extender.ExtendWithActivity)
                    .Return(_activity).Repeat.Any();

                Expect
                    .On(_extender)
                    .Call(_extender.NumberOfLayers)
                    .Return(3).Repeat.Any();

                Expect
                    .On(_extender)
                    .Call(_extender.AutoPositionIntervalSegment)
                    .Return(TimeSpan.Zero).Repeat.Any();

                _extender.ExtendWithActivity = _activity;
                LastCall.IgnoreArguments().Repeat.Any();
                _extender.ActivityLengthWithSegment = length;
                LastCall.IgnoreArguments().Repeat.Any();
                _extender.NumberOfLayers = 2;
                LastCall.IgnoreArguments().Repeat.Any();
                _extender.AutoPositionIntervalSegment = TimeSpan.Zero;
                LastCall.IgnoreArguments().Repeat.Any();
            }

            _target = new AutoPositionViewModel(_ruleSet, _extender);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCanReadWriteProperties()
        {
            Assert.AreEqual(3, _target.Count);
            _target.Count = 2;

            Assert.IsNull(_target.APStartTime);
            Assert.IsNull(_target.APEndTime); 
            //Assert.IsNull(_target.APSegment);
            Assert.IsNull(_target.TypeOfClass);

            Assert.IsNotNull(_target.ContainedEntity);
            Assert.IsNotNull(((IActivityViewModel) _target).WorkShiftExtender);
        }

        [Test]
        public void VerifyValidate()
        {
            bool returnValue = false;

            this._target.ALMinTime = TimeSpan.FromMinutes(15);
            this._target.ALMaxTime = TimeSpan.FromMinutes(30);
            this._target.ALSegment = TimeSpan.FromMinutes(15);
            this._target.APSegment = TimeSpan.FromMinutes(15);
            returnValue = this._target.Validate();
            Assert.AreEqual(true, returnValue);

            this._target.ALSegment = TimeSpan.FromMinutes(0);
            returnValue = this._target.Validate();
            Assert.IsFalse(returnValue);

            this._target.APSegment = TimeSpan.FromMinutes(0);
            returnValue = this._target.Validate();
            Assert.IsFalse(returnValue);

            this._target.ALMinTime = TimeSpan.FromMinutes(0);
            this._target.ALMaxTime = TimeSpan.FromMinutes(15);
            this._target.ALSegment = TimeSpan.FromMinutes(15);
            this._target.APSegment = TimeSpan.FromMinutes(15);
            returnValue = this._target.Validate();
            Assert.IsFalse(returnValue);

            this._target.ALMinTime = TimeSpan.FromMinutes(15);
            this._target.ALMaxTime = TimeSpan.FromMinutes(0);
            returnValue = this._target.Validate();
            Assert.IsFalse(returnValue);

            this._target.ALMinTime = TimeSpan.FromMinutes(45);
            this._target.ALMaxTime = TimeSpan.FromMinutes(15);
            returnValue = this._target.Validate();
            Assert.IsFalse(returnValue);
        }
    }
}
