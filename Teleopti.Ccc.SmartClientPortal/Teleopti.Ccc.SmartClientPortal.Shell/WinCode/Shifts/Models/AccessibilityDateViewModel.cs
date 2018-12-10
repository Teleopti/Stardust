using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models
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
                var dateTime = new DateOnly(value);

                var existingDates = (from p in WorkShiftRuleSet.AccessibilityDates where p.Equals(dateTime) select p).ToList();
                existingDates.Sort();
                if (existingDates.Count == 0)
                {
                    WorkShiftRuleSet.RemoveAccessibilityDate(new DateOnly(_dateTime));
                    WorkShiftRuleSet.AddAccessibilityDate(dateTime);
                }
				_dateTime = dateTime.Date;

            }
        }

    }
}
