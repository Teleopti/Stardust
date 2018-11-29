using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class SchedulePeriodTargetDayOffCalculatorTest
	{
		[Test]
		public void ShouldGetDaysOff()
		{
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			virtualSchedulePeriod.Stub(x => x.DaysOff()).Return(2);

			var target = new SchedulePeriodTargetDayOffCalculator();

			var result = target.TargetDaysOff(virtualSchedulePeriod);

			result.Should().Be(new MinMax<int>(2, 2));
		}

		[Test]
		public void ShouldCalculateDaysOffWithToleranceFromContract()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.NegativeDayOffTolerance = 2;
			contract.PositiveDayOffTolerance = 1;
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			virtualSchedulePeriod.Stub(x => x.DaysOff()).Return(8);
			virtualSchedulePeriod.Stub(x => x.Contract).Return(contract);

			var target = new SchedulePeriodTargetDayOffCalculator();

			var result = target.TargetDaysOff(virtualSchedulePeriod);

			result.Should().Be(new MinMax<int>(6, 9));
		}
	}
}