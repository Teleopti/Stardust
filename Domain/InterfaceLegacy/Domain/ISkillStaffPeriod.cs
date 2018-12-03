using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
    /// Periodized staffing information for skill day
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-09-18
    /// </remarks>
    public interface ISkillStaffPeriod : ILayer<ISkillStaff>, ICloneableEntity<ILayer<ISkillStaff>>, IResourceCalculationPeriod, IValidatePeriod
	{
        /// <summary>
        /// Gets the segment collection.
        /// </summary>
        /// <value>The segment collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-09
        /// </remarks>
        IList<ISkillStaffSegmentPeriod> SortedSegmentCollection { get; }

		/// <summary>
		/// Gets the segment in this collection.
		/// </summary>
		/// <value>The segment in this collection.</value>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-04-18
		/// </remarks>
		IList<ISkillStaffSegmentPeriod> SegmentInThisCollection { get; }

        /// <summary>
        /// Gets the relative difference for display only.
        /// </summary>
        /// <value>The relative difference for display only.</value>
        double RelativeDifferenceForDisplayOnly { get; }


        /// <summary>
        /// Gets or sets the relative boosted difference for display only.
        /// </summary>
        /// <value>
        /// The relative boosted difference for display only.
        /// </value>
        double RelativeBoostedDifferenceForDisplayOnly { get; set; }

        /// <summary>
        /// Gets the diff percent.
        /// </summary>
        /// <value>The diff percent.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-09
        /// </remarks>
        double RelativeDifferenceIncoming { get; }
		
        /// <summary>
        /// Gets the forecasted distributed demand with shrinkage.
        /// </summary>
        /// <value>The forecasted distributed demand with shrinkage.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-05
        /// </remarks>
        double ForecastedDistributedDemandWithShrinkage { get; }

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
        bool IsAvailable { get; set; }

        /// <summary>
        /// Gets or sets the statistic task. Needs to be set from outside this class before use!
        /// </summary>
        /// <value>The statistic task.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-19
        /// </remarks>
        IStatisticTask StatisticTask { get; set; }

        /// <summary>
        /// Gets or sets the active agent count. Needs to be set from outside this class before use!
        /// </summary>
        /// <value>The active agent count.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-19
        /// </remarks>
        IActiveAgentCount ActiveAgentCount { get; set; }
		
        /// <summary>
        /// Picks the resource65.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-04
        /// </remarks>
        void PickResources65();

        /// <summary>
        /// Gets the calculated staff time during this period.
        /// </summary>
        /// <value>The calculated staff minutes.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-24
        /// </remarks>
        TimeSpan ForecastedIncomingDemand();

        /// <summary>
        /// Calculateds the staff time with shrinkage.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-08
        /// </remarks>
        TimeSpan ForecastedIncomingDemandWithShrinkage();

        /// <summary>
        /// Calculates the staff and distributes the traff down to segmentPeriods.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-04-18
        /// </remarks>
        void CalculateStaff(IList<ISkillStaffPeriod> periods);

        /// <summary>
        /// Calculates the staff.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-04-18
        /// </remarks>
        void CalculateStaff();

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
        ISkillStaffPeriod IntersectingResult(DateTimePeriod period);

        /// <summary>
        /// Resets this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        void Reset();

 /// <summary>
        /// Gets the scheduled against incoming forecast65.
        /// </summary>
        /// <value>The scheduled against incoming forecast65.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-04
        /// </remarks>
        double ScheduledAgentsIncoming { get; }

        /// <summary>
        /// Gets the incoming difference.
        /// </summary>
        /// <value>The incoming difference.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-11-24
        /// </remarks>
        double IncomingDifference { get; }

        /// <summary>
        /// Gets the staffing calculator service.
        /// </summary>
        /// <value>The staffing calculator service.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-26
        /// </remarks>
		IStaffingCalculatorServiceFacade StaffingCalculatorService { get; }

        /// <summary>
        /// Gets the total booked resource against incomming forecasted demand.
        /// </summary>
        /// <value>The booked resource65.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-09
        /// </remarks>
        double BookedResource65 { get; }

        /// <summary>
        /// Calculates the TRAFF calculated resources in hours.
        /// </summary>
        /// <returns></returns>
        double ScheduledHours();

        /// <summary>
        /// Calculates the TRAFF of FStaff as time.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-10
        /// </remarks>
        TimeSpan FStaffTime();

        /// <summary>
        /// Calculates the TRAFF and Fstaff as hours.
        /// </summary>
        /// <returns></returns>
        double FStaffHours();

        /// <summary>
        /// Absolutes difference between scheduled heads and min/max heads.
        /// </summary>
        /// <param name="shiftValueMode">if set to <c>true</c> [shift value mode].</param>
        /// <returns></returns>
        /// <remarks>
        /// shiftValueMode = true, is only for ShiftValueCalculator
        /// Created by: micke
        /// Created date: 2009-02-23
        /// </remarks>
        double AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(bool shiftValueMode);

        /// <summary>
        /// Gets the estimated service level.
        /// </summary>
        /// <value>The estimated service level.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-02-27
        /// </remarks>
        Percent EstimatedServiceLevel { get; }

        /// <summary>
        /// Gets the actual service level.
        /// </summary>
        /// <value>The actual service level.</value>
        /// <remarks>
        /// Created by: marias
        /// Created date: 2011-05-18
        /// </remarks>
        Percent ActualServiceLevel { get; }

        /// <summary>
        /// Gets or sets the period distribution.
        /// </summary>
        /// <value>The period distribution.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-05-05    
        /// /// </remarks>
        IPeriodDistribution PeriodDistribution { get; }


        /// <summary>
        /// Absolute difference boosted by not fulfilled min staff.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-05-22
        /// </remarks>
        double AbsoluteDifferenceMinStaffBoosted();

        /// <summary>
        /// Absolute difference boosted by the exceeded maximum staff.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-05-22
        /// </remarks>
        double AbsoluteDifferenceMaxStaffBoosted();

        /// <summary>
        /// Absolute difference between the scheduled and the forecasted value
        /// with considering the min-max personnel limit.
        /// </summary>
        /// <value>The absolute difference.</value>
        double AbsoluteDifferenceBoosted();

        /// <summary>
        /// Relative difference boosted by not fulfilled min staff.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2009-11-30
        /// </remarks>
        double RelativeDifferenceMinStaffBoosted();

        /// <summary>
        /// Relative difference boosted by the exceeded maximum staff.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2009-11-30
        /// </remarks>
        double RelativeDifferenceMaxStaffBoosted();

        /// <summary>
        /// Relative difference between the scheduled and the forecasted value
        /// with considering the min-max personnel limit.
        /// </summary>
        /// <value>The absolute difference.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2009-11-30
        /// </remarks>
        double RelativeDifferenceBoosted();

        /// <summary>
        /// Splits the period into one or several ISkillStaffPeriodView(s).
        /// The specified period length must be shorter than on this and the split must be even.
        /// For example can not a 15 minutes period be split on 10. But 5.
        /// </summary>
        /// <param name="periodLength">Length of the period in the new ISkillStaffPeriodView(s).</param>
        /// <returns></returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-07-06    
        /// /// </remarks>
        IList<ISkillStaffPeriodView> Split(TimeSpan periodLength);

	    /// <summary>
	    /// Set the skillday the period belongs to
	    /// </summary>
	    /// <param name="skillDay"></param>
	    void SetSkillDay(ISkillDay skillDay);

		/// <summary>
		/// The skillday the period belongs to
		/// </summary>
		ISkillDay SkillDay { get; }

	    Percent EstimatedServiceLevelShrinkage { get; }

		void CalculateEstimatedServiceLevel();

		bool HasIntraIntervalIssue { get; set; }
		double IntraIntervalValue { get; set; }
		IList<int> IntraIntervalSamples { get; set; }
		double AbsoluteDifference { get; }
		double CalculatedResource { get; }
		
	}
}