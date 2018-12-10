using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Budgeting
{
	[TestFixture]
	public class FullAllowanceCalculatorTest
	{
		[Test]
		public void ShouldReturnWhenZeroFte()
		{
			var target = new FullAllowanceCalculator();
			var budgetDay = new BudgetDay(new BudgetGroup(), new Scenario("Test"), new DateOnly(2013, 04, 16))
			{
				FulltimeEquivalentHours = 0
			};
			var result = new BudgetCalculationResult();
			target.Calculate(budgetDay, new List<IBudgetDay> { budgetDay }, ref result);

			result.FullAllowance.Should().Be.EqualTo(0);
		}
	}
}