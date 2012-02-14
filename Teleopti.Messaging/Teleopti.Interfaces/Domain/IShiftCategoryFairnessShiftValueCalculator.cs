namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Modifies the shift value according to fairness factor.
    /// </summary>
    public interface IShiftCategoryFairnessShiftValueCalculator
    {
        /// <summary>
        /// Modifies the shift value according to fairness factor.
        /// </summary>
        /// <param name="shiftValue">The shift value.</param>
        /// <param name="factorForShiftEvaluation">The factor for shift evaluation.</param>
        /// <param name="maxShiftValue">The max shift value.</param>
        /// <returns></returns>
        double ModifiedShiftValue(double shiftValue, double factorForShiftEvaluation, double maxShiftValue);
    }
}