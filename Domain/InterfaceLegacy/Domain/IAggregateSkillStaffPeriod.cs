namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for SkillStaffPeriods that are constructed as aggregate of other SkillStaffPeriods
    /// </summary>
    public interface IAggregateSkillStaffPeriod
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is aggregate.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is aggregate; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-12
        /// </remarks>
        bool IsAggregate { get; set; }

        /// <summary>
        /// Gets or sets the aggregated FStaff in TRAFF.
        /// </summary>
        /// <value>The FStaff in TRAFF.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-12
        /// </remarks>
        double AggregatedFStaff { get; set; }

        /// <summary>
        /// Gets the aggregated calculated logged on.
        /// </summary>
        /// <value>The aggregated calculated logged on.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-13
        /// </remarks>
        double AggregatedCalculatedLoggedOn { get; }

        /// <summary>
        /// Combines the aggregated skill staff period.
        /// </summary>
        /// <param name="aggregateSkillStaffPeriod">The aggregate skill staff period.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-13
        /// </remarks>
        void CombineAggregatedSkillStaffPeriod(IAggregateSkillStaffPeriod aggregateSkillStaffPeriod);

        /// <summary>
        /// Gets or sets the calculated resource.
        /// </summary>
        /// <value>The calculated resource.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-13
        /// </remarks>
        double AggregatedCalculatedResource { get; set; }

        /// <summary>
        /// Gets or sets the aggregated estimated service level.
        /// </summary>
        /// <value>The aggregated estimated service level.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-03-05
        /// </remarks>
        Percent AggregatedEstimatedServiceLevel { get; set; }

		Percent AggregatedEstimatedServiceLevelShrinkage { get; set; }

        /// <summary>
        /// Gets or sets the aggregated forecasted incoming demand.
        /// </summary>
        /// <value>The aggregated forecasted incoming demand.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-03-05
        /// </remarks>
        double AggregatedForecastedIncomingDemand { get; set; }

        /// <summary>
        /// Gets or sets the aggregated min max staff alarm.
        /// </summary>
        /// <value>
        /// The aggregated min max staff alarm.
        /// </value>
        MinMaxStaffBroken AggregatedMinMaxStaffAlarm { get; set; }

        /// <summary>
        /// Gets or sets the aggregated staffing threshold.
        /// </summary>
        /// <value>
        /// The aggregated staffing threshold.
        /// </value>
        StaffingThreshold AggregatedStaffingThreshold { get; set; }
    }
}
