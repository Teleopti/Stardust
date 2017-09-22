using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
	[TestFixture]
	public class SchedulePeriodCloseCalculatorTest
	{
		private SchedulePeriodCloseCalculator _schedulePeriodCloseCalculator;
		private MockRepository _mockRepository;
		private IScheduleContractTimeCalculator _scheduleContractTimeCalculator;
		private IScheduleTargetTimeCalculator _scheduleTargetTimeCalculator;
		private ISchedulePeriod _previousPeriod;
		private ISchedulePeriod _period;

		[SetUp]
		public void Setup()
		{
			_mockRepository = new MockRepository();
			_scheduleContractTimeCalculator = _mockRepository.StrictMock<IScheduleContractTimeCalculator>();
			_scheduleTargetTimeCalculator = _mockRepository.StrictMock<IScheduleTargetTimeCalculator>();
			_previousPeriod = _mockRepository.StrictMock<ISchedulePeriod>();
			_period = _mockRepository.StrictMock<ISchedulePeriod>();
			_schedulePeriodCloseCalculator = new SchedulePeriodCloseCalculator(_scheduleContractTimeCalculator, _scheduleTargetTimeCalculator, _previousPeriod, _period);
		}

		[Test]
		public void ShouldCalculateBalanceIn()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(() => _previousPeriod.BalanceOut = TimeSpan.Zero);
				Expect.Call(_scheduleContractTimeCalculator.CalculateContractTime()).Return(TimeSpan.FromHours(8));
				Expect.Call(_scheduleTargetTimeCalculator.CalculateTargetTime()).Return(TimeSpan.FromHours(3));
				Expect.Call(() => _previousPeriod.BalanceOut = TimeSpan.FromHours(5));
				Expect.Call(_previousPeriod.BalanceOut).Return(TimeSpan.FromHours(5));
				Expect.Call(() => _period.BalanceIn = TimeSpan.FromHours(5));
			}

			using(_mockRepository.Playback())
			{
				_schedulePeriodCloseCalculator.CalculateBalanceOut();
			}
		}
	}
}
