using System;

namespace Teleopti.Interfaces.MessageBroker.Events
{
	/// <summary>
	/// The client MessageBroker instance will enable
	/// ASM and RAPTOR clients to publish
	/// to messages.
	/// </summary>
	public interface IMessageBrokerSender
	{
		/// <summary>
		/// Sends the event message.
		/// </summary>
		/// <param name="eventStartDate">The event start date.</param>
		/// <param name="eventEndDate">The event end date.</param>
		/// <param name="moduleId">The module id.</param>
		/// <param name="referenceObjectId">The reference object id.</param>
		/// <param name="referenceObjectType">Type of the reference object.</param>
		/// <param name="domainObjectId">The domain object id.</param>
		/// <param name="domainObjectType">Type of the domain object.</param>
		/// <param name="updateType">Type of the update.</param>
		/// <param name="domainObject">The domain object.</param>
		/// <param name="businessUnitId"></param>
		/// <param name="dataSource"></param>
		/// <remarks>
		/// Created by: ankarlp
		/// Created date: 16/04/2009
		/// </remarks>
		void SendEventMessage(string dataSource, Guid businessUnitId, DateTime eventStartDate,
		                      DateTime eventEndDate,
		                      Guid moduleId,
		                      Guid referenceObjectId,
		                      Type referenceObjectType,
		                      Guid domainObjectId,
		                      Type domainObjectType,
		                      DomainUpdateType updateType,
		                      byte[] domainObject);

		/// <summary>
		/// Sends the event message.
		/// </summary>
		/// <param name="eventStartDate">The event start date.</param>
		/// <param name="eventEndDate">The event end date.</param>
		/// <param name="moduleId">The module id.</param>
		/// <param name="domainObjectId">The domain object id.</param>
		/// <param name="domainObjectType">Type of the domain object.</param>
		/// <param name="updateType">Type of the update.</param>
		/// <param name="domainObject">The domain object.</param>
		/// <param name="businessUnitId"></param>
		/// <param name="dataSource"></param>
		void SendEventMessage(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject);

		/// <summary>
		/// Sends the event messages.
		/// </summary>
		/// <param name="eventMessages">The event messages.</param>
		/// <param name="businessUnitId"></param>
		/// <param name="dataSource"></param>
		void SendEventMessages(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages);

		void SendNotification(Notification notification);
	}
}