using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Holds data for a splitted interval (period)
    /// </summary>
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-10-27    
    /// </remarks>
    public class PeriodDistribution : IPeriodDistribution
    {
        private readonly ISkillStaffPeriod _skillStaffPeriod;
        private readonly IActivity _activity;
        private readonly DateTimePeriod _period;
        private readonly int _lengthToSplitOn;
        double[] _splittedValues;
        private int[] _splittedPeriodLength;
        private double _demandedTraff;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodDistribution"/> class.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="period">The period start.</param>
        /// <param name="lengthToSplitOn">The length to split on.</param>
        /// <param name="demandedTraff">The demanded traff.</param>
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-10-27
        /// </remarks>
        public PeriodDistribution(ISkillStaffPeriod skillStaffPeriod, IActivity activity, DateTimePeriod period, int lengthToSplitOn, double demandedTraff)
        {
            InParameter.NotNull("period", period);
            InParameter.ValueMustBeLargerThanZero("lengthToSplitOn", lengthToSplitOn);

            _skillStaffPeriod = skillStaffPeriod;
            _activity = activity;
            _period = period;
            _lengthToSplitOn = lengthToSplitOn;
            _demandedTraff = demandedTraff;

            int overflow;
            int cnt = Math.DivRem((int)period.ElapsedTime().TotalMinutes, lengthToSplitOn, out overflow);
            if (overflow > 0)
                cnt++;

            _splittedValues = new double[cnt];
            _splittedPeriodLength = new int[cnt];
            for (int i = 0; i < _splittedPeriodLength.Length; i++)
            {
                _splittedPeriodLength[i] = lengthToSplitOn;
            }
            if (overflow > 0)
            {
                _splittedPeriodLength[_splittedPeriodLength.Length - 1] = overflow;
            }

        }

        /// <summary>
        /// Processes the layers and splittes them into smaller pieces.
        /// </summary>
        /// <param name="layerCollectionFilteredByPeriod">The layer collection.</param>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-27    
        ///  Changed:
        ///  RK, 090227
        ///  Filter layercollection by activity once and
        ///  leave early if possible
        /// </remarks>
        public void ProcessLayers(IVisualLayerCollection layerCollectionFilteredByPeriod)
        {
            _splittedValues = FillInValuesFromLayers(layerCollectionFilteredByPeriod, _splittedValues);
            IPopulationStatisticsCalculator calculator =
                new PopulationStatisticsCalculator(CalculateSplitPeriodRelativeValues());
            _skillStaffPeriod.SetDistributionValues(calculator, this);
        }

        public double[] CalculateSplitPeriodRelativeValues()
        {
            DeviationStatisticData stat;
            var ret = new double[_splittedValues.Length];
            for (int i = 0; i < _splittedValues.Length; i++)
            {
                double traff = _splittedValues[i]/_splittedPeriodLength[i];
                stat= new DeviationStatisticData(_demandedTraff, traff);
                ret[i] = stat.RelativeDeviation;
            }
            return ret;
        }

        public double DeviationAfterNewLayers(IVisualLayerCollection layerCollection)
        {
            double[] tmp = FillInValuesFromLayers(layerCollection.FilterLayers(_period), _splittedValues);
            return new PopulationStatisticsCalculator(tmp).StandardDeviation;
        }

        private double[] FillInValuesFromLayers(IVisualLayerCollection layerCollectionFilteredByPeriod, double[] splittedValues)
        {
            if (!layerCollectionFilteredByPeriod.HasLayers)
                return splittedValues;

            IVisualLayerCollection layerCollectionFilteredByPeriodAndActivity = layerCollectionFilteredByPeriod.FilterLayers(_activity);
            if (!layerCollectionFilteredByPeriodAndActivity.HasLayers) return splittedValues;

            //layerCollectionFilteredByPeriodAndActivity = layerCollectionFilteredByPeriodAndActivity.FilterLayers(_period);

            for (int i = 0; i < splittedValues.Length; i++)
            {
                var time = _period.StartDateTime.AddMinutes(i*_lengthToSplitOn);
                DateTimePeriod period = new DateTimePeriod(time,time.AddMinutes(_lengthToSplitOn));
                foreach (IVisualLayer layer in layerCollectionFilteredByPeriodAndActivity)
                {
                    DateTimePeriod? intersection = period.Intersection(layer.Period);
                    if (intersection.HasValue)
                    {
                        splittedValues[i] += intersection.Value.ElapsedTime().TotalMinutes;
                    }
                }
            }
            return splittedValues;
        }

        /// <summary>
        /// Gets the deviation.
        /// </summary>
        /// <value>The deviation.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-27    
        /// </remarks>
        public double CalculateStandardDeviation()
        {
            return new PopulationStatisticsCalculator(CalculateSplitPeriodRelativeValues()).StandardDeviation;
        }

        /// <summary>
        /// Gets the period detail average.
        /// </summary>
        /// <value>The period detail average.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-27    
        /// </remarks>
        public double PeriodDetailAverage
        {
            get { return PeriodDetailsSum / _splittedValues.Length; }
        }

        /// <summary>
        /// Gets the period details sum.
        /// </summary>
        /// <value>The period details sum.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-27    
        /// </remarks>
        public double PeriodDetailsSum
        {
            get
            {
                double ret = 0;
                foreach (double detail in _splittedValues)
                {
                    ret += detail;
                }
                return ret;
            }
        }

        /// <summary>
        /// Gets the splitted period values.
        /// </summary>
        /// <value>The splitted period values.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-27    
        /// </remarks>
        public double[] GetSplitPeriodValues()
        {
             return _splittedValues;
        }
    }
}
