using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Holds data needed to calculate staffing needs (activity start, end, length, occupancy etc.).
    /// </summary>
    public class SkillStaffPeriod : Layer<ISkillStaff>, ISkillStaffPeriod, IAggregateSkillStaffPeriod
    {
        private readonly SortedList<DateTime, ISkillStaffSegmentPeriod> _sortedSegmentCollection;
        private IList<ISkillStaffSegmentPeriod> _segmentInThisCollection;
        private readonly IStaffingCalculatorService _staffingCalculatorService;

        private bool _isAvailable;
        private bool _isAggregate;
        private double _aggregatedFStaff;
        private double _aggregatedCalculatedResources;
	    private Percent _estimatedServiceLevel;
        private Percent _estimatedServiceLevelShrinkage; 
        private IPeriodDistribution _periodDistribution;

	    private static readonly object Locker = new object();
	    
        public SkillStaffPeriod(DateTimePeriod period, ITask taskData, ServiceAgreement serviceAgreementData, IStaffingCalculatorService staffingCalculatorService) : base(new SkillStaff(taskData, serviceAgreementData), period)
        {
	        AggregatedStaffingThreshold = StaffingThreshold.Ok;
	        _sortedSegmentCollection = new SortedList<DateTime, ISkillStaffSegmentPeriod>();
            _staffingCalculatorService = staffingCalculatorService;
            _segmentInThisCollection = new List<ISkillStaffSegmentPeriod>();
        }

        public IStatisticTask StatisticTask { get; set; }

        public IActiveAgentCount ActiveAgentCount { get; set; }
        
        public double IntraIntervalDeviation { get; private set; }

        public double IntraIntervalRootMeanSquare { get; private set; }

        public double RelativeBoostedDifferenceForDisplayOnly { get; set; }

        public double ScheduledAgentsIncoming
        {
            get
            {
                return Payload.BookedAgainstIncomingDemand65 + (Payload.CalculatedResource - BookedResource65);
            }
        }

        public double IncomingDifference
        {
            get
            {
                return ScheduledAgentsIncoming - Payload.ForecastedIncomingDemand;
            }
        }

        public double BookedResource65
        {
            get
            {
                lock (Locker)
                {
                    double ret = 0;

                    for (int i = 0; i < _segmentInThisCollection.Count; i++)
                    {
                        ret += _segmentInThisCollection[i].BookedResource65;
                    }
                    
                    return ret;
                }
            }
        }

        public double CalculatedLoggedOn
        {
            get
            {
                lock (Locker)
                {
                    if (_isAggregate)
							  return ((IAggregateSkillStaffPeriod)this).AggregatedCalculatedLoggedOn;

                    return Payload.CalculatedLoggedOn;
                }
            }
        }

        public double AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(bool shiftValueMode)
        {
            if (Payload.SkillPersonData.MinimumPersons > 0 && Payload.CalculatedLoggedOn < Payload.SkillPersonData.MinimumPersons)
                return Payload.CalculatedLoggedOn - Payload.SkillPersonData.MinimumPersons;
            if (!shiftValueMode && Payload.SkillPersonData.MaximumPersons > 0 && Payload.CalculatedLoggedOn > Payload.SkillPersonData.MaximumPersons)
                return Payload.CalculatedLoggedOn - Payload.SkillPersonData.MaximumPersons;
            if (shiftValueMode && Payload.SkillPersonData.MaximumPersons > 0 && Payload.CalculatedLoggedOn >= Payload.SkillPersonData.MaximumPersons)
                return Payload.CalculatedLoggedOn - Payload.SkillPersonData.MaximumPersons +1;
            return 0;
        }

        public Percent EstimatedServiceLevel
        {
            get
            {
                lock (Locker)
                {
                    if (_isAggregate)
							  return ((IAggregateSkillStaffPeriod)this).AggregatedEstimatedServiceLevel;
                    return _estimatedServiceLevel;
                }
            } 
        }

        public Percent EstimatedServiceLevelShrinkage
        {
            get
            {
                lock (Locker)
                {
                    if (_isAggregate)
                        return ((IAggregateSkillStaffPeriod)this).AggregatedEstimatedServiceLevel;
                    return _estimatedServiceLevelShrinkage;
                }
            }
        }

        public Percent ActualServiceLevel
        {
            get
            {
                if (StatisticTask == null || StatisticTask.StatCalculatedTasks == 0)
                    return new Percent(double.NaN);
                //Never over 100%:
                return StatisticTask.StatAnsweredTasksWithinSL >= StatisticTask.StatCalculatedTasks ? new Percent(1) : new Percent(StatisticTask.StatAnsweredTasksWithinSL / StatisticTask.StatCalculatedTasks);
            }
        }

        public double ScheduledHours()
        {
            return CalculatedResource * Period.ElapsedTime().TotalHours;
        }

        public double CalculatedResource
        {
            get
            {
                lock (Locker)
                {
                    if (_isAggregate)
							  return ((IAggregateSkillStaffPeriod)this).AggregatedCalculatedResource;

                    return Payload.CalculatedResource;
                }
            }
        }

        public double FStaff
        {
            get {
                lock (Locker)
                {
                    if (_isAggregate)
							  return ((IAggregateSkillStaffPeriod)this).AggregatedFStaff;

	                return _segmentInThisCollection.Sum(ySegment => ySegment.FStaff());
                }
            }
        }

        public TimeSpan FStaffTime()
        {
            return TimeSpan.FromMinutes(FStaff*Period.ElapsedTime().TotalMinutes);
        }

        public double FStaffHours()
        {
            return FStaffTime().TotalHours;
        }

        public IList<ISkillStaffSegmentPeriod> SortedSegmentCollection
        {
            get { return new ReadOnlyCollection<ISkillStaffSegmentPeriod>(_sortedSegmentCollection.Values); }
        }

        public IList<ISkillStaffSegmentPeriod> SegmentInThisCollection
        {
            get { return new ReadOnlyCollection<ISkillStaffSegmentPeriod>(_segmentInThisCollection); }
        }

        public double RelativeDifference
        {
            get
            {
                return new DeviationStatisticData(FStaff, CalculatedResource).RelativeDeviation;
            }
        }

        public double RelativeDifferenceForDisplayOnly
        {
            get
            {
                return new DeviationStatisticData(FStaff, CalculatedResource).RelativeDeviationForDisplayOnly;
            }
        }

        public double RelativeDifferenceIncoming
        {
            get
            {
                if (Payload.ForecastedIncomingDemand == 0 && ScheduledAgentsIncoming == 0)
                    return 0;

                if (Payload.ForecastedIncomingDemand == 0)
                    return double.NaN;

                return IncomingDifference/Payload.ForecastedIncomingDemand;
            }
        }

        public double AbsoluteDifference
        {
            get { return CalculatedResource - FStaff; }
        }

        public double AbsoluteDifferenceMinStaffBoosted()
        {
            if (Payload.SkillPersonData.MinimumPersons > 0 && Payload.CalculatedLoggedOn < Payload.SkillPersonData.MinimumPersons)
                return ((Payload.CalculatedLoggedOn - Payload.SkillPersonData.MinimumPersons)* 10000) + AbsoluteDifference;

            return AbsoluteDifference;
        }

        public double AbsoluteDifferenceMaxStaffBoosted()
        {

            if (Payload.SkillPersonData.MaximumPersons > 0 && Payload.CalculatedLoggedOn > Payload.SkillPersonData.MaximumPersons)
                return ((Payload.CalculatedLoggedOn - Payload.SkillPersonData.MaximumPersons) * 10000) + AbsoluteDifference;

            return AbsoluteDifference;
        }

        public double AbsoluteDifferenceBoosted()
        {
            double ret = 0;
            if (Payload.SkillPersonData.MinimumPersons > 0 && Payload.CalculatedLoggedOn < Payload.SkillPersonData.MinimumPersons)
                ret= (Payload.CalculatedLoggedOn - Payload.SkillPersonData.MinimumPersons) * 10000;
            if (Payload.SkillPersonData.MaximumPersons > 0 && Payload.CalculatedLoggedOn > Payload.SkillPersonData.MaximumPersons)
                ret = (Payload.CalculatedLoggedOn - Payload.SkillPersonData.MaximumPersons) * 10000;

            return ret + AbsoluteDifference;
        }

        public double RelativeDifferenceMinStaffBoosted()
        {
            if (Payload.SkillPersonData.MinimumPersons > 0 && Payload.CalculatedLoggedOn < Payload.SkillPersonData.MinimumPersons)
                return ((Payload.CalculatedLoggedOn - Payload.SkillPersonData.MinimumPersons) * 10000) + RelativeDifference;

            return RelativeDifference;

        }

        public double RelativeDifferenceMaxStaffBoosted()
        {
            if (Payload.SkillPersonData.MaximumPersons > 0 && Payload.CalculatedLoggedOn > Payload.SkillPersonData.MaximumPersons)
                return ((Payload.CalculatedLoggedOn - Payload.SkillPersonData.MaximumPersons) * 10000) + RelativeDifference;

            return RelativeDifference;
        }

        public double RelativeDifferenceBoosted()
        {
            double ret = 0;
            if (Payload.SkillPersonData.MinimumPersons > 0 && Payload.CalculatedLoggedOn < Payload.SkillPersonData.MinimumPersons)
                ret = (Payload.CalculatedLoggedOn - Payload.SkillPersonData.MinimumPersons) * 10000;
            if (Payload.SkillPersonData.MaximumPersons > 0 && Payload.CalculatedLoggedOn > Payload.SkillPersonData.MaximumPersons)
                ret = (Payload.CalculatedLoggedOn - Payload.SkillPersonData.MaximumPersons) * 10000;

            return ret + RelativeDifference;
        }

        public IList<ISkillStaffPeriodView> Split(TimeSpan periodLength)
        {
            if (Period.ElapsedTime() < periodLength)
                throw new ArgumentOutOfRangeException("periodLength", periodLength,
                                                      "You cannot split to a higher period length");
       
            if(Period.ElapsedTime().TotalMinutes % periodLength.TotalMinutes > 0)
                throw new ArgumentOutOfRangeException("periodLength", periodLength,
                                                      "You cannot split if you get a remaining time");

            IList<ISkillStaffPeriodView> newViews = new List<ISkillStaffPeriodView>();
            IList<DateTimePeriod> newPeriods = Period.Intervals(periodLength);
            foreach (DateTimePeriod newPeriod in newPeriods)
            {
                ISkillStaffPeriodView newView = new SkillStaffPeriodView
                                                	{
                                                		Period = newPeriod,
                                                		CalculatedResource = CalculatedResource,
                                                		ForecastedIncomingDemand = Payload.ForecastedIncomingDemand,
                                                		ForecastedIncomingDemandWithShrinkage =
                                                			Payload.CalculatedTrafficIntensityWithShrinkage,
                                                		FStaff = FStaff
                                                	};
            	newViews.Add(newView);
            }

            return newViews;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void SetDistributionValues(IPopulationStatisticsCalculatedValues calculatedValues, IPeriodDistribution periodDistribution)
        {
            IntraIntervalDeviation = calculatedValues.StandardDeviation;
            IntraIntervalRootMeanSquare = calculatedValues.RootMeanSquare;
            _periodDistribution = periodDistribution;
        }

        public void ClearIntraIntervalDistribution()
        {
            IntraIntervalDeviation = 0;
            IntraIntervalRootMeanSquare = 0;
        }

	    public void SetSkillDay(ISkillDay skillDay)
	    {
			SkillDay = skillDay;
	    }

	    public ISkillDay SkillDay { get; private set; }

        private void CalculateEstimatedServiceLevel()
        {
            var parent = SkillDay;
            
            //Never over 100%, if demand = 0 then 100 if Email etc
            if (parent != null && parent.Skill.SkillType.ForecastSource != ForecastSource.InboundTelephony 
				&& parent.Skill.SkillType.ForecastSource!=ForecastSource.Retail
				&& parent.Skill.SkillType.ForecastSource!=ForecastSource.Chat)
            {
                if (Payload.ForecastedIncomingDemand == 0)
                {
                    _estimatedServiceLevel = new Percent(1);
                    _estimatedServiceLevelShrinkage = new Percent(1);
                    return;
                }

                if (ScheduledAgentsIncoming >= Payload.ForecastedIncomingDemand)
                {
                    _estimatedServiceLevel = new Percent(1);
                    _estimatedServiceLevelShrinkage = new Percent(1);
                    return;
                }

                _estimatedServiceLevel = new Percent(ScheduledAgentsIncoming / Payload.ForecastedIncomingDemand);
                var shrinkage = 1 - Payload.Shrinkage.Value;
                var scheduledAgentsIncomingWithShrinkage = ScheduledAgentsIncoming * shrinkage;
                _estimatedServiceLevelShrinkage = new Percent(
                    scheduledAgentsIncomingWithShrinkage/Payload.ForecastedIncomingDemand);
            }
            else
            {
                _estimatedServiceLevel = new Percent(_staffingCalculatorService.ServiceLevelAchieved(
                    ScheduledAgentsIncoming * SkillDay.Skill.MaxParallelTasks * Payload.Efficiency.Value,
                    Payload.ServiceAgreementData.ServiceLevel.Seconds,
                    Payload.TaskData.Tasks,
                    Payload.TaskData.AverageTaskTime.TotalSeconds + Payload.TaskData.AverageAfterTaskTime.TotalSeconds,
                    Period.ElapsedTime(),
                    (int)(Payload.ServiceAgreementData.ServiceLevel.Percent.Value * 100)));

                var shrinkage = 1 - Payload.Shrinkage.Value;
                var scheduledAgentsIncomingWithShrinkage = ScheduledAgentsIncoming * shrinkage;
                _estimatedServiceLevelShrinkage = new Percent(_staffingCalculatorService.ServiceLevelAchieved(
                    scheduledAgentsIncomingWithShrinkage * SkillDay.Skill.MaxParallelTasks * Payload.Efficiency.Value,
                    Payload.ServiceAgreementData.ServiceLevel.Seconds,
                    Payload.TaskData.Tasks,
                    Payload.TaskData.AverageTaskTime.TotalSeconds + Payload.TaskData.AverageAfterTaskTime.TotalSeconds,
                    Period.ElapsedTime(),
                    (int)(Payload.ServiceAgreementData.ServiceLevel.Percent.Value * 100)));

            }
        }

        public void SetCalculatedResource65(double value)
        {
            SkillStaff skillStaff = (SkillStaff) Payload;
            skillStaff.CalculatedResource = value;

            if(SegmentInThisCollection.Count == 1)
            {
                PickResources65();
            }
            else
            {
                var sortedSegmentInThisCollection =
                 SegmentInThisCollection.OrderBy(s => s.BelongsTo.Period.StartDateTime).ToList();

                foreach (var ySegment in sortedSegmentInThisCollection)
                {
                    ySegment.BelongsTo.PickResources65();
                } 
            }
        }

        public TimeSpan ForecastedIncomingDemand()
        {
            return TimeSpan.FromMinutes(Payload.ForecastedIncomingDemand*Period.ElapsedTime().TotalMinutes);
        }

        public TimeSpan ForecastedIncomingDemandWithShrinkage()
        {
            return TimeSpan.FromMinutes(ForecastedIncomingDemand().TotalMinutes / (1 - Payload.Shrinkage.Value));
        }

        public void CalculateStaff(IList<ISkillStaffPeriod> periods)
        {
            if (Payload.IsCalculated) return;

            double traffic = 0;
            double maxOcc = Payload.ServiceAgreementData.MaxOccupancy.Value;

            if (maxOcc == 0) maxOcc = 1;

            double minOcc =
                Payload.MultiskillMinOccupancy.GetValueOrDefault(Payload.ServiceAgreementData.MinOccupancy).Value;

            if (Payload.TaskData.Tasks == 0 && Payload.ServiceAgreementData.MinOccupancy.Value == 0)
                traffic = 0;
            else
            {
				if (!Payload.ManualAgents.HasValue && !Payload.NoneBlendDemand.HasValue)
				{
					var maxParallel = SkillDay.Skill.MaxParallelTasks;
					traffic = _staffingCalculatorService.AgentsUseOccupancy(
					Payload.ServiceAgreementData.ServiceLevel.Percent.Value,
					(int)Math.Round(Payload.ServiceAgreementData.ServiceLevel.Seconds),
					Payload.TaskData.Tasks,
					Payload.TaskData.AverageTaskTime.TotalSeconds + Payload.TaskData.AverageAfterTaskTime.TotalSeconds,
					Period.ElapsedTime(),
					minOcc,
					maxOcc,
					maxParallel);
				}
                
            }

        	var castedPayLoad = ((SkillStaff) Payload);
	        double demandWithoutEfficiency;
	        if (!Payload.ManualAgents.HasValue && !Payload.NoneBlendDemand.HasValue)
	        {
		        castedPayLoad.ForecastedIncomingDemand = traffic;
		        demandWithoutEfficiency = traffic;
	        }
	        else
			{
				if (Payload.ManualAgents.HasValue)
				{
					castedPayLoad.ForecastedIncomingDemand = Payload.ManualAgents.Value;
					demandWithoutEfficiency = Payload.ManualAgents.Value;
				}
				else
				{
					castedPayLoad.ForecastedIncomingDemand = Payload.NoneBlendDemand.Value;
					demandWithoutEfficiency = Payload.NoneBlendDemand.Value;
				}
			}

			castedPayLoad.CalculatedOccupancy = _staffingCalculatorService.Utilization(demandWithoutEfficiency, Payload.TaskData.Tasks,
                                                      Payload.TaskData.AverageTaskTime.TotalSeconds +
                                                      Payload.TaskData.AverageAfterTaskTime.TotalSeconds, Period.ElapsedTime());
            if (periods != null)
            {
                int thisIndex = periods.IndexOf(this);
                if (thisIndex == -1)
                    throw new ArgumentException("List with skillstaffperiods must contain working skillstaffperiod");
                RemoveExistingSegments(periods, thisIndex);
                CreateSkillStaffSegments65(periods, thisIndex);
            }

            castedPayLoad.IsCalculated = true;
        }

        private void RemoveExistingSegments(IList<ISkillStaffPeriod> skillStaffPeriods, int currentIndex)
        {
            _sortedSegmentCollection.Clear();
            for (int i = currentIndex; i < skillStaffPeriods.Count; i++)
            {
                SkillStaffPeriod period = skillStaffPeriods[i] as SkillStaffPeriod;


                if (period == null) continue;
                var belongsToThis = period.SegmentInThisCollection.Where(s => s.BelongsTo.Equals(this)).ToList();
                if (belongsToThis.Count==0 && i>currentIndex) break; //Because we do this sequential, we can stop when no more distributed segments are found

                foreach (ISkillStaffSegmentPeriod segmentPeriod in belongsToThis)
                {
                    period._segmentInThisCollection.Remove(segmentPeriod);
                }
            }
        }

        public void CalculateStaff()
        {
            CalculateStaff(new List<ISkillStaffPeriod> { this });
        }

        public static ISkillStaffPeriod Combine(IList<ISkillStaffPeriod> skillStaffPeriods)
        {
            if (skillStaffPeriods.Count < 1)
                return null;
            if (skillStaffPeriods.Count == 1)
                return (ISkillStaffPeriod)skillStaffPeriods[0].NoneEntityClone();

            DateTimePeriod period = skillStaffPeriods[0].Period;
            for (int index = 1; index<skillStaffPeriods.Count;index++)
            {
                TimeSpan elapsedTime = skillStaffPeriods[index].Period.ElapsedTime();
                if (elapsedTime != period.ElapsedTime())
                    InParameter.CheckTimeLimit("skillStaffPeriods", elapsedTime,0);
            }

            double tasks = 0;
            double taskSeconds = 0;
            double afterTaskSeconds = 0;
            double serviceSeconds = 0;
            double servicePercent = 0;
            double minOcc = 0;
            double maxOcc = 0;
            double resource = 0;
            bool isAvail = false;
            bool useShrinkage = false;
	        double forecastedIncomingHours = 0;

            foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriods)
            {
                tasks += skillStaffPeriod.Payload.TaskData.Tasks;
                taskSeconds += skillStaffPeriod.Payload.TaskData.AverageTaskTime.TotalSeconds * skillStaffPeriod.Payload.TaskData.Tasks;
                afterTaskSeconds += skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime.TotalSeconds * skillStaffPeriod.Payload.TaskData.Tasks;
                serviceSeconds += skillStaffPeriod.Payload.ServiceAgreementData.ServiceLevel.Seconds * skillStaffPeriod.Payload.TaskData.Tasks;
                servicePercent += skillStaffPeriod.Payload.ServiceAgreementData.ServiceLevel.Percent.Value * skillStaffPeriod.Payload.TaskData.Tasks;
                minOcc += skillStaffPeriod.Payload.ServiceAgreementData.MinOccupancy.Value * skillStaffPeriod.Payload.TaskData.Tasks;
                maxOcc += skillStaffPeriod.Payload.ServiceAgreementData.MaxOccupancy.Value * skillStaffPeriod.Payload.TaskData.Tasks;
                resource += skillStaffPeriod.Payload.CalculatedResource;
                if(!isAvail)
                    isAvail = skillStaffPeriod.IsAvailable;
                useShrinkage = skillStaffPeriod.Payload.UseShrinkage;

	            forecastedIncomingHours += skillStaffPeriod.Payload.ForecastedIncomingDemand;
            }
	        var skillDay = skillStaffPeriods[0].SkillDay;
            if (tasks == 0)
            {
	            var newPeriod = new SkillStaffPeriod(period, new Task(), ServiceAgreement.DefaultValues(), skillStaffPeriods[0].StaffingCalculatorService);
				newPeriod.SetSkillDay(skillDay);
				return newPeriod;
            }

            Task retTask = new Task(tasks,TimeSpan.FromSeconds(taskSeconds/tasks),TimeSpan.FromSeconds(afterTaskSeconds/tasks));
            ServiceLevel retLevel = new ServiceLevel(new Percent(servicePercent/tasks),serviceSeconds/tasks);
            Percent retMinOcc = new Percent(minOcc / tasks);
            Percent retMaxOcc = new Percent(maxOcc / tasks);
            ServiceAgreement retAgr = new ServiceAgreement(retLevel, retMinOcc, retMaxOcc);

            SkillStaffPeriod ret = new SkillStaffPeriod(period, retTask, retAgr, skillStaffPeriods[0].StaffingCalculatorService);
            ret.IsAvailable = isAvail;
            ret.Payload.UseShrinkage = useShrinkage;
            ret.SetCalculatedResource65(resource);
			((SkillStaff)ret.Payload).ForecastedIncomingDemand = forecastedIncomingHours;
			ret.SetSkillDay(skillDay);
			
            return ret;
        }

        public double ForecastedDistributedDemand
        {
            get
            {
                lock (Locker)
                {
                    double ret = 0;
                    foreach (ISkillStaffSegmentPeriod segmentPeriod in _segmentInThisCollection)
                    {
                        ISkillStaffPeriod owner = segmentPeriod.BelongsTo;
                        var segmentCount = owner.SortedSegmentCollection.Count;
                        if (segmentCount > 0)
                        {
                            ret += owner.Payload.ForecastedIncomingDemand/segmentCount;
                        }
                    }
                    return ret;
                }
            }
        }

        public double ForecastedDistributedDemandWithShrinkage
        {
            get { return ForecastedDistributedDemand * (1d + Payload.Shrinkage.Value); }
        }

        public bool IsAvailable
        {
            get { return _isAvailable; }
            set { _isAvailable = value; }
        }

        public IStaffingCalculatorService StaffingCalculatorService
        {
            get { return _staffingCalculatorService; }
        }

        public void PickResources65()
        {
            SkillStaff thisSkillStaff = (SkillStaff)Payload;

            if (SortedSegmentCollection.Count == 1)
            {
                thisSkillStaff.BookedAgainstIncomingDemand65 = Payload.CalculatedResource;
                SortedSegmentCollection[0].BookedResource65 = Payload.CalculatedResource;
            }

            thisSkillStaff.BookedAgainstIncomingDemand65 = 0;

            foreach (ISkillStaffSegmentPeriod xSegment in SortedSegmentCollection)
            {
                xSegment.BookedResource65 = 0;
                
                double diff = Payload.ForecastedIncomingDemand - Payload.BookedAgainstIncomingDemand65;
                if (diff > 0)
                {

                    ISkillStaffPeriod ownerSkillStaffPeriod = xSegment.BelongsToY;
                    ISkillStaff ownerSkillStaff = ownerSkillStaffPeriod.Payload;
                    var ownerSortedSegmentCollection = ownerSkillStaffPeriod.SortedSegmentCollection;
                    if (ownerSortedSegmentCollection.Count == 0)
                        continue;

                    double ownerNotBookedResource = ownerSkillStaff.CalculatedResource -
                                                (ownerSkillStaffPeriod.BookedResource65 - ownerSortedSegmentCollection[0].BookedResource65);
                    if (ownerNotBookedResource <= 0)
                        continue;


                    if (diff >= ownerNotBookedResource)
                        diff = ownerNotBookedResource;

                    thisSkillStaff.BookedAgainstIncomingDemand65 += diff;
                    xSegment.BookedResource65 = diff;
                }
            }

            CalculateEstimatedServiceLevel();
        }

        public ISkillStaffPeriod IntersectingResult(DateTimePeriod period)
        {
            if (!Period.Intersect(period))
                return null;

            ISkillStaffPeriod skillStaffPeriod;
            if (period.Contains(Period))
            {
                skillStaffPeriod = new SkillStaffPeriod(period, Payload.TaskData,
                    new ServiceAgreement(Payload.ServiceAgreementData.ServiceLevel, 
                                        Payload.ServiceAgreementData.MinOccupancy,
                                        Payload.ServiceAgreementData.MaxOccupancy),_staffingCalculatorService);
                skillStaffPeriod.Payload.Shrinkage = Payload.Shrinkage;
                skillStaffPeriod.Payload.SkillPersonData = Payload.SkillPersonData;
                return skillStaffPeriod;
            }

            DateTimePeriod intersection = Period.Intersection(period).Value;
            double interectPercent = intersection.ElapsedTime().TotalSeconds/
                                      Period.ElapsedTime().TotalSeconds;
            double newTasks = interectPercent * Payload.TaskData.Tasks;
            ITask newTaskData = new Task(newTasks, Payload.TaskData.AverageTaskTime, Payload.TaskData.AverageAfterTaskTime);

            skillStaffPeriod = new SkillStaffPeriod(period, newTaskData,
                    new ServiceAgreement(Payload.ServiceAgreementData.ServiceLevel,
                                        Payload.ServiceAgreementData.MinOccupancy,
                                        Payload.ServiceAgreementData.MaxOccupancy),_staffingCalculatorService);
            skillStaffPeriod.Payload.Shrinkage = Payload.Shrinkage;
            return skillStaffPeriod;
        }

        private static void AddToLists(double traffToDistribute, ISkillStaffPeriod currentPeriod, ISkillStaffPeriod ourPeriod)
        {
            ISkillStaffSegment newSegment = new SkillStaffSegment(traffToDistribute);
            ISkillStaffSegmentPeriod newSegmentPeriod = new SkillStaffSegmentPeriod(ourPeriod, currentPeriod, newSegment, currentPeriod.Period);
            ((SkillStaffPeriod)ourPeriod)._sortedSegmentCollection.Add(newSegmentPeriod.Period.StartDateTime, newSegmentPeriod);
            ((SkillStaffPeriod)currentPeriod)._segmentInThisCollection.Add(newSegmentPeriod);
        }

        private void CreateSkillStaffSegments65(IList<ISkillStaffPeriod> sortedPeriods, int currentIndex)
        {
            ISkillStaffPeriod ourPeriod = sortedPeriods[currentIndex];
	        TimeSpan sa = TimeSpan.FromSeconds(Payload.ServiceAgreementData.ServiceLevel.Seconds);

            while ((sa.TotalSeconds > 0.1) && (currentIndex < sortedPeriods.Count))
            {
                ISkillStaffPeriod currentPeriod = sortedPeriods[currentIndex];
                TimeSpan currentLength = currentPeriod.Period.ElapsedTime();
                AddToLists(0, currentPeriod, ourPeriod);

                currentIndex++;
                sa = sa.Add(-currentLength);
            }
        }

        public void Reset()
        {
            Payload.Reset();
            _sortedSegmentCollection.Clear();
            _segmentInThisCollection = _segmentInThisCollection.Where(s => !s.BelongsTo.Equals(this)).ToList();
            _isAvailable = false;
        }

        public override string ToString()
        {
            return base.ToString() + ", " + Period;
        }

        bool IAggregateSkillStaffPeriod.IsAggregate
        {
            get { return _isAggregate; }
            set { _isAggregate = value; }
        }

        double IAggregateSkillStaffPeriod.AggregatedFStaff
        {
            get { return _aggregatedFStaff; }
            set { _aggregatedFStaff = value; }
        }

        double IAggregateSkillStaffPeriod.AggregatedCalculatedLoggedOn
        {
            get { return double.NaN; }
        }
       
        void IAggregateSkillStaffPeriod.CombineAggregatedSkillStaffPeriod(IAggregateSkillStaffPeriod aggregateSkillStaffPeriod)
        {
            if(_isAggregate && aggregateSkillStaffPeriod.IsAggregate)
            {
                _aggregatedFStaff += aggregateSkillStaffPeriod.AggregatedFStaff;
                _aggregatedCalculatedResources += aggregateSkillStaffPeriod.AggregatedCalculatedResource;
                AggregatedEstimatedServiceLevel = CombineEstimatedServiceLevel(aggregateSkillStaffPeriod);
                AggregatedForecastedIncomingDemand += aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand;
                AggregatedMinMaxStaffAlarm = combineMinMaxStaffAlarm(aggregateSkillStaffPeriod);
                AggregatedStaffingThreshold = combineStaffingTreshold(aggregateSkillStaffPeriod);
            }
            else
            {
                throw new InvalidCastException("Both instances must be aggregated");
            }
        }

        private StaffingThreshold combineStaffingTreshold(IAggregateSkillStaffPeriod aggregateSkillStaffPeriod)
        {
            int thisStatus = (int) AggregatedStaffingThreshold;
            int otherStatus = (int) aggregateSkillStaffPeriod.AggregatedStaffingThreshold;
            return (StaffingThreshold) Math.Max(thisStatus, otherStatus);
        }

        private MinMaxStaffBroken combineMinMaxStaffAlarm(IAggregateSkillStaffPeriod aggregateSkillStaffPeriod)
        {
            if (AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.BothBroken)
                return AggregatedMinMaxStaffAlarm;

            if (AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.Ok && aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm != MinMaxStaffBroken.Ok)
                return aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm;

            if(aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.BothBroken)
                return aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm;

            if (AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MinStaffBroken && aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MaxStaffBroken)
                return MinMaxStaffBroken.BothBroken;

            if (AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MaxStaffBroken && aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MinStaffBroken)
                return MinMaxStaffBroken.BothBroken;

            return AggregatedMinMaxStaffAlarm;
        }

        private Percent CombineEstimatedServiceLevel(IAggregateSkillStaffPeriod aggregateSkillStaffPeriod)
        {
            double value = (AggregatedForecastedIncomingDemand*AggregatedEstimatedServiceLevel.Value) + 
                (aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand * aggregateSkillStaffPeriod.AggregatedEstimatedServiceLevel.Value);
            double demand = AggregatedForecastedIncomingDemand +
                            aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand;
            if (demand == 0)
                return new Percent(1);
            return new Percent(value / demand);
        }

        double IAggregateSkillStaffPeriod.AggregatedCalculatedResource
        {
            get { return _aggregatedCalculatedResources; }
            set { _aggregatedCalculatedResources = value; }
        }

	    public Percent AggregatedEstimatedServiceLevel { get; set; }

	    public double AggregatedForecastedIncomingDemand { get; set; }

	    public MinMaxStaffBroken AggregatedMinMaxStaffAlarm { get; set; }

	    public StaffingThreshold AggregatedStaffingThreshold { get; set; }

	    public IPeriodDistribution PeriodDistribution
        {
            get
            {
                return _periodDistribution;
            }
        }
    }

	public class PopulationStatisticsCalculatedValues : IPopulationStatisticsCalculatedValues
	{
		public PopulationStatisticsCalculatedValues(double standardDeviation, double rootMeanSquare)
		{
			RootMeanSquare = rootMeanSquare;
			StandardDeviation = standardDeviation;
		}

		public double StandardDeviation { get; private set; }
		public double RootMeanSquare { get; private set; }
	}
}
