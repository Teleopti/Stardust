using System;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.ShiftCreator
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by:SanjayaI
    /// Created date: 7/21/2008
    /// </remarks>
    public class DaysOfWeekView
    {
        #region Variables
        private WorkShiftRuleSet _ruleSet;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DaysOfWeekView"/> class.
        /// </summary>
        /// <param name="ruleSet">The rule set.</param>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        public DaysOfWeekView(WorkShiftRuleSet ruleSet)
        {
            _ruleSet = ruleSet;
        }

        #region Properties

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        public Guid? Id
        {
            get
            {
                return _ruleSet.Id;
            }
        }
        /// <summary>
        /// Gets the rule set.
        /// </summary>
        /// <value>The rule set.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        public Description RuleSet
        {
            get
            {
                InParameter.NotNull("Description", _ruleSet.Description);
                return _ruleSet.Description;
            }
        }

        /// <summary>
        /// Gets or sets the accessibility.
        /// </summary>
        /// <value>The accessibility.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        public DefaultAccessibility Accessibility
        {
            get { return _ruleSet.DefaultAccessibility; }
            set { _ruleSet.DefaultAccessibility = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DaysOfWeekView"/> is sunday.
        /// </summary>
        /// <value><c>true</c> if sunday; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        public bool Sunday
        {
            get { return _ruleSet.AccessibilityDaysOfWeek.Contains(DayOfWeek.Sunday); }
            set
            {
                if (value) _ruleSet.AddAccessibilityDayOfWeek(DayOfWeek.Sunday);
                else _ruleSet.RemoveAccessibilityDayOfWeek(DayOfWeek.Sunday);
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DaysOfWeekView"/> is monday.
        /// </summary>
        /// <value><c>true</c> if monday; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        public bool Monday
        {
            get { return _ruleSet.AccessibilityDaysOfWeek.Contains(DayOfWeek.Monday); }
            set
            {
                if (value) _ruleSet.AddAccessibilityDayOfWeek(DayOfWeek.Monday);
                else _ruleSet.RemoveAccessibilityDayOfWeek(DayOfWeek.Monday);
            }

        }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DaysOfWeekView"/> is tuesday.
        /// </summary>
        /// <value><c>true</c> if tuesday; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        public bool Tuesday
        {
            get { return _ruleSet.AccessibilityDaysOfWeek.Contains(DayOfWeek.Tuesday); }
            set
            {
                if (value) _ruleSet.AddAccessibilityDayOfWeek(DayOfWeek.Tuesday);
                else _ruleSet.RemoveAccessibilityDayOfWeek(DayOfWeek.Tuesday);
            }

        }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DaysOfWeekView"/> is wednesday.
        /// </summary>
        /// <value><c>true</c> if wednesday; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        public bool Wednesday
        {
            get { return _ruleSet.AccessibilityDaysOfWeek.Contains(DayOfWeek.Wednesday); }
            set
            {
                if (value) _ruleSet.AddAccessibilityDayOfWeek(DayOfWeek.Wednesday);
                else _ruleSet.RemoveAccessibilityDayOfWeek(DayOfWeek.Wednesday);
            }

        }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DaysOfWeekView"/> is thursday.
        /// </summary>
        /// <value><c>true</c> if thursday; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        public bool Thursday
        {
            get { return _ruleSet.AccessibilityDaysOfWeek.Contains(DayOfWeek.Thursday); }
            set
            {
                if (value) _ruleSet.AddAccessibilityDayOfWeek(DayOfWeek.Thursday);
                else _ruleSet.RemoveAccessibilityDayOfWeek(DayOfWeek.Thursday);
            }

        }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DaysOfWeekView"/> is friday.
        /// </summary>
        /// <value><c>true</c> if friday; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        public bool Friday
        {
            get { return _ruleSet.AccessibilityDaysOfWeek.Contains(DayOfWeek.Friday); }
            set
            {
                if (value) _ruleSet.AddAccessibilityDayOfWeek(DayOfWeek.Friday);
                else _ruleSet.RemoveAccessibilityDayOfWeek(DayOfWeek.Friday);
            }

        }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DaysOfWeekView"/> is saturday.
        /// </summary>
        /// <value><c>true</c> if saturday; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 7/21/2008
        /// </remarks>
        public bool Saturday
        {
            get { return _ruleSet.AccessibilityDaysOfWeek.Contains(DayOfWeek.Saturday); }
            set
            {
                if (value) _ruleSet.AddAccessibilityDayOfWeek(DayOfWeek.Saturday);
                else _ruleSet.RemoveAccessibilityDayOfWeek(DayOfWeek.Saturday);
            }
        }
        #endregion
    }
}
