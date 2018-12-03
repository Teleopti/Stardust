using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Class for varius Statistical Smoothing algo...
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-03-26
    /// </remarks>
    public class StatisticalSmoothing
    {
        private readonly IDictionary<DateTimePeriod, double> _numbers;
        private readonly List<double> _internalNumbers;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticalSmoothing"/> class.
        /// </summary>
        /// <param name="numbers">The numbers.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-26
        /// </remarks>
        public StatisticalSmoothing(IEnumerable<KeyValuePair<DateTimePeriod, double>> numbers)
        {
            //First make sure the are sorted on DateTimePeriodKey
            _numbers = numbers.OrderBy(v => v.Key.StartDateTime).ToDictionary(item => item.Key, item => item.Value);
            _internalNumbers = new List<double>();
            _internalNumbers.AddRange(_numbers.Values);
        }

        /// <summary>
        /// Calculates the gliding average.
        /// </summary>
        /// <param name="periods">The periods. 3, 5, 7 etc...</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-26
        /// </remarks>
        public IDictionary<DateTimePeriod, double> CalculateRunningAverage(int periods)
        {
            //InParameter.GraterThanZero("periods", periods); 
            if (periods < 1)
            {
                throw new ArgumentOutOfRangeException("periods", "Running average smoothing factor must be greater than zero");
            }

            IList<double> averageNumbers = new List<double>();

            //this is the "offset" for different length of glides
            //If you have 3 periods gliding avg this number will be 2, 5=3, 7=4 etc.
            //This indicates the starting and end point
            //     10,20,30,20,10,40  with 5 periods, the starting point is 3
            //     first slot = (10+20+30)/3
            //     second =     (10+2+30+20)/4
            //     third  =     (10+2+30+20+10)/5  //this is the first "real" gliding value with 5 periods

            // if gliding periods are more than intervals count, no smoothing to apply
            if (_internalNumbers.Count < periods)
            {
                periods = 1;
            }
            int periodOffset = (int)Math.Ceiling(periods / 2.0);

            for (int i = 0; i < _internalNumbers.Count; i++)
            {
                //In the begining we have a special case
                //described above
                if (i >= periodOffset - 1 && i < periods - 1)
                {
                    firstIterations(i, averageNumbers);
                }
                else if (i >= periods - 1) //Ok, these are the easy ones just sum them back and devide by the correct period
                {
                    normalIterations(periods, i, averageNumbers);
                }
                //Special case No 2 this is the end of the loop
                //Depending on the period number of unfilled slots can differ
                //It is done as the in the beginning but from the end?!?
                if (_internalNumbers.Count - 1 == i)
                {
                    endIterations(periods, periodOffset, averageNumbers);
                }
            }
            IDictionary<DateTimePeriod, double> result = putInDictionary(averageNumbers);
            double origSum = 0;
            double newSum = 0;
            foreach (double number in _internalNumbers)
            {
                origSum += number;
            }

            foreach (KeyValuePair<DateTimePeriod, double> valuePair in result)
            {
                newSum += valuePair.Value;
            }

			double factor = newSum != 0d ? origSum / newSum : 1d;
            IList<double> modifiedValues = new List<double>();

            foreach (KeyValuePair<DateTimePeriod, double> valuePair in result)
            {
                modifiedValues.Add(valuePair.Value * factor);
            }

            return putInDictionary(modifiedValues);
        }

        /// <summary>
        /// Ends the iterations.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <param name="periodOffset">The period offset.</param>
        /// <param name="averageNumbers">The average numbers.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-26
        /// </remarks>
        private void endIterations(int periods, int periodOffset, ICollection<double> averageNumbers)
        {
            //Need to loop from "periodOffset" back to the end 
            for (int k = _internalNumbers.Count - periodOffset; k < _internalNumbers.Count - 1; k++)
            {
                double sum = 0;
                int devider = 0;
                int loopstop = k - (periods - 1) + periodOffset - 1;

                for (int j = _internalNumbers.Count - 1; j > loopstop; j--)
                {
                    sum += _internalNumbers[j];
                    devider++;
                }
                averageNumbers.Add(sum / devider);
            }
        }

        /// <summary>
        /// Normals the iterations.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <param name="i">The i.</param>
        /// <param name="averageNumbers">The average numbers.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-26
        /// </remarks>
        private void normalIterations(int periods, int i, ICollection<double> averageNumbers)
        {
            //summera "periods" bakåt
            double sum = 0;
            for (int j = i; j > i - periods; j--)
            {
                sum += _internalNumbers[j];
            }
            averageNumbers.Add(sum / periods);
        }

        /// <summary>
        /// Firsts the iterations.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="averageNumbers">The average numbers.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-26
        /// </remarks>
        private void firstIterations(int i, ICollection<double> averageNumbers)
        {
            int devider = 0;
            double sum = 0;
            //loop from the begining to the correct stop and sum
            for (int j = 0; j < i + 1; j++)
            {
                devider++;
                sum += _internalNumbers[j];
            }
            averageNumbers.Add(sum / devider);
        }

        /// <summary>
        /// Puts the in dictionary.
        /// </summary>
        /// <param name="averageNumbers">The average numbers.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-26
        /// </remarks>
        private IDictionary<DateTimePeriod, double> putInDictionary(IList<double> averageNumbers)
        {
            int index = 0;
            IDictionary<DateTimePeriod, double> returnAverageNumbers = new Dictionary<DateTimePeriod, double>();

            foreach (KeyValuePair<DateTimePeriod, double> pair in _numbers)
            {
                returnAverageNumbers.Add(pair.Key, averageNumbers[index]);
                index++;
            }
            return returnAverageNumbers;
        }
    }
}
