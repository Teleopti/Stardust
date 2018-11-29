using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class NewDynamicDayOffSchedulePeriodTargetCalculatorTest
	{
		private ISchedulePeriodTargetCalculator _target;
		private MockRepository _mocks;
		private IScheduleMatrixPro _matrix;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleDayPro[] _days;
		private IScheduleDay _scheduleDay;
		private IVirtualSchedulePeriod _schedulePeriod;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_target = new NewDynamicDayOffSchedulePeriodTargetCalculator(_matrix);
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_days = new [] { _scheduleDayPro };
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
		}

		[Test]
		public void ShouldCalculatePeriodTime()
		{
			using(_mocks.Record())
			{
				Expect.Call(_matrix.EffectivePeriodDays).Return(_days);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));
				Expect.Call(_schedulePeriod.BalanceIn).Return(TimeSpan.FromHours(1));
				Expect.Call(_schedulePeriod.Extra).Return(TimeSpan.FromHours(2));
				Expect.Call(_schedulePeriod.BalanceOut).Return(TimeSpan.FromHours(3));
			    Expect.Call(_schedulePeriod.Seasonality).Return(new Percent(0.5));
			}

			using(_mocks.Playback())
			{
				TimeSpan result = _target.PeriodTarget(true);
				Assert.AreEqual(TimeSpan.FromHours(16), result);
			}
		}
	}
}