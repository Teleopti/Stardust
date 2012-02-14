using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Common interface for MultiplcatorDefinitions
    ///</summary>
    public interface IMultiplicatorDefinition : IAggregateEntity, ICloneableEntity<IMultiplicatorDefinition>
    {
        /// <summary>
        /// Gets the multiplicator.
        /// </summary>
        /// <value>The multiplicator.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-08
        /// </remarks>
        IMultiplicator Multiplicator { get; set; }

        /// <summary>
        /// Gets the index of the order.
        /// </summary>
        /// <value>The index of the order.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-09
        /// </remarks>
        int OrderIndex { get; }

        /// <summary>
        /// Gets the layers for period.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date. (Including)</param>
        /// <param name="timeZoneInfo">The time zone info.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-10
        /// </remarks>
        IList<IMultiplicatorLayer> GetLayersForPeriod(DateOnly startDate, DateOnly endDate,
                                                      ICccTimeZoneInfo timeZoneInfo);
    }
}
