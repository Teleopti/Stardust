using System;

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
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <param name="agentFairness"> </param>
        /// <param name="allowedMinMax">allowed min max work time</param>
        /// <returns></returns>
        IBestShiftCategoryResult BestShiftCategoryForDays(IBlockFinderResult result, IPerson person, ISchedulingOptions schedulingOptions, IFairnessValueResult agentFairness, MinMax<TimeSpan>? allowedMinMax=null);


		/// <summary>
		/// Gets the scheduling result state holder.
		/// </summary>
		/// <value>The scheduling result state holder.</value>
		IScheduleDictionary ScheduleDictionary { get; }
    }
}