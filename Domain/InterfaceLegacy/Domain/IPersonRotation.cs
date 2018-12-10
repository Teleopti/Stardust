using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Base class for persons using a rotation restriction
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-06-30
    /// </remarks>
    public interface IPersonRotation : IAggregateRoot 
    {
        /// <summary>
        /// Gets the start row.
        /// </summary>
        /// <value>The start row.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-27
        /// </remarks>
        int StartDay { get; set; }

        /// <summary>
        /// Gets the start date.
        /// </summary>
        /// <value>The start date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-27
        /// </remarks>
        DateOnly StartDate { get; set; }

        /// <summary>
        /// Gets the rotation.
        /// </summary>
        /// <value>The rotation.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-27
        /// </remarks>
        IRotation Rotation { get; set; }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-27
        /// </remarks>
        IPerson Person { get; }

        /// <summary>
        /// Gets the start date as UTC.
        /// </summary>
        /// <value>The start date as UTC.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-01-28
        /// </remarks>
        DateTime StartDateAsUtc { get; }

        /// <summary>
        /// Gets the rotation day.
        /// </summary>
        /// <param name="currentDate">The current date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-27
        /// </remarks>
        IRotationDay GetRotationDay(DateOnly currentDate);
    }
}