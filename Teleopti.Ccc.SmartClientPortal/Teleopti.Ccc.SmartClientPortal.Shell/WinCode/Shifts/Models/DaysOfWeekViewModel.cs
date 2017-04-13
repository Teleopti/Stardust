using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Models
{
    public class DaysOfWeekViewModel : BaseModel, IDaysOfWeekViewModel
    {
        public DaysOfWeekViewModel(IWorkShiftRuleSet workShiftRuleSet) : base(workShiftRuleSet)
        {}

        public DefaultAccessibility Accessibility
        {
            get
            {
                if (WorkShiftRuleSet.DefaultAccessibility == DefaultAccessibility.Excluded)
                    return DefaultAccessibility.Included;
                return DefaultAccessibility.Excluded;
            }
        }

        public bool Sunday
        {
            get { return isDayContainsInRuleSet(DayOfWeek.Sunday);}
            set { addRemoveAccessibilityDates(value, DayOfWeek.Sunday);}
        }

        public bool Monday
        {
            get { return isDayContainsInRuleSet(DayOfWeek.Monday);}
            set { addRemoveAccessibilityDates(value, DayOfWeek.Monday);}
        }

        public bool Tuesday 
        { 
            get { return isDayContainsInRuleSet(DayOfWeek.Tuesday);}
            set { addRemoveAccessibilityDates(value, DayOfWeek.Tuesday);}
        }

        public bool Wednesday
        {
            get { return isDayContainsInRuleSet(DayOfWeek.Wednesday);}
            set { addRemoveAccessibilityDates(value, DayOfWeek.Wednesday);}
        }

        public bool Thursday
        {
            get { return isDayContainsInRuleSet(DayOfWeek.Thursday);}
            set { addRemoveAccessibilityDates(value, DayOfWeek.Thursday);}
        }

        public bool Friday
        {
            get { return isDayContainsInRuleSet(DayOfWeek.Friday);}
            set { addRemoveAccessibilityDates(value, DayOfWeek.Friday);}
        }

        public bool Saturday
        {
            get { return isDayContainsInRuleSet(DayOfWeek.Saturday);}
            set { addRemoveAccessibilityDates(value, DayOfWeek.Saturday);}
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

        private bool isDayContainsInRuleSet(DayOfWeek day)
        {
            return WorkShiftRuleSet.AccessibilityDaysOfWeek.Contains(day);
        }

        private void addRemoveAccessibilityDates(bool mode, DayOfWeek day)
        {
            if (mode)
                WorkShiftRuleSet.AddAccessibilityDayOfWeek(day);
            else
                WorkShiftRuleSet.RemoveAccessibilityDayOfWeek(day);
        }
    }
}
