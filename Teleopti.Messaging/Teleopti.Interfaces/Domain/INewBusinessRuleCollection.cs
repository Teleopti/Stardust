using System;
using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for aggregate root business rule collection
    /// </summary>
    public interface INewBusinessRuleCollection : ICollection<INewBusinessRule>
    {
        /// <summary>
        /// Checks the rules.
        /// </summary>
        /// <param name="rangeClones">The range clones.</param>
        /// <param name="scheduleDays">The schedule days.</param>
        /// <returns></returns>
        IEnumerable<IBusinessRuleResponse> CheckRules(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays);

        /// <summary>
        /// Removes the rule.
        /// </summary>
        /// <param name="businessRuleResponseToOverride">The business rule response to override.</param>
        void Remove(IBusinessRuleResponse businessRuleResponseToOverride);

        /// <summary>
        /// Removes the specified business rule type.
        /// </summary>
        /// <param name="businessRuleType">Type of the business rule.</param>
        void Remove(Type businessRuleType);

        /// <summary>
        /// Gets the <see cref="Teleopti.Interfaces.Domain.INewBusinessRule"/> with the specified business rule type.
        /// </summary>
        /// <value></value>
        INewBusinessRule Item(Type businessRuleType);

        ///<summary>
        /// Sets the UI culture to use for validation texts.
        ///</summary>
        ///<param name="cultureInfo">The culture info.</param>
        void SetUICulture(CultureInfo cultureInfo);

        ///<summary>
        /// The currently used UICulture.
        ///</summary>
        CultureInfo UICulture { get; }

        /// <summary>
        /// Activate rule for min work time per week
        /// </summary>
        void ActivateMinWeekWorkTimeRule();
    }
}