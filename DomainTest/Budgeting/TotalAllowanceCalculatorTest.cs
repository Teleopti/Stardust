using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Budgeting
{
	[TestFixture]
	public class TotalAllowanceCalculatorTest
	{
		[Test]
		public void ShouldReturnWhenZeroFte()
		{
			var target = new TotalAllowanceCalculator();
			var budgetDay = new BudgetDay(new BudgetGroup(), new Scenario("Test"), new DateOnly(2013, 04, 16))
				{
					FulltimeEquivalentHours = 0
				};
			var result = new BudgetCalculationResult();
			target.Calculate(budgetDay, new List<IBudgetDay>{budgetDay}, ref result);

			result.TotalAllowance.Should().Be.EqualTo(0);
		}
	}
}
