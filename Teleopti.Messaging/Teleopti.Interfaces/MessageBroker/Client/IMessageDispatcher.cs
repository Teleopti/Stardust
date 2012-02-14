using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Client
{

    ///<summary>
    /// The Message Dispatcher.
    ///</summary>
    public interface IMessageDispatcher
    {

        /// <summary>
        /// Sends the event message.
        /// </summary>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        void SendEventMessage(DateTime eventStartDate,
                                      DateTime eventEndDate,
                                      Guid moduleId,
                                      Guid parentObjectId,
                                      Type parentObjectType,
                                      Guid domainObjectId,
                                      Type domainObjectType,
                                      DomainUpdateType updateType);

        /// <summary>
        /// Sends the event message.
        /// </summary>
        /// <param name="eventStartDate">The event start date.</param>
        /// <param name="eventEndDate">The event end date.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <param name="domainObject">The domain object.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        void SendEventMessage(DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject);

        /// <summary>
        /// Sends the event messages.
        /// </summary>
        /// <param name="eventMessages">The event messages.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        void SendEventMessages(IEventMessage[] eventMessages);
        /// <summary>
        /// Sends the event message.
        /// </summary>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        void SendEventMessage(Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType);
        /// <summary>
        /// Sends the event message.
        /// </summary>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        void SendEventMessage(Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);
        /// <summary>
        /// Sends the event message.
        /// </summary>
        /// <param name="moduleId">The module id.</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        void SendEventMessage(Guid moduleId, Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType);
        /// <summary>
        /// Sends the event message.
        /// </summary>
        /// <param name="moduleId">The module id.</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="domainObjectId">The domain object id.</param>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <param name="domainObject">The domain object.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/04/2009
        /// </remarks>
        void SendEventMessage(Guid moduleId, Guid parentObjectId, Type parentObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject);
    }

}