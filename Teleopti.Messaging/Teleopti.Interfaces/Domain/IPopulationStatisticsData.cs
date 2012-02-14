namespace Teleopti.Interfaces.Domain
{

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2009-01-21
    /// </remarks>
    public interface IPopulationStatisticsData
    {
        /// <summary>
        /// Gets the input value.
        /// </summary>
        /// <value>The value.</value>
        double Value { get; }

        /// <summary>
        /// Gets the deviation from average.
        /// </summary>
        /// <value>The deviation from average.</value>
        double DeviationFromAverage { get; }

        /// <summary>
        /// Gets the deviation square from average.
        /// </summary>
        /// <value>The deviation square from average.</value>
        double DeviationSquareFromAverage { get; }

        /// <summary>
        /// Analizes and calculates the item data.
        /// </summary>
        /// <param name="average">The mean.</param>
        void Analyze(double average);
    }
}