using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupMoveTimeOptimizerTest
    {
        private MockRepository _mock;
        private IGroupMoveTimeOptimizer _target;
        private IScheduleMatrixLockableBitArrayConverter _scheduleMatrixLockableBitArrayConverter;
        private IMoveTimeDecisionMaker _moveTimeDecisionMaker;
        private IScheduleResultDataExtractor _dataExtractor;
        private IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitByRestrictionDecider;
        private IScheduleMatrixPro _matrix;
        private IPerson _person;
        private ILockableBitArray _lockableBitArray;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleMatrixLockableBitArrayConverter = _mock.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _moveTimeDecisionMaker = _mock.StrictMock<IMoveTimeDecisionMaker>();
            _dataExtractor = _mock.StrictMock<IScheduleResultDataExtractor>();
            _optimizationOverLimitByRestrictionDecider = _mock.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
            _target = new GroupMoveTimeOptimizer(_scheduleMatrixLockableBitArrayConverter, _moveTimeDecisionMaker,
                                                 _dataExtractor, _optimizationOverLimitByRestrictionDecider);
             _matrix = _mock.StrictMock<IScheduleMatrixPro>();
            _person = PersonFactory.CreatePerson();
            _lockableBitArray = _mock.StrictMock<ILockableBitArray>();
        }

        [Test]
        public void ExecuteShouldCreateBitArrayIfNotCreatedIfCallingExecute()
        {
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixLockableBitArrayConverter.Convert(false, false)).Return(_lockableBitArray);
                Expect.Call(_moveTimeDecisionMaker.Execute(_scheduleMatrixLockableBitArrayConverter, _dataExtractor)).IgnoreArguments().Return(new List<DateOnly>(2));
            }

            IList<DateOnly> result;

            using (_mock.Playback())
            {
                result = _target.Execute();
            }

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ExecuteShouldCreateBitArrayIfNotCreatedIfCallingLockDate()
        {
            var scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            
            using (_mock.Record())
            {
                commomMocks();
                Expect.Call(_matrix.FullWeeksPeriodDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDayPro }));
                Expect.Call(scheduleDayPro.Day).Return(new DateOnly(2012, 1, 1));
                Expect.Call(() => _lockableBitArray.Lock(0, true));
            }

            using (_mock.Playback())
            {
                _target.LockDate(new DateOnly(2012, 1, 1));
            }
        }

        [Test]
        public void ShouldReturnDateIfDateWasFound()
        {
            var moveDates = new List<DateOnly> {new DateOnly(2012, 1, 1), new DateOnly(2012, 1, 5)};
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixLockableBitArrayConverter.Convert(false, false)).Return(_lockableBitArray);
                Expect.Call(_moveTimeDecisionMaker.Execute(_scheduleMatrixLockableBitArrayConverter, _dataExtractor)).Return(moveDates);
            }

            IList<DateOnly> result;

            using (_mock.Playback())
            {
                result = _target.Execute();
            }

            Assert.That(result, Is.EqualTo(moveDates));
        }

        [Test]
        public void VerifyProperties()
        {
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixLockableBitArrayConverter.SourceMatrix).Return(_matrix).Repeat.Times(2);
                Expect.Call(_matrix.Person).Return(_person);
            }

            using (_mock.Playback())
            {
                Assert.AreSame(_matrix, _target.Matrix);
                Assert.AreSame(_optimizationOverLimitByRestrictionDecider, _target.OptimizationOverLimitByRestrictionDecider);
                Assert.AreSame(_person, _target.Person);
            }
        }

        [Test]
        public void IsMatrixForDateAndPersonShouldReturnFalseIfNotCorrectPerson()
        {
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixLockableBitArrayConverter.SourceMatrix).Return(_matrix);
                Expect.Call(_matrix.Person).Return(_person);
            }

            bool result;

            using (_mock.Playback())
            {
                result = _target.IsMatrixForDateAndPerson(new DateOnly(2012, 1, 1), new Person());
            }

            Assert.IsFalse(result);
        }

        [Test]
        public void IsMatrixForDateAndPersonShouldReturnFalseIfNotCorrectDate()
        {
            var schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixLockableBitArrayConverter.SourceMatrix).Return(_matrix);
                Expect.Call(_matrix.Person).Return(_person);
                Expect.Call(_matrix.SchedulePeriod).Return(schedulePeriod);
                Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());
            }

            bool result;

            using (_mock.Playback())
            {
                result = _target.IsMatrixForDateAndPerson(new DateOnly(2012, 1, 1), _person);
            }

            Assert.IsFalse(result);
        }

        [Test]
        public void IsMatrixForDateAndPersonShouldReturnTrueIfAllCorrect()
        {
            var schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixLockableBitArrayConverter.SourceMatrix).Return(_matrix);
                Expect.Call(_matrix.Person).Return(_person);
                Expect.Call(_matrix.SchedulePeriod).Return(schedulePeriod);
                Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());
            }

            bool result;

            using (_mock.Playback())
            {
                result = _target.IsMatrixForDateAndPerson(new DateOnly(), _person);
            }

            Assert.IsTrue(result);
        }

        private void commomMocks()
        {
            Expect.Call(_scheduleMatrixLockableBitArrayConverter.Convert(false, false)).Return(_lockableBitArray);
            Expect.Call(_scheduleMatrixLockableBitArrayConverter.SourceMatrix).Return(_matrix);
        }

    }
}
