using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// A part of a request
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-10-10
    /// </remarks>
    public interface IRequest : ICloneableEntity<IRequest>, IAggregateEntity
    {
        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        DateTimePeriod Period { get; }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        IPerson Person { get; }

        /// <summary>
        /// Description for type of request
        /// </summary>
        string RequestTypeDescription { get; set; }

		/// <summary>
		/// Type of request
		/// </summary>
		RequestType RequestType { get; }

        /// <summary>
        /// Description for the payload
        /// </summary>
        Description RequestPayloadDescription { get; }

        /// <summary>
        /// Denies this instance.
        /// </summary>
        /// <param name="denyPerson">The deny person.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        void Deny(IPerson denyPerson);

        /// <summary>
        /// Refers this instance.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-10-27
        /// </remarks>
        string GetDetails(CultureInfo cultureInfo);

        /// <summary>
        /// Gets the text for notification.
        /// </summary>
        /// <value>The text for notification.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-10-28
        /// </remarks>
        string TextForNotification { get; set; }

        /// <summary>
        /// Gets a value indicating whether [should notify with message].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [should notify with message]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-10-28
        /// </remarks>
        bool ShouldNotifyWithMessage { get; }

        /// <summary>
        /// Gets the receivers for notification.
        /// </summary>
        /// <value>The receivers for notification.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-10-28
        /// </remarks>
        IList<IPerson> ReceiversForNotification { get; }

        ///<summary>
        ///</summary>
        IPerson PersonTo { get; }

        ///<summary>
        ///</summary>
        IPerson PersonFrom { get; }
    }
}