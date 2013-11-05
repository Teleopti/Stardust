using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

		#region Fields (3) 

        private readonly SortedList<DateTime, ISkillStaffSegmentPeriod> _sortedSegmentCollection;
        private IList<ISkillStaffSegmentPeriod> _segmentInThisCollection;
        private readonly IStaffingCalculatorService _staffingCalculatorService;

        private bool _isAvailable;
        private bool _isAggregate;
        private double _aggregatedFStaff;
        private double _aggregatedCalculatedResources;
        private Percent _aggregatedEstimatedServiceLevel;
        private double _aggregatedForecastedIncomingDemand;
        private Percent _estimatedServiceLevel;
        private IPeriodDistribution _periodDistribution;
        private MinMaxStaffBroken _aggregatedMinMaxStaffAlarm;
        private StaffingThreshold _aggregatedStaffingThreshold = StaffingThreshold.Ok;

        private static readonly object Locker = new object();
	    
	    #endregion Fields 

		#region Constructors (2) 

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillStaffPeriod"/> class.
        /// Used by tests
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="taskData">The task data.</param>
        /// <param name="serviceAgreementData">The service agreement data.</param>
        /// <param name="staffingCalculatorService">The staffing calculator service.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-24
        /// </remarks>
        public SkillStaffPeriod(DateTimePeriod period, ITask taskData, ServiceAgreement serviceAgreementData, IStaffingCalculatorService staffingCalculatorService) : base(new SkillStaff(taskData, serviceAgreementData), period)
        {
            _sortedSegmentCollection = new SortedList<DateTime, ISkillStaffSegmentPeriod>();
            _staffingCalculatorService = staffingCalculatorService;
            _segmentInThisCollection = new List<ISkillStaffSegmentPeriod>();
        }

		#endregion Constructors 

		#region Properties (3) 

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

                    double ret = 0;
                    foreach (ISkillStaffSegmentPeriod ySegment in _segmentInThisCollection)
                    {
                        ret += ySegment.FStaff();
                    }
                    return ret;
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
        public void SetDistributionValues(IPopulationStatisticsCalculator calculator, IPeriodDistribution periodDistribution)
        {
            IntraIntervalDeviation = calculator.StandardDeviation;
            IntraIntervalRootMeanSquare = calculator.RootMeanSquare;
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

	    #endregion Properties 

		#region Methods 

        private void CalculateEstimatedServiceLevel()
        {
            var parent = SkillDay;
            
            //Never over 100%, if demand = 0 then 100 if Email etc
            if (parent != null && parent.Skill.SkillType.ForecastSource != ForecastSource.InboundTelephony && parent.Skill.SkillType.ForecastSource!=ForecastSource.Retail)
            {
                if (Payload.ForecastedIncomingDemand == 0)
                {
                    _estimatedServiceLevel = new Percent(1);
                    return;
                }

                if (ScheduledAgentsIncoming >= Payload.ForecastedIncomingDemand)
                {
                    _estimatedServiceLevel = new Percent(1);
                    return;
                }
                _estimatedServiceLevel = new Percent(ScheduledAgentsIncoming / Payload.ForecastedIncomingDemand);
            }
            else
            {
                _estimatedServiceLevel = new Percent(_staffingCalculatorService.ServiceLevelAchieved(
                                                   ScheduledAgentsIncoming * Payload.Efficiency.Value,
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

		// Public Methods (8) 

        /// <summary>
        /// Gets the calculated staff time during this period.
        /// </summary>
        /// <value>The calculated staff minutes.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-24
        /// </remarks>
        public TimeSpan ForecastedIncomingDemand()
        {
            return TimeSpan.FromMinutes(Payload.ForecastedIncomingDemand*Period.ElapsedTime().TotalMinutes);
        }


        /// <summary>
        /// Calculateds the staff time with shrinkage.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-08
        /// </remarks>
        public TimeSpan ForecastedIncomingDemandWithShrinkage()
        {
            return TimeSpan.FromMinutes(ForecastedIncomingDemand().TotalMinutes / (1 - Payload.Shrinkage.Value));
        }

        /// <summary>
        /// Calculates the staff and distributes the traff down to segmentPeriods.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-04-18
        /// </remarks>
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
					traffic = _staffingCalculatorService.AgentsUseOccupancy(
					Payload.ServiceAgreementData.ServiceLevel.Percent.Value,
					(int)Math.Round(Payload.ServiceAgreementData.ServiceLevel.Seconds),
					Payload.TaskData.Tasks,
					Payload.TaskData.AverageTaskTime.TotalSeconds + Payload.TaskData.AverageAfterTaskTime.TotalSeconds,
					Period.ElapsedTime(),
					minOcc,
					maxOcc);
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

        /// <summary>
        /// Calculates the staff.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-04-18
        /// </remarks>
        public void CalculateStaff()
        {
            CalculateStaff(new List<ISkillStaffPeriod> { this });
        }

        /// <summary>
        /// Combines the specified skill staff periods. ServiceLevels are weighted according to number of tasks.
        /// All periods must be the of the same length.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-06
        /// </remarks>
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
            }

            if (tasks == 0)
                return new SkillStaffPeriod(period, new Task(), ServiceAgreement.DefaultValues(), skillStaffPeriods[0].StaffingCalculatorService);

            Task retTask = new Task(tasks,TimeSpan.FromSeconds(taskSeconds/tasks),TimeSpan.FromSeconds(afterTaskSeconds/tasks));
            ServiceLevel retLevel = new ServiceLevel(new Percent(servicePercent/tasks),serviceSeconds/tasks);
            Percent retMinOcc = new Percent(minOcc / tasks);
            Percent retMaxOcc = new Percent(maxOcc / tasks);
            ServiceAgreement retAgr = new ServiceAgreement(retLevel, retMinOcc, retMaxOcc);

            SkillStaffPeriod ret = new SkillStaffPeriod(period, retTask, retAgr, skillStaffPeriods[0].StaffingCalculatorService);
            ret.IsAvailable = isAvail;
            ret.Payload.UseShrinkage = useShrinkage;
            ret.SetCalculatedResource65(resource);

            return ret;
        }

        /// <summary>
        /// Returns the sum if all forecasted distributed demands
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-08-28
        /// </remarks>
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

        /// <summary>
        /// Gets the forecasted distributed demand with shrinkage.
        /// </summary>
        /// <value>The forecasted distributed demand with shrinkage.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-05
        /// </remarks>
        public double ForecastedDistributedDemandWithShrinkage
        {
            get { return ForecastedDistributedDemand * (1d + Payload.Shrinkage.Value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is available.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is available; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-08
        /// </remarks>
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
                //if(thisSkillStaff.CalculatedResource == 0)
                //    continue;

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

            //if(thisSkillStaff.CalculatedResource == 0)
            //{
            //    _estimatedServiceLevel = new Percent(0);
            //    return;
            //}

            CalculateEstimatedServiceLevel();
        }

        /// <summary>
        /// Returns a SkillStaffPeriod containing the amount of calls that corresponds to the intersection of skillStaffPeriod and period.
        /// The length of the period will be the same as the supplied period.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-26
        /// </remarks>
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

		// Private Methods (4) 

        private static void AddToLists(double traffToDistribute, ISkillStaffPeriod currentPeriod, ISkillStaffPeriod ourPeriod)
        {
            ISkillStaffSegment newSegment = new SkillStaffSegment(traffToDistribute);
            ISkillStaffSegmentPeriod newSegmentPeriod = new SkillStaffSegmentPeriod(ourPeriod, currentPeriod, newSegment, currentPeriod.Period);
            ((SkillStaffPeriod)ourPeriod)._sortedSegmentCollection.Add(newSegmentPeriod.Period.StartDateTime, newSegmentPeriod);
            ((SkillStaffPeriod)currentPeriod)._segmentInThisCollection.Add(newSegmentPeriod);
        }

        //private TimeSpan adjustTotalLength(IList<ISkillStaffPeriod> sortedPeriods, int currentIndex)
        //{
        //    TimeSpan originalLength = TimeSpan.FromSeconds(Payload.ServiceAgreementData.ServiceLevel.Seconds);
        //    TimeSpan currentLength = TimeSpan.Zero;

        //    while (currentLength <= originalLength)
        //    {
        //        if (currentIndex == sortedPeriods.Count)
        //        {
        //            return currentLength;
        //        }
        //        ISkillStaffPeriod currentPeriod = sortedPeriods[currentIndex];
        //        currentLength = currentLength.Add(currentPeriod.Period.ElapsedTime());
        //        currentIndex++;
        //    }

        //    return originalLength;
        //}

        //private void createSkillStaffSegments(IList<ISkillStaffPeriod> sortedPeriods,
        //                                        int currentIndex)
        //{
        //    ISkillStaffPeriod ourPeriod = sortedPeriods[currentIndex];
        //    double timeLeftToDistribute = Payload.ForecastedIncomingDemand;
        //    double totalLength = adjustTotalLength(sortedPeriods, currentIndex).TotalSeconds;


        //    ISkillStaffPeriod currentPeriod = sortedPeriods[currentIndex];
        //    double periodLength = currentPeriod.Period.ElapsedTime().TotalSeconds;
        //    double distrPercent = periodLength / totalLength;
        //    double traffToDistribute = Payload.ForecastedIncomingDemand * distrPercent;

        //    if (traffToDistribute > timeLeftToDistribute)
        //        traffToDistribute = timeLeftToDistribute;
        //    AddToLists(traffToDistribute, currentPeriod, ourPeriod);

        //    currentIndex++;
        //    timeLeftToDistribute -= traffToDistribute;

        //    while ((timeLeftToDistribute > 0.00001) && (currentIndex < sortedPeriods.Count))
        //    {
        //        currentPeriod = sortedPeriods[currentIndex];
        //        periodLength = currentPeriod.Period.ElapsedTime().TotalSeconds;
        //        distrPercent = periodLength / totalLength;
        //        traffToDistribute = Payload.ForecastedIncomingDemand * distrPercent;
                
        //        if (traffToDistribute > timeLeftToDistribute)
        //            traffToDistribute = timeLeftToDistribute;
        //        AddToLists(traffToDistribute, currentPeriod, ourPeriod);

        //        currentIndex++;
        //        timeLeftToDistribute -= traffToDistribute;
        //    }   
        //}

        private void CreateSkillStaffSegments65(IList<ISkillStaffPeriod> sortedPeriods,
                                                int currentIndex)
        {
            ISkillStaffPeriod ourPeriod = sortedPeriods[currentIndex];
            ISkillStaffPeriod currentPeriod;
            TimeSpan sa = TimeSpan.FromSeconds(Payload.ServiceAgreementData.ServiceLevel.Seconds);

            while ((sa.TotalSeconds > 0.1) && (currentIndex < sortedPeriods.Count))
            {
                currentPeriod = sortedPeriods[currentIndex];
                TimeSpan currentLength = currentPeriod.Period.ElapsedTime();
                //double distrPercent = currentLength.TotalSeconds/Payload.ServiceAgreementData.ServiceLevel.Seconds;
                //AddToLists(Payload.ForecastedIncomingDemand * distrPercent, currentPeriod, ourPeriod);
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

		#endregion Methods 

        #region IAggregateSkillStaffPeriod members

        
        bool IAggregateSkillStaffPeriod.IsAggregate
        {
            get { return _isAggregate; }
            set { _isAggregate = value; }
        }


        double IAggregateSkillStaffPeriod.AggregatedFStaff
        {
            get
            {
                return _aggregatedFStaff;
            }
            set
            {
                _aggregatedFStaff = value;
            }
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
                _aggregatedEstimatedServiceLevel = CombineEstimatedServiceLevel(aggregateSkillStaffPeriod);
                _aggregatedForecastedIncomingDemand += aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand;
                _aggregatedMinMaxStaffAlarm = combineMinMaxStaffAlarm(aggregateSkillStaffPeriod);
                _aggregatedStaffingThreshold = combineStaffingTreshold(aggregateSkillStaffPeriod);
            }
            else
            {
                throw new InvalidCastException("Both instances must be aggregated");
            }
        }

        private StaffingThreshold combineStaffingTreshold(IAggregateSkillStaffPeriod aggregateSkillStaffPeriod)
        {
            int thisStatus = (int) _aggregatedStaffingThreshold;
            int otherStatus = (int) aggregateSkillStaffPeriod.AggregatedStaffingThreshold;
            return (StaffingThreshold) Math.Max(thisStatus, otherStatus);
        }

        private MinMaxStaffBroken combineMinMaxStaffAlarm(IAggregateSkillStaffPeriod aggregateSkillStaffPeriod)
        {
            if (_aggregatedMinMaxStaffAlarm == MinMaxStaffBroken.BothBroken)
                return _aggregatedMinMaxStaffAlarm;

            if (_aggregatedMinMaxStaffAlarm == MinMaxStaffBroken.Ok && aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm != MinMaxStaffBroken.Ok)
                return aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm;

            if(aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.BothBroken)
                return aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm;

            if (_aggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MinStaffBroken && aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MaxStaffBroken)
                return MinMaxStaffBroken.BothBroken;

            if (_aggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MaxStaffBroken && aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MinStaffBroken)
                return MinMaxStaffBroken.BothBroken;

            return _aggregatedMinMaxStaffAlarm;
        }

        private Percent CombineEstimatedServiceLevel(IAggregateSkillStaffPeriod aggregateSkillStaffPeriod)
        {
            double value = (_aggregatedForecastedIncomingDemand*_aggregatedEstimatedServiceLevel.Value) + 
                (aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand * aggregateSkillStaffPeriod.AggregatedEstimatedServiceLevel.Value);
            double demand = _aggregatedForecastedIncomingDemand +
                            aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand;
            if (demand == 0)
                return new Percent(1);
            return new Percent(value / demand);
        }

        /// <summary>
        /// Gets or sets the calculated resource.
        /// </summary>
        /// <value>The calculated resource.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-13
        /// </remarks>
        double IAggregateSkillStaffPeriod.AggregatedCalculatedResource
        {
            get { return _aggregatedCalculatedResources; }
            set { _aggregatedCalculatedResources = value; }
        }

        public Percent AggregatedEstimatedServiceLevel
        {
            get { return _aggregatedEstimatedServiceLevel; }
            set { _aggregatedEstimatedServiceLevel = value; }
        }

        public double AggregatedForecastedIncomingDemand
        {
            get { return _aggregatedForecastedIncomingDemand; }
            set { _aggregatedForecastedIncomingDemand = value; }
        }

        public MinMaxStaffBroken AggregatedMinMaxStaffAlarm
        {
            get { return _aggregatedMinMaxStaffAlarm; }
            set { _aggregatedMinMaxStaffAlarm = value; }
        }

        public StaffingThreshold AggregatedStaffingThreshold
        {
            get { return _aggregatedStaffingThreshold; }
            set { _aggregatedStaffingThreshold = value; }
        }

        public IPeriodDistribution PeriodDistribution
        {
            get
            {
                return _periodDistribution;
            }
        }




        #endregion
    }
}
