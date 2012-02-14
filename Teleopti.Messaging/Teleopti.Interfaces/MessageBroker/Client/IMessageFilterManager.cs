using System.Diagnostics.CodeAnalysis;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Client
{

    /// <summary>
    /// The Message Filter Manager
    /// </summary>
    public interface IMessageFilterManager
    {
        /// <summary>
        /// Checks the filters.
        /// </summary>
        /// <param name="eventMessageArgs">The event message args.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void CheckFilters(EventMessageArgs eventMessageArgs);

        /// <summary>
        /// Checks the filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="eventMessage">The event message.</param>
        IEventMessage CheckFilter(IEventFilter filter, IEventMessage eventMessage);

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="eventMessage">The event message.</param>
        IEventMessage CheckDates(IEventFilter filter, IEventMessage eventMessage);

        /// <summary>
        /// Gets or sets the message registration.
        /// </summary>
        /// <value>The message registration.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 30/06/2009
        /// </remarks>
        IMessageRegistrationManager MessageRegistration { get; set; }

    }

}