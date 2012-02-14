
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFairnessValueCalculator
    {

        /// <summary>
        /// Calculates the fairness value.
        /// </summary>
        /// <param name="shiftValue">The shift value.</param>
        /// <param name="shiftCategoryFairnessPoint">The shift category fairness point.</param>
        /// <param name="maxFairnessPoint">The max fairness point.</param>
        /// <param name="totalFairness">The total fairness.</param>
        /// <param name="agentFairness">The agent fairness.</param>
        /// <param name="maxShiftValue">The max shift value.</param>
        /// <returns></returns>
        double CalculateFairnessValue(double shiftValue, double shiftCategoryFairnessPoint, double maxFairnessPoint, double totalFairness, IFairnessValueResult agentFairness, double maxShiftValue);
    }
}
