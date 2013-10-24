using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
    public class TeamBlockTargetValueCalculator : ITeamBlockTargetValueCalculator
    {
        private readonly IList<IPopulationStatisticsData> _values = new List<IPopulationStatisticsData>();
        private double _average;
        private double _summa;
        private double _deviationSquareFromAverageSumma;
        private double _standardDeviation;
        private double _squareSumma;
        private double _rootMeanSquare;
        private bool _ignoreNonNumberValues = true;
		private double _teleopti;
		private double _absSumma;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamBlockTargetValueCalculator"/> class.
        /// </summary>
        public TeamBlockTargetValueCalculator()
        {
            _values = new List<IPopulationStatisticsData>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamBlockTargetValueCalculator"/> class.
        /// </summary>
        /// <param name="ignoreNonNumberValues">if set to <c>true</c> ignores non number values.</param>
        public TeamBlockTargetValueCalculator(bool ignoreNonNumberValues) : this()
        {
            _ignoreNonNumberValues = ignoreNonNumberValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamBlockTargetValueCalculator"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="ignoreNonNumberValues">if set to <c>true</c> ignores non number values.</param>
        public TeamBlockTargetValueCalculator(IEnumerable<double> values, bool ignoreNonNumberValues) : this(ignoreNonNumberValues)
        {
            if (values != null)
                foreach (var value in values)
                    AddItem(value);
            Analyze();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamBlockTargetValueCalculator"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public TeamBlockTargetValueCalculator(IEnumerable<double> values)
            : this(values, true){}

        public double Average
        {
            get { return _average; }
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public double Summa
        {
            get { return _summa; }
            set { _summa = value; }
        }

        public double StandardDeviation
        {
            get { return _standardDeviation; }
        }

        public double RootMeanSquare
        {
            get { return _rootMeanSquare; }
        }

        public void AddItem(double value)
        {
            if(!IgnoreNonNumberValues 
               || (IgnoreNonNumberValues && CheckNonNumberValueCriteria(value)))
                    _values.Add(new PopulationStatisticsData(value));
        }

        public double AbsoluteSum
        {
            get { return _absSumma; }
        }

        /// <summary>
        /// Checks if the parameter is non number value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>true if the criteria is met, false otherwise</returns>
        private static bool CheckNonNumberValueCriteria(double value)
        {
            return !(double.IsNaN(value) || double.IsInfinity(value));
        }

        public void Analyze()
        {
            CalculateSums();
            CalculateAverage();
            foreach (PopulationStatisticsData statisticData in _values)
            {
                statisticData.Analyze(_average);
            }
            CalculateDeviationSquareFromAverageSumma();
            CalculateStandardDeviation();
            CalculateRootMeanSquare();
			calculateTeleopti();
        }

        public bool IgnoreNonNumberValues 
        { 
            get { return _ignoreNonNumberValues; }
            set { _ignoreNonNumberValues = value; }
        }

		public double Teleopti
    	{
    		get { return _teleopti; }
    	}

	    private void calculateTeleopti()
	    {
		    _teleopti = _absSumma + _standardDeviation;
	    }

	    /// <summary>
        /// Calculates the mean.
        /// </summary>
        protected void CalculateAverage()
        {
            _average = Summa/Count;
        }

		protected void CalculateSums()
		{
			_summa = 0;
			_squareSumma = 0;
			_absSumma = 0;
			foreach (var populationStatisticsData in _values)
			{
				double value = populationStatisticsData.Value;
				_summa += value;
				_squareSumma += Math.Pow(value, 2d);
				_absSumma += Math.Abs(value);
			}
		}

        /// <summary>
        /// Calculates the deviation square from average summa.
        /// </summary>
        protected void CalculateDeviationSquareFromAverageSumma()
        {
            _deviationSquareFromAverageSumma = _values.Cast<PopulationStatisticsData>().Sum(statisticData => statisticData.DeviationSquareFromAverage);
        }

        /// <summary>
        /// Calculates the standard population deviation.
        /// </summary>
        protected void CalculateStandardDeviation()
        {
            _standardDeviation = Math.Sqrt(_deviationSquareFromAverageSumma / Count);
        }

        /// <summary>
        /// Calculates the population deviation from zero.
        /// </summary>
        protected void CalculateRootMeanSquare()
        {
            _rootMeanSquare = Math.Sqrt(_squareSumma / Count);
        }

        /// <summary>
        /// Gets the <see cref="IPopulationStatisticsData"/> with the specified key.
        /// Here for easier testing
        /// </summary>
        /// <value></value>
        public IPopulationStatisticsData this[int key]
        {
            get { return _values[key]; }
        }

        public override string ToString()
        {
            var builder = new StringBuilder(); 
            foreach (var statisticsData in _values)
            {
                builder.Append(statisticsData.Value.ToString(CultureInfo.CurrentCulture));
                builder.Append(";");
            }
            return builder.ToString();
        }

        public void Clear()
        {
            _values.Clear();
        }
    }
}