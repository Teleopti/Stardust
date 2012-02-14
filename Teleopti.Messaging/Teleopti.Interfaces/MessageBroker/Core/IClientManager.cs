namespace Teleopti.Interfaces.MessageBroker.Core
{
    ///<summary>
    /// The client manager.
    ///</summary>
    public interface IClientManager
    {
        /// <summary>
        /// Adds the listner.
        /// </summary>
        int AddListener(string address);

        /// <summary>
        /// Sends the package.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="value">The value.</param>
        void QueueMessage(string address, int port, byte[] value);

        /// <summary>
        /// Removes the listener.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 08/05/2010
        /// </remarks>
        void RemoveListener(int port);


        /// <summary>
        /// Gets or sets the broker service.
        /// </summary>
        /// <value>The broker service.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/05/2010
        /// </remarks>
        IBrokerService BrokerService { get; set; }

        /// <summary>
        /// Gets or sets the start port.
        /// </summary>
        /// <value>The start port.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/05/2010
        /// </remarks>
        int StartPort { get; set; }

        /// <summary>
        /// Gets or sets the client throttle.
        /// </summary>
        /// <value>The client throttle.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 12/05/2010
        /// </remarks>
        int ClientThrottle { get; set; }

    }
}
