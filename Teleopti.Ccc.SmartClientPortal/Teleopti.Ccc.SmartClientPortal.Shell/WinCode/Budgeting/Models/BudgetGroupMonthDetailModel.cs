using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Budgeting.Presenters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Models
{
    public class BudgetGroupMonthDetailModel : BudgetGroupDistributedDetailModel
    {
        public BudgetGroupMonthDetailModel(IList<IBudgetGroupDayDetailModel> budgetDays, IBudgetDayProvider budgetDayProvider, IBudgetPermissionService budgetPermissionService)
            : base(budgetDays, budgetDayProvider, budgetPermissionService)
        {}

        public string Year
        {
            get
            {
            	return
            		CultureInfo.CurrentCulture.Calendar.GetYear(BudgetDays.First().BudgetDay.Day.Date).ToString(
            			CultureInfo.CurrentCulture);
            }
        }
    } 
}