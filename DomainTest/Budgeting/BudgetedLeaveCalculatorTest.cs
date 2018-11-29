using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Budgeting
{
	[TestFixture]
	public class BudgetedLeaveCalculatorTest
	{
		[Test]
		public void ShouldReturnOnClosedDay()
		{
			var target = new BudgetedLeaveCalculator(new NetStaffCalculator(new GrossStaffCalculator()));
			var budgetDay = new BudgetDay(new BudgetGroup(), new Scenario("Test"), new DateOnly(2013, 04, 16)) {IsClosed = true};
			var res = new BudgetCalculationResult();
			target.Calculate(budgetDay, new List<IBudgetDay> {budgetDay}, ref res);
		}
	}
}
