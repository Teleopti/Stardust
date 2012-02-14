using System.Diagnostics.CodeAnalysis;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.Server
{
    /// <summary>
    /// The Publisher Factory
    /// </summary>
    public class PublisherFactory
    {
        /// <summary>
        /// Creates the publisher.
        /// </summary>
        /// <param name="serverThrottle">The server throttle.</param>
        /// <param name="messagingProtocol">The messaging protocol.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public IPublisher CreatePublisher(int serverThrottle, MessagingProtocol messagingProtocol)
        {
            return new Publisher(serverThrottle, messagingProtocol);
        }
        
    }
}