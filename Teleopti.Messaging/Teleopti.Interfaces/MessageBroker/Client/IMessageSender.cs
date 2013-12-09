using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Client
{
    /// <summary>
    /// The MessageSender send messages only from the same machine as the Broker Service.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 11/06/2010
    /// </remarks>
    public interface IMessageSender
    {
        /// <summary>
        /// Gets a value indicating whether this instance is alive.
        /// </summary>
        /// <value><c>true</c> if this instance is alive; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 10/06/2010
        /// </remarks>
        bool IsAlive { get; }

    	/// <summary>
    	/// Sends the RTA data.
    	/// </summary>
    	/// <param name="personId">The person id.</param>
    	/// <param name="businessUnitId">The business unit id.</param>
    	/// <param name="actualAgentState"> </param>
    	/// <remarks>
    	/// Created by: ankarlp
    	/// Created date: 10/06/2010
    	/// </remarks>
    	void QueueRtaNotification(Guid personId, Guid businessUnitId, IActualAgentState actualAgentState);

    	/// <summary>
    	/// Sends schedule data.
    	/// </summary>
    	/// <param name="floor">The floor.</param>
    	/// <param name="ceiling">The ceiling.</param>
    	/// <param name="moduleId">The module id.</param>
    	/// <param name="domainObjectId">The domain object id.</param>
    	/// <param name="domainInterfaceType">Type of the domain interface.</param>
    	/// <param name="updateType">Type of the update.</param>
    	/// <param name="dataSource">Data source.</param>
    	/// <param name="businessUnitId">Business unit id.</param>
    	/// <remarks>
    	/// Created by: ankarlp
    	/// Created date: 12/06/2010
    	/// </remarks>
    	void SendData(DateTime floor, DateTime ceiling, Guid moduleId, Guid domainObjectId, Type domainInterfaceType, DomainUpdateType updateType, string dataSource, Guid businessUnitId);

        /// <summary>
        /// Instantiates the broker service.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 09/06/2010
        /// </remarks>
        void InstantiateBrokerService();
    }
}