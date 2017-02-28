namespace Teleopti.Interfaces.Domain
{

    /// <summary>
    /// Carries out statictical calculations on the input list of values (as population).
    /// </summary>
    /// <remarks>
    /// Created date: 2009-01-21
    /// </remarks>
    public interface ITeamBlockTargetValueCalculator
    {
        /// <summary>
        /// Gets the average.
        /// </summary>
        /// <value>The average.</value>
        double Average { get; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Gets or sets the summa.
        /// </summary>
        /// <value>The summa.</value>
        double Summa { get; set; }

        /// <summary>
        /// Gets the standard deviation. Also known as STDEV.
        /// </summary>
        /// <value>The standard population deviation.</value>
        /// <remarks>
        /// That is the squareroot of the average of the summa of squares of each deviation between the value and the average. [SQRT(SUM(SQR(x-AVG))/N)]
        /// </remarks>
        double StandardDeviation { get; }

        /// <summary>
        /// Gets the population deviation from zero. Also known as Root Mean Square deviation or RMS.
        /// </summary>
        /// <value>The population deviation from zero.</value>
        /// <remarks>
        /// That is the squareroot of the average of the summa of squares of each value. [SQRT(SUM(SQR(x))/N)]
        /// </remarks>
        double RootMeanSquare { get; }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="value">The value.</param>
        void AddItem(double value);

        /// <summary>
        /// Clears the inner items.
        /// </summary>
        void Clear();

        /// <summary>
        /// Analizes the input data and calculates statistic data.
        /// </summary>
        void Analyze();

        /// <summary>
        /// Gets a value indicating whether the calculator ignores the non number values (NaN, Infinitive).
        /// </summary>
        /// <value>
        /// 	<c>true</c> if ignore non number values; otherwise, <c>false</c>.
        /// </value>
        bool IgnoreNonNumberValues { get; }

        /// <summary>
        /// Gets the teleopti.
        /// </summary>
        double Teleopti { get; }
    }
}