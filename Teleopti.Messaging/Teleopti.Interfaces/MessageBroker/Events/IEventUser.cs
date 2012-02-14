using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Teleopti.Interfaces.MessageBroker.Events
{
    /// <summary>
    /// Event User, who is publishing?
    /// </summary>
    public interface IEventUser : ISerializable, IXmlSerializable
    {
        /// <summary>
        /// The user id who is subscribing to the events.
        /// This will be the hibernate user id.
        /// </summary>
        int UserId { get; set; }

        /// <summary>
        /// The domain to which the user belongs.
        /// </summary>
        string Domain { get; set; }

        /// <summary>
        /// The name of the user. 
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// The user or program that created/changed the event.
        /// </summary>
        string ChangedBy { get; set; }

        /// <summary>
        /// The Date Time for which the event was created/changed.
        /// </summary>
        DateTime ChangedDateTime { get; set; }

        /// <summary>
        /// Override of the ToString method
        /// </summary>
        /// <returns></returns>
        String ToString();
    }
}