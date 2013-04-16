using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Budgeting
{
	[TestFixture]
	public class NetStaffFcAdjustSurplusDistributorTest
	{
		private IBudgetDay _budgetDay;

		[SetUp]
		public void Setup()
		{
			_budgetDay = new BudgetDay(new BudgetGroup(), new Scenario("Test"), new DateOnly(2013, 04, 16));
		}

		[Test]
		public void ShouldDistribute()
		{
			var result = NetStaffFcAdjustSurplusDistributor.Distribute(_budgetDay, 100, 50, 150);
			result.Should().Be.EqualTo(150);
		}

		[Test]
		public void ShouldAddSurplusToBudgetDay()
		{
			var result = NetStaffFcAdjustSurplusDistributor.Distribute(_budgetDay, 100, 200, 150);
			_budgetDay.NetStaffFcAdjustedSurplus.Should().Be.EqualTo(150);
			result.Should().Be.EqualTo(150);
		}
	}
}
