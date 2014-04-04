using System;
using System.Collections.Generic;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.Secrets.SkillStaffPeriodDataHolder;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class SkillStaffPeriodDataInfo : ISkillStaffPeriodDataHolder
    {
        private readonly DateTimePeriod _period;
        private readonly IPeriodDistribution _periodDistribution;
        private Percent _overstaffingFactor;
        private readonly double _priorityValue;
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
            _periodDistribution = periodDistribution;

            _overstaffingFactor = overstaffingFactor;
            _priorityValue = priorityValue;
			TweakedCurrentDemand = SkillStaffPeriodCalculator.GetTweakedCurrentDemand(originalDemandInMinutes, assignedResourceInMinutes, overstaffingFactor.Value, priorityValue);
        }

        protected SkillStaffPeriodDataInfo()
        {}


        public double OriginalDemandInMinutes
        {
            get
            {
                if (_originalDemandInMinutes == 0)
                    return 1;

                return _originalDemandInMinutes;
            }
            set { _originalDemandInMinutes = value; }
        }

        public double TweakedCurrentDemand { get; set; }
        public bool Boost { get; set; }
        public double AssignedResourceInMinutes { get; set; }

        public DateTimePeriod Period
        {
            get { return _period; }
        }

        public int MinimumPersons { get; set; }

        public int MaximumPersons { get; set; }

        public double AbsoluteDifferenceScheduledHeadsAndMinMaxHeads { get; set; }

	    public DateTime PeriodStartDateTime { get { return _period.StartDateTime; }}
	    public DateTime PeriodEndDateTime { get { return _period.EndDateTime; } }

	    public double PeriodValue(int currentResourceInMinutes, bool useMinimumPersons, bool useMaximumPersons)
        {
            if (currentResourceInMinutes == 0)
                return 0;

            double partOfResolution = currentResourceInMinutes/Period.ElapsedTime().TotalMinutes;
            //double calculatedValue = OldCalculateWorkShiftPeriodValue(OriginalDemandInMinutes*partOfResolution,
            //                                                       AssignedResourceInMinutes*partOfResolution,
            //                                                       currentResourceInMinutes);

            double calculatedValue = SkillStaffPeriodCalculator.CalculateWorkShiftPeriodValue(OriginalDemandInMinutes*partOfResolution,
                                                                   TweakedCurrentDemand*partOfResolution,
                                                                    currentResourceInMinutes);
            double corrFactor;

            if (AssignedResourceInMinutes == 0 && Boost)
				corrFactor = SkillStaffPeriodCalculator.BigNumber;
            else
				corrFactor = SkillStaffPeriodCalculator.GetCorrectionFactor(useMinimumPersons, useMaximumPersons, AbsoluteDifferenceScheduledHeadsAndMinMaxHeads, MinimumPersons, AssignedResourceInMinutes);

            calculatedValue += corrFactor;

			//Debug.Print(_period.ToString() + ";" + OriginalDemandInMinutes + ";" + TweakedCurrentDemand + ";" + calculatedValue);

            return calculatedValue;
        }

        public IPeriodDistribution PeriodDistribution { get { return _periodDistribution; } }

		public bool CanCalculateDeviations()
	    {
		    return PeriodDistribution != null;
	    }

	    public double CalculateStandardDeviation()
	    {
		    return PeriodDistribution.CalculateStandardDeviation();
	    }

	    public double DeviationAfterNewLayers(IEnumerable<IWorkShiftCalculatableVisualLayer> mainShiftLayers)
	    {
			return PeriodDistribution.DeviationAfterNewLayers(mainShiftLayers as IVisualLayerCollection);
		}
    }
}
