namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// interface for BestBlockShiftCategoryFinder
    /// </summary>
    public interface IBestBlockShiftCategoryFinder
    {
    	/// <summary>
    	/// Finds the best shift category to be used on the specifyed list of dates.
    	/// </summary>
    	/// <param name="result">The result.</param>
    	/// <param name="person">The person.</param>
    	/// <param name="totalFairness">The Total Fainess value</param>
    	/// <param name="agentFairness">The Fairness value for the Person</param>
    	/// <returns></returns>
		IBestShiftCategoryResult BestShiftCategoryForDays(IBlockFinderResult result, IPerson person, IFairnessValueResult totalFairness, IFairnessValueResult agentFairness);


		/// <summary>
		/// Gets the scheduling result state holder.
		/// </summary>
		/// <value>The scheduling result state holder.</value>
		IScheduleDictionary ScheduleDictionary { get; } 
    }
}