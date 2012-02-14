using System.Runtime.Serialization;

namespace Teleopti.Interfaces.MessageBroker.Client
{

    /// <summary>
    /// Number of concurrent users.
    /// </summary>
    public interface IConcurrentUsers : ISerializable
    {
        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        /// <value>The ip address.</value>
        string IPAddress { get; set; }
        /// <summary>
        /// Gets or sets the number of concurrent users.
        /// </summary>
        /// <value>The number of concurrent users.</value>
        int NumberOfConcurrentUsers { get; set; }
    }

}