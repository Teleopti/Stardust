using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class MatrixUserLockLockerTest
    {
        private MatrixUserLockLocker _target;
        private MockRepository _mockRepository;
        private IScheduleMatrixPro _scheduleMatrix;
        private IScheduleDayPro _scheduleDayPro1;
        private IList<IScheduleMatrixPro> _scheduleMatrixList;
        private IGridlockManager _gridlockManager;
        private IPerson _person;
        private DateOnly _dateOnly;
        private IScheduleDayPro[] _effectiveDays;
        private GridlockDictionary _gridlockDictionary;
        private Gridlock _gridLock;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _person = _mockRepository.StrictMock<IPerson>();
            _scheduleMatrix = _mockRepository.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro1 = _mockRepository.StrictMock<IScheduleDayPro>();
            _scheduleMatrixList = new List<IScheduleMatrixPro> { _scheduleMatrix };
            _dateOnly = new DateOnly(2011, 1, 1);
            _effectiveDays = new [] {_scheduleDayPro1};
            _gridLock = new Gridlock(_person, _dateOnly, LockType.Normal);
            _gridlockDictionary = new GridlockDictionary();
            _gridlockDictionary.Add("key", _gridLock);
            _gridlockManager = _mockRepository.StrictMock<IGridlockManager>();
            _target = new MatrixUserLockLocker(()=>_gridlockManager, CurrentAuthorization.Make());
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
                Expect.Call(() =>_scheduleMatrix.LockDay(_dateOnly));
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
			var dateOnly2 = new DateOnly(2011, 1, 2);
			_scheduleMatrixList = new List<IScheduleMatrixPro> { _scheduleMatrix };
			
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
