using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.ShiftCreator
{
    /// <summary>
    /// Data Source for DatesViewGrid in Shift Creator
    /// </summary>
    /// <remarks>
    /// Created by: Aruna Priyankara Wickrama
    /// Created date: 7/18/2008
    /// </remarks>
    public class AccessibilityDateView : EntityContainer<IWorkShiftRuleSet>
    {
        #region Variables
        private readonly WorkShiftRuleSet _ruleSet;
        private DateTime _dateTime;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessibilityDateView"/> class.
        /// </summary>
        /// <param name="ruleSet">The rule set.</param>
        /// <param name="dateTime">The date time.</param>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 7/18/2008
        /// </remarks>
        public AccessibilityDateView(WorkShiftRuleSet ruleSet,
                                    DateTime dateTime)
        {
            this._ruleSet = ruleSet;
            this._dateTime = dateTime;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the rule set.
        /// </summary>
        /// <value>The rule set.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 7/16/2008
        /// </remarks>
        public Description RuleSetName
        {
            get
            {
                return this._ruleSet.Description;
            }
        }

        /// <summary>
        /// Gets the rule set.
        /// </summary>
        /// <value>The rule set.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 7/18/2008
        /// </remarks>
        public WorkShiftRuleSet RuleSet
        {
            get
            {
                return this._ruleSet;
            }
        }

        /// <summary>
        /// Gets the accessibility.
        /// </summary>
        /// <value>The accessibility.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 7/16/2008
        /// </remarks>
        public DefaultAccessibility Accessibility
        {
            get
            {
                return _ruleSet.DefaultAccessibility;
            }
        }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 7/16/2008
        /// </remarks>
        public DateTime Date
        {
            get
            {
                return _dateTime;
            }
            set
            {
                DateTime dateTime = value.Date;
               
                List<DateTime> existingDates = (from p in _ruleSet.AccessibilityDates where p.Equals(dateTime) select p).ToList();
                existingDates.Sort();
                if (existingDates.Count == 0)
                {
                    _ruleSet.RemoveAccessibilityDate(_dateTime);
                    _ruleSet.AddAccessibilityDate(DateTime.SpecifyKind(value, DateTimeKind.Utc));
                    _dateTime = value;
                }
               
            }
        }

        #endregion

    }

}
