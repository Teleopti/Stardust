using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Used for calculating standard deviation for intervalls
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-10-27    
    /// /// </remarks>
    public class PeriodDistributionService
    {
        private readonly IResourceCalculationDataContainer _relevantProjections;
        private readonly int _lengthToSplitOn;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodDistributionService"/> class.
        /// </summary>
        /// <param name="relevantProjections">The relevant projections.</param>
        /// <param name="lengthToSplitOn">The length to split on.</param>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-27    
        /// /// </remarks>
        public PeriodDistributionService(IResourceCalculationDataContainer relevantProjections, int lengthToSplitOn)
        {
            _relevantProjections = relevantProjections;
            _lengthToSplitOn = lengthToSplitOn;
        }

        /// <summary>
        /// Calculates the day.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// ///
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-10-27
        /// /// </remarks>
        public void CalculateDay(ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriods)
        {
			var periodDistr = new SortedList<DateTimePeriod, ICollection<PeriodDistribution>>(new DateTimePeriodPerStartTimeSorter());
            foreach (var skillStaffPeriodDictionary in skillStaffPeriods)
            {
                foreach (var pair in skillStaffPeriodDictionary.Value)
                {
                    var period = pair.Key;
                    var demandedTraff = pair.Value.FStaff;
                    var periodDistribution =
                        new PeriodDistribution(pair.Value, skillStaffPeriodDictionary.Key.Activity, period, _lengthToSplitOn, demandedTraff);

                	addPeriodDistributionToList(periodDistr, period, periodDistribution);
                    pair.Value.ClearIntraIntervalDistribution();
                }
            }

        	/*foreach (var projection in _relevantProjections)
        	{
        		foreach (var pair in periodDistr)
        		{
        			var period = pair.Key;
        			var projFilteredByPeriod = projection.FilterLayers(period);
					if(!projFilteredByPeriod.HasLayers)
						continue;
        			foreach (var periodDistribution in pair.Value)
        			{
        				periodDistribution.ProcessLayers(projFilteredByPeriod);
        			}
        		}	
        	}*/
        }

    	private static void addPeriodDistributionToList(SortedList<DateTimePeriod, ICollection<PeriodDistribution>> periodDistributionCollection, 
												DateTimePeriod period, 
												PeriodDistribution periodDistribution)
    	{
    		ICollection<PeriodDistribution> periodDistrList;
    		if(!periodDistributionCollection.TryGetValue(period, out periodDistrList))
    		{
    			periodDistrList = new List<PeriodDistribution>();
    			periodDistributionCollection[period] = periodDistrList;
    		}
			periodDistrList.Add(periodDistribution);
    	}
    }
 }
