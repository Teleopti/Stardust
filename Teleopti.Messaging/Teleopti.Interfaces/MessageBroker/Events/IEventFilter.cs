using System;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.MessageBroker.Events
{
    /// <summary>
    /// Filter class is used to filter out unneccessary 
    /// incomming messages to the message broker instance.
    /// </summary>
    public interface IEventFilter : ISerializable
    {
        /// <summary>
        /// The filter Id for a certain filter,
        /// this is a counter of filters persisted.
        /// </summary>
        Guid FilterId { get; set; }

        /// <summary>
        /// The subscriber id, each subscription has 
        /// a unique id. one user can have several subscriptions.
        /// </summary>
        Guid SubscriberId { get; set; }

        /// <summary>
        /// The 'Domain Object ID' for the Parent Object. The hibernate key for the parent domain object.
        /// The Parent is an object that rules the existance of a child. Forinstance IPersonalAssignment
        /// belongs to one person. This enables a client to subscribe to events just concerning himself.
        /// </summary>
        Guid ReferenceObjectId { get; set; }

        /// <summary>
        /// The 'Type' in String format.
        /// </summary>
        string ReferenceObjectType { get; set; }

        /// <summary>
        /// Domain Object Id is used for filtering. 
        /// This filter will only propagate messages
        /// where the domain object id and the domain
        /// object type of the Event Message corresponds
        /// to this id and this domain object type below.
        /// Typically the domain object id is a Guid.
        /// </summary>
        Guid DomainObjectId { get; set; }

        /// <summary>
        /// The Domain Object Type, or Event Topic,
        /// this is the namespace and class updated.
        /// </summary>
        string DomainObjectType { get; set; }

        /// <summary>
        /// Event Start Date, 
        /// the first date this event is valid,
        /// for subscription.
        /// </summary>
        DateTime EventStartDate { get; set; }

        /// <summary>
        /// Event End Date, 
        /// the last date this event is valid,
        /// for subscription.
        /// </summary>
        DateTime EventEndDate { get; set; }

        /// <summary>
        /// The user or program that created/changed the event.
        /// </summary>
        string ChangedBy { get; set; }

        /// <summary>
        /// The Date Time for which the event was created/changed.
        /// </summary>
        DateTime ChangedDateTime { get; set; }

        /// <summary>
        /// Gets the reference object type cache.
        /// </summary>
        /// <value>The reference object type cache.</value>
        Type ReferenceObjectTypeCache { get; }

        /// <summary>
        /// Gets the domain object type cache.
        /// </summary>
        /// <value>The domain object type cache.</value>
        Type DomainObjectTypeCache { get; }

        /// <summary>
        /// Override of the ToString method
        /// </summary>
        /// <returns></returns>
        String ToString();
    }
}