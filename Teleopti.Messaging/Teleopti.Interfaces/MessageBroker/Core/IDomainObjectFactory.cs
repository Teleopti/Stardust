using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// Event Domain Object Factory.
    /// </summary>
    public interface IDomainObjectFactory
    {
        /// <summary>
        /// Create new Event Message.
        /// </summary>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="packageSize">Size of the package.</param>
        /// <param name="isHeartbeat">if set to <c>true</c> [is heartbeat].</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Int32 userId, Int32 processId, Guid moduleId, Int32 packageSize, bool isHeartbeat, Guid parentObjectId, string parentObjectType, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, string userName);

        /// <summary>
        /// Create new Event Message.
        /// </summary>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="packageSize">Size of the package.</param>
        /// <param name="isHeartbeat">if set to <c>true</c> [is heartbeat].</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <param name="domainObject">The domain object.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        IEventMessage CreateEventMessage(DateTime eventStartDate, DateTime eventEndDate, Int32 userId, Int32 processId, Guid moduleId, Int32 packageSize, bool isHeartbeat, Guid parentObjectId, string parentObjectType, Guid domainObjectId, string domainObjectType, DomainUpdateType updateType, byte[] domainObject, string userName);

        /// <summary>
        /// Create a subscription to Events for the UserId and ProcessId in question.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="processId"></param>
        /// <param name="userName"></param>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        IEventSubscriber CreateSubscription(Int32 userId, Int32 processId, string userName, string ipAddress, int port);

        /// <summary>
        /// Create a Receipt to signal messaging is still working.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="processId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        IEventReceipt CreateReceipt(Guid eventId, Int32 processId, string userName);

        /// <summary>
        /// Creates a new Event Log Entry.
        /// </summary>
        /// <param name="processId">The process id.</param>
        /// <param name="description">The description.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="stackTrace">The stack trace.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        ILogEntry CreateEventLogEntry(Int32 processId, string description, string exception, string message, string stackTrace, string userName);

        /// <summary>
        /// Create a Heartbeat to signal messaging is still working.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="processId">The process id.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        IEventHeartbeat CreateHeartbeat(Guid subscriberId, Int32 processId, string userName);

        /// <summary>
        /// Create a new user. 
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        IEventUser CreateUser(string domain, string userName);

        /// <summary>
        /// Call this method to create a filter for which event messages should be propagated.
        /// </summary>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        IEventFilter CreateFilter(Guid subscriberId, Guid parentObjectId, string parentObjectType, Guid domainObjectId, string domainObjectType, DateTime startDate, DateTime endDate, string userName);
    }
}