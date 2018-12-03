using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class MatrixUnselectedDaysLockerTest
	{
		private MockRepository _mocks;
		private MatrixUnselectedDaysLocker _target;
		private IScheduleMatrixPro _matrix;
		private DateOnlyPeriod _dop;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private IScheduleDayPro _scheduleDayPro3;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_dop = new DateOnlyPeriod(2013, 6, 14, 2013, 6, 14);
			_target = new MatrixUnselectedDaysLocker(new List<IScheduleMatrixPro>{_matrix}, _dop);
			_scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDayPro3 = _mocks.StrictMock<IScheduleDayPro>();
		}

		[Test]
		public void ShouldLockDaysOutsideSelection()
		{
			var list = new [] {_scheduleDayPro1, _scheduleDayPro2,_scheduleDayPro3};
			using (_mocks.Record())
			{
				Expect.Call(_matrix.EffectivePeriodDays).Return(list);

				Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2013, 6, 13));
				Expect.Call(() => _matrix.LockDay(new DateOnly(2013, 6, 13)));

				Expect.Call(_scheduleDayPro2.Day).Return(new DateOnly(2013, 6, 14));

				Expect.Call(_scheduleDayPro3.Day).Return(new DateOnly(2013, 6, 15));
				Expect.Call(() => _matrix.LockDay(new DateOnly(2013, 6, 15)));
			}

			using (_mocks.Playback())
			{
				_target.Execute();
			}
		}
      
      
      

	}
}