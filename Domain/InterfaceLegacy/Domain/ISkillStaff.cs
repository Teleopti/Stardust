namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Staff information for interval on skill day
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-09-18
    /// </remarks>
    public interface ISkillStaff : IOriginator<ISkillStaff>
    {
        /// <summary>
        /// Resets this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-18
        /// </remarks>
        void Reset();

        /// <summary>
        /// Gets a value indicating whether this instance is calculated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is calculated; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-08
        /// </remarks>
        bool IsCalculated { get; }

        /// <summary>
        /// Gets or sets the calculated logged on.
        /// </summary>
        /// <value>The calculated logged on.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-09
        /// </remarks>
        double CalculatedLoggedOn { get; set; }

        /// <summary>
        /// Gets the calculated occupancy.
        /// </summary>
        /// <value>The calculated occupancy.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-09
        /// </remarks>
        double CalculatedOccupancy { get; }

        /// <summary>
        /// Gets the calculated occupancy percent.
        /// </summary>
        /// <value>The calculated occupancy percent.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-09-04
        /// </remarks>
        Percent CalculatedOccupancyPercent { get; }

        /// <summary>
        /// Gets or sets the calculated resource.
        /// </summary>
        /// <value>The calculated resource.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-09
        /// </remarks>
        double CalculatedResource { get; }

        /// <summary>
        /// Gets the calculated traffic intensity.
        /// </summary>
        /// <value>The calculated traffic intensity.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-09
        /// </remarks>
        double ForecastedIncomingDemand { get; }

		double ForecastedIncomingDemandWithoutShrinkage { get; }

        /// <summary>
        /// Gets the booked against incoming demand65.
        /// </summary>
        /// <value>The booked against incoming demand65.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-05
        /// </remarks>
        double BookedAgainstIncomingDemand65 { get; }

        /// <summary>
        /// Gets the multi skill min occupancy.
        /// </summary>
        /// <value>The multi skill min occupancy.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-09
        /// </remarks>
        Percent? MultiskillMinOccupancy { get; set; }

        /// <summary>
        /// Gets or sets the service agreement data.
        /// </summary>
        /// <value>The service agreement data.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-08
        /// </remarks>
        ServiceAgreement ServiceAgreementData { get; set; }

        /// <summary>
        /// Gets or sets the task data.
        /// </summary>
        /// <value>The task data.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-08
        /// </remarks>
        ITask TaskData { get; set; }

        /// <summary>
        /// Gets or sets the shrinkage.
        /// </summary>
        /// <value>The shrinkage.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-12
        /// </remarks>
        Percent Shrinkage { get; set; }

        /// <summary>
        /// Gets or sets the efficiency.
        /// </summary>
        /// <value>The shrinkage.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2010-07-01
        /// </remarks>
        Percent Efficiency { get; set; }

        /// <summary>
        /// Gets or sets the skill person data.
        /// </summary>
        /// <value>The skill person data.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-13
        /// </remarks>
        SkillPersonData SkillPersonData { get; set; }

        /// <summary>
        /// Gets the calculated traffic intensity with shrinkage.
        /// </summary>
        /// <value>The calculated traffic intensity with shrinkage.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-12
        /// </remarks>
        double CalculatedTrafficIntensityWithShrinkage { get; }

        /// <summary>
        /// Gets or sets a value indicating whether [use shrinkage].
        /// </summary>
        /// <value><c>true</c> if [use shrinkage]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-02-24
        /// </remarks>
        bool UseShrinkage { get; set; }

		/// <summary>
		/// Gets or manually set agents
		/// </summary>
		/// <value>The manully input agents.</value>
    	double? ManualAgents { get; set; }

		/// <summary>
		/// Gets or sets the calculated used seats.
		/// </summary>
		/// <value>The calculated used seats.</value>
		double CalculatedUsedSeats { get; set; }

		/// <summary>
		/// Gets or sets the maximum seats.
		/// </summary>
		/// <value>The max seats.</value>
		int MaxSeats { get; set; }

        /// <summary>
        /// Gets or sets the non blend demand.
        /// </summary>
        /// <value>
        /// The none blend demand.
        /// </value>
        int? NoneBlendDemand { get; set; }
    }
}