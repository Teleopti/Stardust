namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Base Interface for Absence Request validators.
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2010-04-19
    /// </remarks>
    public interface IAbsenceRequestValidator
    {
        /// <summary>
        /// Gets or sets the scheduling result state holder.
        /// </summary>
        /// <value>The scheduling result state holder.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-19
        /// </remarks>
        ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
        /// <summary>
        /// Gets or sets the person account balance calculator.
        /// </summary>
        /// <value>The person account balance calculator.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-19
        /// </remarks>
        IPersonAccountBalanceCalculator PersonAccountBalanceCalculator { get; set; }

        /// <summary>
        /// Gets the invalid reason text.
        /// </summary>
        /// <value>The invalid reason.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-19
        /// </remarks>
        string InvalidReason { get; }

        /// <summary>
        /// Gets or sets the resource optimization helper.
        /// </summary>
        /// <value>The resource optimization helper.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-20
        /// </remarks>
        IResourceOptimizationHelper ResourceOptimizationHelper { get; set; }

        /// <summary>
        /// Gets the display text.
        /// </summary>
        /// <value>The display text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-04-27
        /// </remarks>
        string DisplayText { get; }

        /// <summary>
        /// Validates the specified absence request.
        /// </summary>
        /// <param name="absenceRequest">The absence request.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-19
        /// </remarks>
        bool Validate(IAbsenceRequest absenceRequest);

        /// <summary>
        /// Creates a new instance of the same validator type. To avoid threading issues.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-04-19
        /// </remarks>
        IAbsenceRequestValidator CreateInstance();

        /// <summary>
        /// Specification for validate whether an absence request exceeds the allowance or not.
        /// </summary>
        IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification { get; set; }
    }
}