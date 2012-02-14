using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for Business Rules
    /// </summary>
    /// /// 
    public interface INewBusinessRule
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>The error message.</value>
        string ErrorMessage { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is mandatory.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is mandatory; otherwise, <c>false</c>.
        /// </value>
        bool IsMandatory { get; }

        /// <summary>
        /// Gets or sets a value indicating whether [halt modify].
        /// </summary>
        /// <value><c>true</c> if [halt modify]; otherwise, <c>false</c>.</value>
        bool HaltModify { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the rule is runned when deleting.
        /// </summary>
        /// <value><c>true</c> if [for delete]; otherwise, <c>false</c>.</value>
        bool ForDelete { get; set; }

        /// <summary>
        /// Validates.
        /// </summary>
        /// <param name="rangeClones">The range clones.</param>
        /// <param name="scheduleDays">The schedule days.</param>
        /// <returns></returns>
        IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays);
        
    }
}