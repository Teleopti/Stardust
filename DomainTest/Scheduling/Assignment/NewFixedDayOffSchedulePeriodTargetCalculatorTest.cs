using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class NewFixedDayOffSchedulePeriodTargetCalculatorTest
	{
		private ISchedulePeriodTargetCalculator _target;
		private MockRepository _mocks;
		private IScheduleMatrixPro _matrix;
		private IVirtualSchedulePeriod _schedulePeriod;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_target = new NewFixedDayOffSchedulePeriodTargetCalculator(_matrix);
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
		}

		[Test]
		public void ShouldCalculatePeriodTime()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.PeriodTarget()).Return(TimeSpan.FromHours(8));
				Expect.Call(_schedulePeriod.BalanceIn).Return(TimeSpan.FromHours(1));
				Expect.Call(_schedulePeriod.Extra).Return(TimeSpan.FromHours(2));
				Expect.Call(_schedulePeriod.BalanceOut).Return(TimeSpan.FromHours(3));
			    Expect.Call(_schedulePeriod.Seasonality).Return(new Percent(0.5));
			}

			using (_mocks.Playback())
			{
				TimeSpan result = _target.PeriodTarget(true);
				//PeriodTarget+(Extra+BalanceOut-BalanceIn)
				Assert.AreEqual(TimeSpan.FromHours(16), result);
			}
		}
	}
}