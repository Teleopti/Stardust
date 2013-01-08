﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class PopulationStatisticsCalculator : IPopulationStatisticsCalculator
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

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationStatisticsCalculator"/> class.
        /// </summary>
        public PopulationStatisticsCalculator()
        {
            _values = new List<IPopulationStatisticsData>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationStatisticsCalculator"/> class.
        /// </summary>
        /// <param name="ignoreNonNumberValues">if set to <c>true</c> ignores non number values.</param>
        public PopulationStatisticsCalculator(bool ignoreNonNumberValues) : this()
        {
            _ignoreNonNumberValues = ignoreNonNumberValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationStatisticsCalculator"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="ignoreNonNumberValues">if set to <c>true</c> ignores non number values.</param>
        public PopulationStatisticsCalculator(IEnumerable<double> values, bool ignoreNonNumberValues) : this(ignoreNonNumberValues)
        {
            if (values != null)
                foreach (var value in values)
                    AddItem(value);
            Analyze();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationStatisticsCalculator"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public PopulationStatisticsCalculator(IEnumerable<double> values)
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
            CalculateSumma();
            CalculateAverage();
            foreach (PopulationStatisticsData statisticData in _values)
            {
                statisticData.Analyze(_average);
            }
            CalculateDeviationSquareFromAverageSumma();
            CalculateStandardDeviation();
            CalculateSquareSumma();
            CalculateRootMeanSquare();
			CalculateTeleopti();
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

		private void CalculateTeleopti()
		{
            _teleopti = _values.Cast<PopulationStatisticsData>().Sum(statisticData => Math.Abs(statisticData.Value)) + _standardDeviation;
		}

    	/// <summary>
        /// Calculates the mean.
        /// </summary>
        protected void CalculateAverage()
        {
            _average = Summa/Count;
        }

        /// <summary>
        /// Calculates the summa.
        /// </summary>
        protected void CalculateSumma()
        {
            _summa = _values.Cast<PopulationStatisticsData>().Sum(statisticData => statisticData.Value);
        }

        /// <summary>
        /// Calculates the deviation square from average summa.
        /// </summary>
        protected void CalculateDeviationSquareFromAverageSumma()
        {
            _deviationSquareFromAverageSumma = _values.Cast<PopulationStatisticsData>().Sum(statisticData => statisticData.DeviationSquareFromAverage);
        }

        /// <summary>
        /// Calculates the value square summa.
        /// </summary>
        protected void CalculateSquareSumma()
        {
            _squareSumma = _values.Cast<PopulationStatisticsData>().Sum(statisticData => Math.Pow(statisticData.Value, 2d));
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