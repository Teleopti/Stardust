namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Dictionary that holdt schift category as key and fairness factors as value
    /// </summary>
    public interface IShiftCategoryFairnessFactors
    {
        /// <summary>
        /// Gets the fairnesses factor.
        /// </summary>
        /// <param name="shiftCategory">The shift category.</param>
        /// <returns></returns>
        double FairnessFactor(IShiftCategory shiftCategory);

		/// <summary>
		/// Gets the fairness points per shift.
		/// </summary>
		/// <value>The fairness points per shift.</value>
    	double FairnessPointsPerShift { get; }
    }
}