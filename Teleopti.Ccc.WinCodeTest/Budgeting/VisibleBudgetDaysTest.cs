using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
	[TestFixture]
	public class VisibleBudgetDaysTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldFilterBudgetDaysOnVisiblePeriod()
		{
			var day = new DateOnly(2010, 2, 1);
			var budgetDay1 = new BudgetDay(null, null, day);
			var budgetDay2 = new BudgetDay(null, null, day.AddDays(-3));
			var budgetDay3 = new BudgetDay(null, null, day.AddDays(5));
			var period = new DateOnlyPeriod(2010, 2, 1, 2010, 2, 5);
			var mainModel = new BudgetGroupMainModel(null){Period = period};
			
			var detailedDay1 = new BudgetGroupDayDetailModel(budgetDay1);
			var detailedDay2 = new BudgetGroupDayDetailModel(budgetDay2);
			var detailedDay3 = new BudgetGroupDayDetailModel(budgetDay3);
			var detailedDays = new List<IBudgetGroupDayDetailModel> { detailedDay1, detailedDay2, detailedDay3 };
			var target = new VisibleBudgetDays(mainModel);
			var visibleDays = target.Filter(detailedDays);
			Assert.AreEqual(1, visibleDays.Count);
		}
	}
}
