using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class MatrixNoMainShiftLockerTest
    {
        private MatrixNoMainShiftLocker _target;
        private MockRepository _mockRepository;
        private IScheduleMatrixPro _scheduleMatrix;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IList<IScheduleMatrixPro> _scheduleMatrixList;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _scheduleMatrix = _mockRepository.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro1 = _mockRepository.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mockRepository.StrictMock<IScheduleDayPro>();
            _scheduleDay1 = _mockRepository.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mockRepository.StrictMock<IScheduleDay>();
            _scheduleMatrixList = new List<IScheduleMatrixPro>{ _scheduleMatrix };
            _target = new MatrixNoMainShiftLocker(_scheduleMatrixList);
        }

        [Test]
        public void VerifyLockDayThatHaveNoMainShift()
        {
            using(_mockRepository.Record())
            {
                Expect.Call(_scheduleMatrix.EffectivePeriodDays)
                    .Return(new [] {_scheduleDayPro1, _scheduleDayPro2});
                Expect.Call(_scheduleDayPro1.DaySchedulePart())
                    .Return(_scheduleDay1);
				Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2010, 1, 1));
				Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
                Expect.Call(_scheduleDayPro2.DaySchedulePart())
                    .Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.IsScheduled()).Return(false);
                Expect.Call(_scheduleDayPro2.Day)
                    .Return(new DateOnly());
                Expect.Call(_scheduleDay1.PersonAbsenceCollection())
                    .Return(new IPersonAbsence[0]);
                Expect.Call(_scheduleDay2.PersonAbsenceCollection())
                    .Return(new IPersonAbsence[0]);
                _scheduleMatrix.LockDay(new DateOnly());
            }
            using(_mockRepository.Playback())
            {
                _target.Execute();
            }
        }

        [Test]
        public void VerifyLockDayThatHavePartDayAbsence()
        {
            IPersonAbsence personAbsence = _mockRepository.StrictMock<IPersonAbsence>();

            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleMatrix.EffectivePeriodDays)
                    .Return(new [] { _scheduleDayPro1, _scheduleDayPro2 });
                Expect.Call(_scheduleDayPro1.DaySchedulePart())
                    .Return(_scheduleDay1);
				Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2010, 1, 1));
				Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
                Expect.Call(_scheduleDayPro2.DaySchedulePart())
                    .Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.IsScheduled()).Return(true);
            	Expect.Call(_scheduleDayPro2.Day)
            		.Return(new DateOnly());
                Expect.Call(_scheduleDay1.PersonAbsenceCollection())
                    .Return(new IPersonAbsence[0]);
                Expect.Call(_scheduleDay2.PersonAbsenceCollection())
                    .Return(new IPersonAbsence[]{ personAbsence });
                _scheduleMatrix.LockDay(new DateOnly());
            }
            using (_mockRepository.Playback())
            {
                _target.Execute();
            }
        }

		[Test]
		public void DayWithContractDayOffShouldBeLocked()
		{
			IPersonAbsence personAbsence = _mockRepository.StrictMock<IPersonAbsence>();
			using (_mockRepository.Record())
			{
				Expect.Call(_scheduleMatrix.EffectivePeriodDays)
					.Return(new [] { _scheduleDayPro1, _scheduleDayPro2 });
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2010, 1, 1));
				Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
				Expect.Call(_scheduleDay2.IsScheduled()).Return(true);
				Expect.Call(_scheduleDayPro2.Day).Return(new DateOnly(2010, 1, 2));
				Expect.Call(_scheduleDay1.PersonAbsenceCollection())
					.Return(new IPersonAbsence[0]);
				Expect.Call(_scheduleDay2.PersonAbsenceCollection())
					.Return(new [] { personAbsence });
				_scheduleMatrix.LockDay(new DateOnly(2010, 1, 2));
			}
			using (_mockRepository.Playback())
			{
				_target.Execute();
			}
		}
    }
}
