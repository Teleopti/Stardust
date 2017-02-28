using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for TemplateReference
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-10-22
    /// </remarks>
    public interface ITemplateReference
    {
        /// <summary>
        /// Gets or sets the name of the template.
        /// </summary>
        /// <value>The name of the template.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09
        /// </remarks>
        string TemplateName { get; set; }

        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        /// <value>The template id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09
        /// </remarks>
        Guid TemplateId { get; set; }

        /// <summary>
        /// Gets the day of week.
        /// </summary>
        /// <value>The day of week.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-12
        /// </remarks>
        DayOfWeek? DayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the version number.
        /// </summary>
        /// <value>The version number.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-16
        /// </remarks>
        int VersionNumber { get; set; }

    	/// <summary>
		/// Gets or sets the updated date.
    	/// </summary>
		/// <value>The updated date.</value>
    	DateTime UpdatedDate { get; set; }

    	/// <summary>
        /// Gets the name of the template.
        /// </summary>
        /// <param name="weekday">The weekday.</param>
        /// <param name="name">The name.</param>
        /// <param name="old">if set to <c>true</c> [old].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-12
        /// </remarks>
        string DisplayName(DayOfWeek? weekday, string name, bool old);
    }
}