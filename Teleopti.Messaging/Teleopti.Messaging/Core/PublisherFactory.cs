using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.Core
{
    /// <summary>
    /// The Publisher Factory
    /// </summary>
    public class PublisherFactory
    {

        /// <summary>
        /// Creates the multicast publisher.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <param name="timeToLive">The time to live.</param>
        /// <returns></returns>
        public IPublisher CreateMulticastPublisher(string ipAddress, int port, int timeToLive)
        {
            return new MulticastPublisher(ipAddress, port, 1, timeToLive);
        }

        /// <summary>
        /// Creates the TCP ip publisher.
        /// </summary>
        /// <param name="numberOfThreads">The number of threads.</param>
        /// <returns></returns>
        public IPublisher CreateTcpIpPublisher(int numberOfThreads)
        {
            return new TcpIpPublisher(numberOfThreads);
        }

    }
}
