using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
{
    public class BudgetGroupWeekDetailModel : BudgetGroupDistributedDetailModel
    {
        public BudgetGroupWeekDetailModel(IList<IBudgetGroupDayDetailModel> budgetDays, IBudgetDayProvider budgetDayProvider, IBudgetPermissionService budgetPermissionService)
            : base(budgetDays, budgetDayProvider, budgetPermissionService)
        {}

        public string Week
        {
            get
            {
                var date = BudgetDays.First().BudgetDay.Day;
                var weekNumber = DateHelper.WeekNumber(date.Date, CultureInfo.CurrentCulture);
                var week = DateHelper.GetWeekPeriod(date, CultureInfo.CurrentCulture);
	            return string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.WeekAbbreviationDot, weekNumber,
		            week.StartDate.ToShortDateString());
            }
        }

        public string MonthYear
        {
            get
            {
                var date = BudgetDays.First().BudgetDay.Day;
                return string.Format(CultureInfo.CurrentCulture, "{0} {1}", Month,
                                     CultureInfo.CurrentCulture.Calendar.GetYear(date.Date));
            }
        }
    }
}