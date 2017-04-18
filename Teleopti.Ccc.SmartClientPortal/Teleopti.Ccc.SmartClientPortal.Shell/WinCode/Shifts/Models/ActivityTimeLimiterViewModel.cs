using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models
{
    public class ActivityTimeLimiterViewModel : BaseModel<ActivityTimeLimiter>, 
                                                IActivityTimeLimiterViewModel
    {
        private readonly IList<KeyValuePair<OperatorLimiter, string>> pair = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTimeLimiterViewModel"/> class.
        /// </summary>
        /// <param name="ruleSet">The rule set.</param>
        /// <param name="limiter">The limiter.</param>
        public ActivityTimeLimiterViewModel(IWorkShiftRuleSet ruleSet, ActivityTimeLimiter limiter) 
            : base(ruleSet, limiter)
        {
            pair = LanguageResourceHelper.TranslateEnumToList<OperatorLimiter>();
        }

        #region IActivityTimeLimiterViewModel Members

        /// <summary>
        /// Gets the target activity.
        /// </summary>
        /// <value>The target activity.</value>
        public IActivity TargetActivity
        {
            get
            {
                return ContainedEntity.Activity;
            }
            set
            {
                ContainedEntity.Activity = value;
            }
        }

        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        /// <value>The time.</value>
        public TimeSpan Time
        {
            get
            {
                return ContainedEntity.TimeLimit;
            }
            set
            {
                ContainedEntity.TimeLimit = value;
            }
        }

        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        /// <value>The operator.</value>
        public string Operator
        {
            get
            {
                return LanguageResourceHelper.TranslateEnumValue(ContainedEntity.TimeLimitOperator);
            }
            set
            {
                ContainedEntity.TimeLimitOperator = GetLimiterFromText(value);
            }
        }

        #endregion

        /// <summary>
        /// Gets the limiter from text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private OperatorLimiter GetLimiterFromText(string text)
        {
            OperatorLimiter limiter = OperatorLimiter.Equals;
            foreach (KeyValuePair<OperatorLimiter, string> value in pair)
            {

                if (string.Compare(value.Value, text, StringComparison.CurrentCulture) == 0)
                {
                    limiter = value.Key;
                    break;
                }
            }
            return limiter;
        }
    }
}
