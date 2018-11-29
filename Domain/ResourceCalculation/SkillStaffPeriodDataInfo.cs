using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class SkillStaffPeriodDataInfo : ISkillStaffPeriodDataHolder
    {
        private DateTimePeriod _period;
	    private double _originalDemandInMinutes;

        public SkillStaffPeriodDataInfo(int originalDemandInMinutes, int assignedResourceInMinutes, DateTimePeriod period, int minimumPersons, int maximumPersons, double absoluteDifferenceScheduledHeadsAndMinMaxHeads, IPeriodDistribution periodDistribution)
            : this(originalDemandInMinutes, assignedResourceInMinutes, period, minimumPersons, maximumPersons, absoluteDifferenceScheduledHeadsAndMinMaxHeads, periodDistribution, new Percent(.5),1)
        {}
        
        public SkillStaffPeriodDataInfo(double originalDemandInMinutes, double assignedResourceInMinutes, DateTimePeriod period, int minimumPersons, int maximumPersons,
            double absoluteDifferenceScheduledHeadsAndMinMaxHeads, IPeriodDistribution periodDistribution, Percent overstaffingFactor, double priorityValue)
        {
            OriginalDemandInMinutes = originalDemandInMinutes;
            AssignedResourceInMinutes = assignedResourceInMinutes;
            MinimumPersons = minimumPersons;
            MaximumPersons = maximumPersons;
            _period = period;
            AbsoluteDifferenceScheduledHeadsAndMinMaxHeads = absoluteDifferenceScheduledHeadsAndMinMaxHeads;
            PeriodDistribution = periodDistribution;
	        ElapsedTimeMinutes = _period.ElapsedTime().TotalMinutes;

			TweakedCurrentDemand = SkillStaffPeriodCalculator.GetTweakedCurrentDemand(originalDemandInMinutes, assignedResourceInMinutes, overstaffingFactor.Value, priorityValue);
        }

        protected SkillStaffPeriodDataInfo()
        {}


        public double OriginalDemandInMinutes
        {
            get => _originalDemandInMinutes;
	        set
	        {
		        if (value == 0)
		        {
			        value = 1;
		        }
				_originalDemandInMinutes = value;
	        }
        }

        public double TweakedCurrentDemand { get; set; }
        public bool Boost { get; set; }
        public double AssignedResourceInMinutes { get; set; }

        public DateTimePeriod Period => _period;

	    public int MinimumPersons { get; set; }

        public int MaximumPersons { get; set; }

        public double AbsoluteDifferenceScheduledHeadsAndMinMaxHeads { get; set; }

	    public DateTime PeriodStartDateTime => _period.StartDateTime;
	    public DateTime PeriodEndDateTime => _period.EndDateTime;
		public double ElapsedTimeMinutes { get; }

	    public double PeriodValue(int currentResourceInMinutes, bool useMinimumPersons, bool useMaximumPersons)
        {
            if (currentResourceInMinutes == 0)
                return 0;

            double partOfResolution = currentResourceInMinutes/ElapsedTimeMinutes;
            
            double calculatedValue = SkillStaffPeriodCalculator.CalculateWorkShiftPeriodValue(OriginalDemandInMinutes*partOfResolution,
                                                                   TweakedCurrentDemand*partOfResolution,
                                                                    currentResourceInMinutes);
            double corrFactor;

	        if (AssignedResourceInMinutes == 0 && Boost)
		        corrFactor = SkillStaffPeriodCalculator.TheBigNumber;
            else
				corrFactor = SkillStaffPeriodCalculator.GetCorrectionFactor(useMinimumPersons, useMaximumPersons, AbsoluteDifferenceScheduledHeadsAndMinMaxHeads, MinimumPersons, AssignedResourceInMinutes);

            calculatedValue += corrFactor;

            return calculatedValue;
        }

        public IPeriodDistribution PeriodDistribution { get; }

	    public bool CanCalculateDeviations()
	    {
		    return PeriodDistribution != null;
	    }

	    public double CalculateStandardDeviation()
	    {
		    return PeriodDistribution.CalculateStandardDeviation();
	    }

	    public double DeviationAfterNewLayers(IEnumerable<IWorkShiftCalculatableLayer> mainShiftLayers)
	    {
			return PeriodDistribution.DeviationAfterNewLayers(((WorkShiftCalculatableVisualLayerCollection) mainShiftLayers).Inner);
		}
    }
}
