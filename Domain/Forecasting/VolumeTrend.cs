using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Calculates the start and end point of the trend line
    /// and changes the values according to the trend.
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-04-15
    /// </remarks>
    public class VolumeTrend
    {
        private KeyValuePair<int, double> _start;
        private KeyValuePair<int, double> _end;
        private readonly IList<double> _trendBaseValues = new List<double>();
        private readonly IList<double> _currentTrendValues = new List<double>();
        private double _xy;
        double _x;
        double _y;
        private double _xSqr;
        private Percent _trend = new Percent(0);
        private static double _dayChangeFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTrend"/> class.
        /// </summary>
        /// <param name="sortedTrendBaseValues">The sorted trend base values.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-04-15
        /// </remarks>
        public VolumeTrend(IList<double> sortedTrendBaseValues)
        {
            InParameter.NotNull(nameof(sortedTrendBaseValues), sortedTrendBaseValues);
            InParameter.ValueMustBeLargerThanZero(nameof(sortedTrendBaseValues), sortedTrendBaseValues.Count);

            ((List<double>)_trendBaseValues).AddRange(sortedTrendBaseValues);
            ((List<double>)_currentTrendValues).AddRange(sortedTrendBaseValues);
            addPoints();
            calculateTrendPoints();
        }

        /// <summary>
        /// Gets the start.
        /// </summary>
        /// <value>The start.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-04-15
        /// </remarks>
        public KeyValuePair<int, double> Start => _start;

	    /// <summary>
        /// Gets the end.
        /// </summary>
        /// <value>The end.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-04-15
        /// </remarks>
        public KeyValuePair<int, double> End => _end;

	    /// <summary>
        /// Calculates the trend start and en points.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-04-16
        /// </remarks>
        private void calculateTrendPoints()
        {
            //Yes, I know _trendBaseValues.Count is used in two variables, but it is done like that 
            //to keep the original formula intact.

            int count = _currentTrendValues.Count;
            int x1 = 1;
            int x2 = _currentTrendValues.Count;

            double m = (count * _xy - (_x * _y)) / (count * _xSqr - _x * _x);
            double b = (_y - m * _x) / count;

            _start = new KeyValuePair<int, double>(x1, ((m * x1) + b));
            _end = new KeyValuePair<int, double>(x2, ((m * x2) + b));
            if (_start.Value != 0)
            {
                if (!double.IsNaN(_start.Value))
                    _trend = new Percent(((_end.Value - _start.Value)/_start.Value) / (count/365d));
            }
            if (count == 1)
            {
                _start = new KeyValuePair<int, double>(x1, 0);
                _end = new KeyValuePair<int, double>(x2, 0);
            }
        }
        private void calculateEndPoint(double changeFactor)
        {
            DateOnly start = DateOnly.MinValue;
            double changePercent = CalculateStartDayFactor(start, start.AddDays(_end.Key), new Percent(changeFactor));

            _end = new KeyValuePair<int, double>(_end.Key, _start.Value * changePercent);
            _trend = new Percent(changeFactor);
        }

        private void addPoints()
        {
            for (int i = 0; i < _currentTrendValues.Count; i++)
                addPoint(i + 1, _currentTrendValues[i]);
        }

        private void addPoint(double x, double y)
        {
            _xy += x * y;
            _x += x;
            _y += y;
            _xSqr += x * x;
        }

        /// <summary>
        /// Changes the trend line. The provided percent  
        /// value should be achieved after 1 year.
        /// Does not change the values in the list.
        /// </summary>
        /// <param name="percent">The percent.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-04-15
        /// </remarks>
        public void ChangeTrendLine(Percent percent)
        {
            calculateEndPoint(percent.Value);
        }

        /// <summary>
        /// The rake (lutning) of the trend
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-04-23
        /// </remarks>
        public Percent Trend => _trend;

	    /// <summary>
        /// Gets the day change factor.
        /// </summary>
        /// <value>The day change factor.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-04-24
        /// </remarks>
        public static double DayChangeFactor => _dayChangeFactor;

	    /// <summary>
        /// Calculates the start day factor for the forecast period.
        /// </summary>
        /// <param name="startTrendPeriod">The start trend period.</param>
        /// <param name="startForecastPeriod">The start forecast period.</param>
        /// <param name="trendDayFactor">The trend day factor.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-04-24
        /// </remarks>
        public static double CalculateStartDayFactor(DateOnly startTrendPeriod, DateOnly startForecastPeriod, Percent trendDayFactor)
        {
            double value = trendDayFactor.Value;
            int dateDiff = startForecastPeriod.Subtract(startTrendPeriod).Days;

            //Recalculate to day value to calculate percent on year basis
            double changePercent = value / 365;

            _dayChangeFactor = changePercent + 1;

            return changePercent * dateDiff + 1;
        }
    }
}