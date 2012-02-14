using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Common interface for day level templates
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-06-10
    /// </remarks>
    public interface IForecastDayTemplate : IAggregateEntity
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-12
        /// </remarks>
        string Name { get; set; }

        /// <summary>
        /// Gets the day of week.
        /// </summary>
        /// <value>The day of week.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-12
        /// </remarks>
        DayOfWeek? DayOfWeek { get; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-16
        /// </remarks>
        int VersionNumber
        { get; }

		///<summary>
		/// Gets the updated date.
		///</summary>
		/// <value>The updated date.</value>
		DateTime UpdatedDate { get; }

        /// <summary>
        /// Increases the version number.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-16
        /// </remarks>
        void IncreaseVersionNumber();

		///<summary>
		/// Refresh the updated date
		///</summary>
		void RefreshUpdatedDate();
    }
}