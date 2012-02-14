using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Models
{
    public class AccessibilityDateViewModel : BaseModel, IAccessibilityDateViewModel
    {
        private DateTime _dateTime;

        public AccessibilityDateViewModel(IWorkShiftRuleSet ruleSet, DateTime date) : base(ruleSet)
        {
            Date = date;
        }

        public DefaultAccessibility Accessibility
        {
            get
            {
                if (WorkShiftRuleSet.DefaultAccessibility == DefaultAccessibility.Excluded)
                {
                    return DefaultAccessibility.Included;
                }
                    return DefaultAccessibility.Excluded;
            }
        }
        public string AccessibilityText
        {
            get
            {
                if (Accessibility == DefaultAccessibility.Included)
                    return UserTexts.Resources.Yes;
                return UserTexts.Resources.No;
            }
        }

        public DateTime Date
        {
            get { return _dateTime;}
            set
            {
                DateTime dateTime = value.Date;

                List<DateTime> existingDates = (from p in WorkShiftRuleSet.AccessibilityDates where p.Equals(dateTime) select p).ToList();
                existingDates.Sort();
                if (existingDates.Count == 0)
                {
                    WorkShiftRuleSet.RemoveAccessibilityDate(_dateTime);
                    WorkShiftRuleSet.AddAccessibilityDate(DateTime.SpecifyKind(value, DateTimeKind.Utc));
                }
                _dateTime = value;

            }
        }

    }
}
