using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Information when a person is write protected
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-03-03
    /// </remarks>
    public interface IPersonWriteProtectionInfo : IAggregateRoot
    {
        /// <summary>
        /// Gets the person this object belongs to.
        /// </summary>
        /// <value>The belongs to.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-03-03
        /// </remarks>
        IPerson BelongsTo { get; }

        /// <summary>
        /// Gets or sets the person write protected date.
        /// </summary>
        /// <value>The person write protected date.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-03-03
        /// </remarks>
        DateOnly? PersonWriteProtectedDate { get; set; }

        /// <summary>
        /// Combination of person's write protected date and its team's protected date
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-03-03
        /// </remarks>
        DateOnly? WriteProtectedUntil();

        /// <summary>
        /// Latest updated by
        /// </summary>
        /// <value>The updated by.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-03-03
        /// </remarks>
        IPerson UpdatedBy { get; }

        /// <summary>
        /// Latest updated date.
        /// </summary>
        /// <value>The updated on.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-03-03
        /// </remarks>
        DateTime? UpdatedOn { get; }

        /// <summary>
        /// Determines whether [is write protected] [the specified date only].
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <returns>
        /// 	<c>true</c> if [is write protected] [the specified date only]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-03-03
        /// </remarks>
        bool IsWriteProtected(DateOnly dateOnly);
    }
}
