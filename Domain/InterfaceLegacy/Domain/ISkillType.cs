namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Abstract base for skill types
    /// </summary>
    public interface ISkillType : IAggregateRoot
    {
        /// <summary>
        /// Gets the default resolution.
        /// </summary>
        /// <value>The default resolution.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-25
        /// </remarks>
        int DefaultResolution { get; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-25
        /// </remarks>
        Description Description { get; set; }

        /// <summary>
        /// Gets or sets the forecast source.
        /// </summary>
        /// <value>The forecast source.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-08-26
        /// </remarks>
        ForecastSource ForecastSource { get; set; }

        /// <summary>
        /// Gets the task time distribution service.
        /// </summary>
        /// <value>The task time distribution service.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-27
        /// </remarks>
        ITaskTimeDistributionService TaskTimeDistributionService { get; }

        /// <summary>
        /// Gets a value indicating whether timespan values should be
        /// displayed as minutes in chart.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [display time span as minutes]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-31
        /// </remarks>
        bool DisplayTimeSpanAsMinutes { get; }

        /// <summary>
        /// Gets the staffing calculator service.
        /// </summary>
        /// <value>The staffing calculator service.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-26
        /// </remarks>
		IStaffingCalculatorServiceFacade StaffingCalculatorService { get; set; }
    }
}
