
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFairnessValueResult
    {
        /// <summary>
        /// Gets or sets the fairness points.
        /// </summary>
        /// <value>The fairness points.</value>
        double FairnessPoints { get; set; }
        /// <summary>
        /// Gets or sets the total number of shifts.
        /// </summary>
        /// <value>The total number of shifts.</value>
        double TotalNumberOfShifts { get; set; }

        /// <summary>
        /// Gets the fairness ponts per shift.
        /// </summary>
        /// <value>The fairness points per shift.</value>
        double FairnessPointsPerShift { get; }
    }
}
