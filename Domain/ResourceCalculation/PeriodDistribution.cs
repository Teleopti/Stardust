using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
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
	    readonly double[] _splittedValues;
        private readonly double _demandedTraff;
	    private readonly IList<DateTimePeriod> _periods;

	    /// <summary>
	    /// Initializes a new instance of the <see cref="PeriodDistribution"/> class.
	    /// </summary>
	    /// <param name="skillStaffPeriod"></param>
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

            _periods = period.Intervals(TimeSpan.FromMinutes(lengthToSplitOn));
		    var lastItemIndex = _periods.Count - 1;
			if (lastItemIndex >= 0)
			{
				_periods[lastItemIndex] = new DateTimePeriod(_periods[lastItemIndex].StartDateTime,period.EndDateTime);
			}
            
            _splittedValues = new double[_periods.Count];
        }

		public void ProcessLayers(IResourceCalculationDataContainerWithSingleOperation resourceCalculationDataContainer)
		{
			for (int i = 0; i < _splittedValues.Length; i++)
			{
				_splittedValues[i] = 0;
			}

			var intraIntervalValues = resourceCalculationDataContainer.IntraIntervalResources(_skillStaffPeriod.SkillDay.Skill, _period);
			foreach (var resourcePeriod in intraIntervalValues)
			{
				for (int i = 0; i < _periods.Count; i++)
				{
					var intersection = _periods[i].Intersection(resourcePeriod);
					if (intersection.HasValue)
					{
						_splittedValues[i] += intersection.Value.ElapsedTime().TotalMinutes;
					}
				}
			}
			
			var values = CalculateSplitPeriodRelativeValues();
			_skillStaffPeriod.SetDistributionValues(
				new PopulationStatisticsCalculatedValues(Calculation.Variances.StandardDeviation(values),
				                                         Calculation.Variances.RMS(values)), this);
		}

        public double[] CalculateSplitPeriodRelativeValues()
        {
	        var ret = new double[_splittedValues.Length];
            for (int i = 0; i < _splittedValues.Length; i++)
            {
                double traff = _splittedValues[i]/_periods[i].ElapsedTime().TotalMinutes;
                DeviationStatisticData stat = new DeviationStatisticData(_demandedTraff, traff);
                ret[i] = stat.RelativeDeviation;
            }
            return ret;
        }

        public double DeviationAfterNewLayers(IVisualLayerCollection layerCollection)
        {
            var tmp = fillInValuesFromLayers(layerCollection.FilterLayers(_period), _splittedValues);
            return Calculation.Variances.StandardDeviation(tmp);
        }

        private IEnumerable<double> fillInValuesFromLayers(IVisualLayerCollection layerCollectionFilteredByPeriod, double[] splittedValues)
        {
            if (!layerCollectionFilteredByPeriod.HasLayers)
                return splittedValues;

            IVisualLayerCollection layerCollectionFilteredByPeriodAndActivity = layerCollectionFilteredByPeriod.FilterLayers(_activity);
            if (!layerCollectionFilteredByPeriodAndActivity.HasLayers) return splittedValues;

	        var retValues = new double[splittedValues.Length];
            for (int i = 0; i < splittedValues.Length; i++)
            {
                var time = _period.StartDateTime.AddMinutes(i*_lengthToSplitOn);
                DateTimePeriod period = new DateTimePeriod(time,time.AddMinutes(_lengthToSplitOn));
                foreach (IVisualLayer layer in layerCollectionFilteredByPeriodAndActivity)
                {
                    DateTimePeriod? intersection = period.Intersection(layer.Period);
                    if (intersection.HasValue)
                    {
                        retValues[i] = splittedValues[i] + intersection.Value.ElapsedTime().TotalMinutes;
                    }
                }
            }
			return retValues;
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
            return Calculation.Variances.StandardDeviation(CalculateSplitPeriodRelativeValues());
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
	            return _splittedValues.Sum();
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
