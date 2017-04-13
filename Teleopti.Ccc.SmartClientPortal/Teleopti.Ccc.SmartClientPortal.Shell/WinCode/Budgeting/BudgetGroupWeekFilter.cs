using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.WinCode.Budgeting.Models;

namespace Teleopti.Ccc.WinCode.Budgeting
{
    public class BudgetGroupWeekFilter
    {
        private readonly IList<IBudgetGroupDayDetailModel> _budgetGroupDayDetailModels;
        private readonly CultureInfo _cultureInfo;

        public BudgetGroupWeekFilter(IList<IBudgetGroupDayDetailModel> budgetGroupDayDetailModels, CultureInfo cultureInfo)
        {
            _budgetGroupDayDetailModels = budgetGroupDayDetailModels;
            _cultureInfo = cultureInfo;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IList<IList<IBudgetGroupDayDetailModel>> Filter()
        {
            IList<IList<IBudgetGroupDayDetailModel>> listOfWeeks = new List<IList<IBudgetGroupDayDetailModel>>();
            IList<IBudgetGroupDayDetailModel> weekList = new List<IBudgetGroupDayDetailModel>();
            var firstDayOfWeek = _cultureInfo.DateTimeFormat.FirstDayOfWeek;
            var foundStart = false;
            const int weekLength = 7;

            foreach (var day in _budgetGroupDayDetailModels)
            {
                if ((day.BudgetDay.Day.DayOfWeek == firstDayOfWeek) && (!foundStart))
                    foundStart = true;
                if (!foundStart) continue;
                weekList.Add(day);
                if (weekList.Count < weekLength) continue;
                listOfWeeks.Add(weekList);
                weekList = new List<IBudgetGroupDayDetailModel>();
            }
            return listOfWeeks;
        }
    }
}