using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Budgeting
{
	[TestFixture]
	public class BudgetDayCalculationsTest
	{
		private BudgetCalculationResult target;

		[SetUp]
		public void Setup()
		{
			target = new BudgetCalculationResult(12, 13, 14, 15, 23, 45, new Percent(0.2), 1, 2, 3, 4);
		}

		[Test]
		public void Properties()
		{
			target.GrossStaff.Should().Be.EqualTo(12);
			target.NetStaff.Should().Be.EqualTo(13);
			target.NetStaffFcAdj.Should().Be.EqualTo(14);
			target.BudgetedStaff.Should().Be.EqualTo(15);
			target.ForecastedStaff.Should().Be.EqualTo(23);
			target.Difference.Should().Be.EqualTo(45);
			target.DifferencePercent.Should().Be.EqualTo(new Percent(0.2));
			target.BudgetedLeave.Should().Be.EqualTo(1);
			target.BudgetedSurplus.Should().Be.EqualTo(2);
			target.ShrinkedAllowance.Should().Be.EqualTo(3);
			target.FullAllowance.Should().Be.EqualTo(4);
		}
	}
}