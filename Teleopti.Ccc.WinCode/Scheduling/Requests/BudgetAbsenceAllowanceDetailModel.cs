using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    public class BudgetAbsenceAllowanceDetailModel
    {
        public double Allowance { get; set; }
        public IDictionary<string, double> UsedAbsencesDictionary { get; set; }
        public double UsedTotalAbsences { get; set; }
        public double AbsoluteDifference { get; set; }
        public Percent RelativeDifference { get; set; }
        public DateDayModel Date { get; set; }
        public string Week
        {
            get
            {
                if (Date == null)
                    return string.Empty;
                var weekNumber = DateHelper.WeekNumber(Date.Date, CultureInfo.CurrentCulture);
                var week = DateHelper.GetWeekPeriod(Date.Date, CultureInfo.CurrentCulture);
                return string.Format(CultureInfo.CurrentCulture, "{0} {1} {2}", UserTexts.Resources.WeekAbbreviationDot, weekNumber, week.StartDate.ToShortDateString(CultureInfo.CurrentCulture));
            }
        }
    }
}