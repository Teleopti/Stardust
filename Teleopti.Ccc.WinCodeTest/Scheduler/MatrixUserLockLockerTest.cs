using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class MatrixUserLockLockerTest
    {
        private MatrixUserLockLocker _target;
        private MockRepository _mockRepository;
        private IScheduleMatrixPro _scheduleMatrix;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDay _scheduleDay1;
        private IList<IScheduleMatrixPro> _scheduleMatrixList;
        private IList<IScheduleDay> _scheduleDays;
        private IGridlockManager _gridlockManager;
        private IPerson _person;
        private DateOnly _dateOnly;
        private ReadOnlyCollection<IScheduleDayPro> _effectiveDays;
        private GridlockDictionary _gridlockDictionary;
        private Gridlock _gridLock;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _person = _mockRepository.StrictMock<IPerson>();
            _scheduleMatrix = _mockRepository.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro1 = _mockRepository.StrictMock<IScheduleDayPro>();
            _scheduleDay1 = _mockRepository.StrictMock<IScheduleDay>();
            _scheduleMatrixList = new List<IScheduleMatrixPro> { _scheduleMatrix };
            _scheduleDays = new List<IScheduleDay> {_scheduleDay1};
            _dateOnly = new DateOnly(2011, 1, 1);
            _effectiveDays = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>{_scheduleDayPro1});
            _gridLock = new Gridlock(_person, _dateOnly, LockType.Normal, new DateTimePeriod(2011, 1, 1, 2011, 1, 1));
            _gridlockDictionary = new GridlockDictionary();
            _gridlockDictionary.Add("key", _gridLock);
            _gridlockManager = _mockRepository.StrictMock<IGridlockManager>();
            _target = new MatrixUserLockLocker(_gridlockManager);
        }

        [Test]
        public void ShouldLock()
        {
            using(_mockRepository.Record())
            {
                Expect.Call(_scheduleMatrix.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.EffectivePeriodDays).Return(_effectiveDays);
                Expect.Call(_scheduleDayPro1.Day).Return(_dateOnly);
                Expect.Call(_gridlockManager.Gridlocks(_person, _dateOnly)).Return(_gridlockDictionary);
				Expect.Call(() => _scheduleMatrix.UnlockPeriod(new DateOnlyPeriod(_dateOnly, _dateOnly)));
                Expect.Call(() =>_scheduleMatrix.LockPeriod(new DateOnlyPeriod(_dateOnly, _dateOnly)));
            }

            using(_mockRepository.Playback())
            {
				_target.Execute(_scheduleMatrixList, new DateOnlyPeriod(_dateOnly, _dateOnly));
            }
        }

        [Test]
        public void ShouldNotLock()
        {
            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleMatrix.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.EffectivePeriodDays).Return(_effectiveDays);
                Expect.Call(_scheduleDayPro1.Day).Return(_dateOnly);
				Expect.Call(() => _scheduleMatrix.UnlockPeriod(new DateOnlyPeriod(_dateOnly, _dateOnly)));
                Expect.Call(_gridlockManager.Gridlocks(_person, _dateOnly)).Return(null);
            }

            using (_mockRepository.Playback())
            {
				_target.Execute(_scheduleMatrixList, new DateOnlyPeriod(_dateOnly, _dateOnly));
            }
        }

		[Test]
		public void MatrixShouldContainSelectedPeriod()
		{
			var scheduleDay2 = _mockRepository.StrictMock<IScheduleDay>();
			var dateOnly2 = new DateOnly(2011, 1, 2);
			_scheduleMatrixList = new List<IScheduleMatrixPro> { _scheduleMatrix };
			_scheduleDays = new List<IScheduleDay> { _scheduleDay1, scheduleDay2 };

			Expect.Call(_scheduleMatrix.Person).Return(_person).Repeat.AtLeastOnce();
			Expect.Call(_scheduleMatrix.EffectivePeriodDays).Return(_effectiveDays);
			Expect.Call(_scheduleDayPro1.Day).Return(_dateOnly);
			Expect.Call(() => _scheduleMatrix.UnlockPeriod(new DateOnlyPeriod(_dateOnly, _dateOnly)));
			Expect.Call(_gridlockManager.Gridlocks(_person, _dateOnly)).Return(null);
			_mockRepository.ReplayAll();
			_target.Execute(_scheduleMatrixList, new DateOnlyPeriod(_dateOnly, dateOnly2));
			_mockRepository.VerifyAll();
		}
    }
}
