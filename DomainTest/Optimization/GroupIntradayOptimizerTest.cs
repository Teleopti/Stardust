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
	public class GroupIntradayOptimizerTest
	{
		private MockRepository _mock;
		private IGroupIntradayOptimizer _target;
		private IScheduleMatrixLockableBitArrayConverter _scheduleMatrixLockableBitArrayConverter;
		private IIntradayDecisionMaker _intradayDecisionMaker;
		private IScheduleResultDataExtractor _dayIntraDayDeviationDataExtractor;
		private IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitByRestrictionDecider;
		private IScheduleMatrixPro _matrix;
		private ILockableBitArray _lockableBitArray;
		private IPerson _person;
        private IScheduleResultDailyValueCalculator _periodValueCalculator;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleMatrixLockableBitArrayConverter = _mock.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
			_intradayDecisionMaker = _mock.StrictMock<IIntradayDecisionMaker>();
			_dayIntraDayDeviationDataExtractor = _mock.StrictMock<IScheduleResultDataExtractor>();
			_optimizationOverLimitByRestrictionDecider = _mock.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
            _periodValueCalculator = _mock.StrictMock<IScheduleResultDailyValueCalculator>();
            _target = new GroupIntradayOptimizer(_scheduleMatrixLockableBitArrayConverter, _intradayDecisionMaker,
                                                 _dayIntraDayDeviationDataExtractor, _optimizationOverLimitByRestrictionDecider, _periodValueCalculator);
            _matrix = _mock.StrictMock<IScheduleMatrixPro>();
			_lockableBitArray = _mock.StrictMock<ILockableBitArray>();
			_person = PersonFactory.CreatePerson();
		}

		[Test]
		public void ExecuteShouldCreateBitArrayIfNotCreatedIfCallingExecute()
		{
			using (_mock.Record())
			{
				commomMocks();
				Expect.Call(_intradayDecisionMaker.Execute(_lockableBitArray,
														   _dayIntraDayDeviationDataExtractor, _matrix)).IgnoreArguments().Return(null);
			}

			DateOnly? result;

			using (_mock.Playback())
			{
				result = _target.Execute();
			}

			Assert.IsFalse(result.HasValue);
		}

		[Test]
		public void ExecuteShouldCreateBitArrayIfNotCreatedIfCallingLockDate()
		{
			IScheduleDayPro scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();

			using (_mock.Record())
			{
				commomMocks();
				Expect.Call(_matrix.FullWeeksPeriodDays).Return(
					new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {scheduleDayPro}));
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
			using (_mock.Record())
			{
				commomMocks();
				Expect.Call(_intradayDecisionMaker.Execute(_lockableBitArray,
														   _dayIntraDayDeviationDataExtractor, _matrix)).Return(new DateOnly(2012, 1, 1));
			}

			DateOnly? result;

			using (_mock.Playback())
			{
				result = _target.Execute();
			}

			Assert.IsTrue(result.HasValue);
			Assert.AreEqual(new DateOnly(2012,1,1), result.Value);
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
			IVirtualSchedulePeriod schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
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
			IVirtualSchedulePeriod schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
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