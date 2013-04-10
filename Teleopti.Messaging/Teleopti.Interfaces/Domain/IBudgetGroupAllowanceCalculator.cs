namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBudgetGroupAllowanceCalculator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="absenceRequest"></param>
        /// <returns></returns>
        string CheckBudgetGroup(IAbsenceRequest absenceRequest);

        /// <summary>
        /// This will return validation error for Budget Group Head Count option in WCS.
        /// </summary>
        /// <param name="absenceRequest"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "HeadCount")]
        string CheckHeadCountInBudgetGroup(IAbsenceRequest absenceRequest);
    }
}
