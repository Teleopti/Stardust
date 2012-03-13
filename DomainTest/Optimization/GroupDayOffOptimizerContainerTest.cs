using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupDayOffOptimizerContainerTest
    {
        private GroupDayOffOptimizerContainer _target;
        private MockRepository _mocks;
        private IScheduleMatrixLockableBitArrayConverter _converter;
        private IDayOffDecisionMaker _decisionMaker;
        private IOptimizationPreferences _optimizationPreferences;
        private IScheduleMatrixPro _matrix;
        private IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private IDayOffLegalStateValidator _dayOffLegalStateValidator;
        private List<IPerson> _allPersons;
        private IList<IScheduleMatrixPro> _allMatrixes;
        private IGroupDayOffOptimizerCreator _groupDayOffOptimizerCreator;
        private IGroupDayOffOptimizer _groupDayOffOptimizer;
        private ISchedulingOptionsCreator _schedulingOptionsCreator;
        private ISchedulingOptions _schedulingOptions;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _converter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _decisionMaker = _mocks.StrictMock<IDayOffDecisionMaker>();
            _optimizationPreferences = new OptimizationPreferences();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _dayOffDecisionMakerExecuter = _mocks.StrictMock<IDayOffDecisionMakerExecuter>();
            _dayOffLegalStateValidator = _mocks.StrictMock<IDayOffLegalStateValidator>();
            _allPersons = new List<IPerson>();
            _allMatrixes = new List<IScheduleMatrixPro>();
            _groupDayOffOptimizerCreator = _mocks.StrictMock<IGroupDayOffOptimizerCreator>();
            _groupDayOffOptimizer = _mocks.StrictMock<IGroupDayOffOptimizer>();
            _schedulingOptionsCreator = _mocks.StrictMock<ISchedulingOptionsCreator>();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
            _target = new GroupDayOffOptimizerContainer(_converter,
                                                new List<IDayOffDecisionMaker> { _decisionMaker, _decisionMaker, _decisionMaker },
                                                _optimizationPreferences,
                                                _matrix,
                                                _dayOffDecisionMakerExecuter,
                                                new List<IDayOffLegalStateValidator> { _dayOffLegalStateValidator },
                                                _allPersons,
                                                _allMatrixes,
                                                _groupDayOffOptimizerCreator, 
                                                _schedulingOptionsCreator
                                                );
        }

        [Test]
        public void VerifyOwner()
        {
            IPerson owner = PersonFactory.CreatePerson();

            Expect.Call(_matrix.Person).Return(owner);
            _mocks.ReplayAll();
            Assert.AreEqual(owner, _target.Owner);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyExecuteFirstGroupSuccessful()
        {
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null)
                                                       {PeriodArea = new MinMax<int>(0, 1)};
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null)
                                                      {PeriodArea = new MinMax<int>(0, 1)};
            bitArrayAfterMove.Set(1, true);

            Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences))
                .Return(_schedulingOptions);
            Expect.Call(_groupDayOffOptimizerCreator.CreateDayOffOptimizer(
                _converter, 
                _decisionMaker,
                _dayOffDecisionMakerExecuter, 
                _optimizationPreferences.DaysOff,
                new List<IDayOffLegalStateValidator> { _dayOffLegalStateValidator }, _allPersons))
                .Return(_groupDayOffOptimizer);
            Expect.Call(_groupDayOffOptimizer.Execute(_matrix, _allMatrixes, _schedulingOptions)).IgnoreArguments()
                .Return(true);
            Expect.Call(_matrix.Person).Return(new Person()).Repeat.Any();

            _mocks.ReplayAll();

            Assert.That(_target.Execute(),Is.True);
            
            _mocks.VerifyAll();
           
        }

        [Test]
        public void VerifyExecuteFirstTwoGroupUnsuccessfulThirdGroupSuccessful()
        {
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null)
                                                       {PeriodArea = new MinMax<int>(0, 1)};
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null)
                                                      {PeriodArea = new MinMax<int>(0, 1)};
            bitArrayAfterMove.Set(1, true);

            Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences))
                .Return(_schedulingOptions);
            Expect.Call(_groupDayOffOptimizerCreator.CreateDayOffOptimizer(_converter, _decisionMaker,
                                                                        _dayOffDecisionMakerExecuter, _optimizationPreferences.DaysOff,
                                                                        new List<IDayOffLegalStateValidator> { _dayOffLegalStateValidator }, _allPersons)).Return(_groupDayOffOptimizer).Repeat.Times(3);
            Expect.Call(_groupDayOffOptimizer.Execute(_matrix, _allMatrixes, _schedulingOptions)).IgnoreArguments()
                .Return(false);
            Expect.Call(_groupDayOffOptimizer.Execute(_matrix, _allMatrixes, _schedulingOptions)).IgnoreArguments()
                .Return(false);
            Expect.Call(_groupDayOffOptimizer.Execute(_matrix, _allMatrixes, _schedulingOptions)).IgnoreArguments()
                .Return(true);
            Expect.Call(_matrix.Person).Return(new Person()).Repeat.Any();

            _mocks.ReplayAll();
             Assert.That(_target.Execute(), Is.True);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyExecuteAllGroupUnsuccessful()
        {
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null)
                                                       {PeriodArea = new MinMax<int>(0, 1)};
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null)
                                                      {PeriodArea = new MinMax<int>(0, 1)};
            bitArrayAfterMove.Set(1, true);

            Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences))
                .Return(_schedulingOptions);
            Expect.Call(_matrix.Person)
                .Return(new Person()).Repeat.Any();
            Expect.Call(_groupDayOffOptimizerCreator.CreateDayOffOptimizer(_converter, _decisionMaker,
                                                                           _dayOffDecisionMakerExecuter, _optimizationPreferences.DaysOff,
                                                                           new List<IDayOffLegalStateValidator> { _dayOffLegalStateValidator }, _allPersons)).Return(_groupDayOffOptimizer).Repeat.Times(3);
            Expect.Call(_groupDayOffOptimizer.Execute(_matrix, _allMatrixes, _schedulingOptions)).IgnoreArguments()
                .Return(false).Repeat.Times(3);

            _mocks.ReplayAll();
            Assert.That(_target.Execute(), Is.False);
            _mocks.VerifyAll();
        }

    }
}
