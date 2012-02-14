//using System;
//using NUnit.Framework;
//using Rhino.Mocks;
//using Teleopti.Ccc.Domain.ResourceCalculation;
//using Teleopti.Ccc.Domain.Scheduling.Rules;
//using Teleopti.Interfaces.Domain;

//namespace Teleopti.Ccc.DomainTest.Scheduling
//{
//    [TestFixture]
//    public class LegalStateRuleTest
//    {
//        private MockRepository _mocks;
//        private LegalStateRule _target;
//        private IScheduleDay _schedulePart;
//        private ISchedulingResultStateHolder _stateHolder;
//        private IPersonAssignmentBusinessRulesOptions _options;

//        [SetUp]
//        public void Setup()
//        {
//            _mocks = new MockRepository();
//            _schedulePart = _mocks.StrictMock<IScheduleDay>();
//            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
//            _options = new PersonAssignmentBusinessRulesOptions();
            
//        }

//        [Test]
//        public void VerifyMandatoryIsFalse()
//        {
//           _target = new LegalStateRule(_stateHolder, _options);
//           Assert.IsFalse(_target.IsMandatory);
//        }

//        [Test]
//        public void VerifyIsFalseWhenAllUsingAreFalse()
//        {
//            _options.LegalStateCheckerOptions.UseAvailability = false;
//            _options.LegalStateCheckerOptions.UsePreferences = false;
//            _options.LegalStateCheckerOptions.UseRotations = false;
//            _options.LegalStateCheckerOptions.UseSchedule = false;
//            _options.LegalStateCheckerOptions.UseStudentAvailability = false;
//            _target = new LegalStateRule(_stateHolder, _options);
//            Assert.IsTrue(_target.Validate(null, null, new DateOnly()));
//        }

//        [Test]
//        public void VerifyWhenOptionsIsSet()
//        {
//            _options.LegalStateCheckerOptions.UseSchedule = true;
//            IPerson person = _mocks.StrictMock<IPerson>();
//            IVirtualSchedulePeriod schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
//            IScheduleDictionary dic = _mocks.StrictMock<IScheduleDictionary>();
//            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
//            IWorkShiftLengthCalculatorFactory factory = _mocks.StrictMock<IWorkShiftLengthCalculatorFactory>();
//            IFixedStaffWorkShiftLengthCalculator calculator = _mocks.StrictMock<IFixedStaffWorkShiftLengthCalculator>();

//            using (_mocks.Record())
//            {
//                Expect.Call(range.Person).Return(person).Repeat.AtLeastOnce();
//                Expect.Call(person.VirtualSchedulePeriod(new DateOnly())).Return(schedulePeriod);
//                Expect.Call(schedulePeriod.IsValid).Return(true);
//                Expect.Call(_stateHolder.Schedules).Return(dic);
//                Expect.Call(dic[person]).Return(range);
//                Expect.Call(range.ScheduledDay(new DateOnly())).Return(_schedulePart);
//                Expect.Call(_stateHolder.WorkShiftLengthCalculatorFactory).Return(factory);
//                Expect.Call(factory.CreateFixedStaffWorkShiftLengthCalculator(_schedulePart, new OptimizerPreferences( new OptimizerOriginalPreferences()), _stateHolder)).Return(
//                    calculator).IgnoreArguments();
//                Expect.Call(calculator.IsInLegalState()).Return(true);
//            }

//            using (_mocks.Playback())
//            {
//                _target = new LegalStateRule(_stateHolder, _options);
//                Assert.IsTrue(_target.Validate(range, null, new DateOnly()));
//            }
//        }
//    }
//}