using System;
using System.Diagnostics.CodeAnalysis;

namespace Teleopti.Interfaces.MessageBroker.Events
{
    /// <summary>
    /// Event Message, is the acctual message sent as a Message or Receipt.
    /// </summary>
    public interface IEventMessage : IComparable
    {
        /// <summary>
        /// Each type of event has an ID.
        /// </summary>
        Guid EventId { get; set; }

        /// <summary>
        /// Event Date for which this partiuclar event is valid.
        /// </summary>
        DateTime EventStartDate { get; set; }

        /// <summary>
        /// Event Date for which this partiuclar event is valid.
        /// </summary>
        DateTime EventEndDate { get; set; }
        
        /// <summary>
        /// The module id within the process.
        /// </summary>
        Guid ModuleId { get; set; }
        
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
        /// The 'Domain Object ID'. The hibernate key for the domain object.
        /// </summary>
        Guid DomainObjectId { get; set; }

        /// <summary>
        /// The 'Type' in String format.
        /// </summary>
        string DomainObjectType { get; set; }

        /// <summary>
        /// The 'Type' in System.Type format, 
        /// it needs to be an interface
        /// in order for the ASM to work.
        /// This Property is set by the MessageBroker.
        /// </summary>
        Type InterfaceType { get; set; }

        /// <summary>
        /// Insert, Update or Delete?
        /// </summary>
        DomainUpdateType DomainUpdateType { get; set; }

        /// <summary>
        /// The serialised Domain Object
        /// in form of a byte array. 
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        byte[] DomainObject { get; set; }

        /// <summary>
        /// The Changed By code represents the namespace and class name of the 
        /// code that created / updated the 'Event Message'
        /// or the User and Domain of person who raised the Event.
        /// </summary>
        string ChangedBy { get; set; }

        /// <summary>
        /// When Event Message was created / updated.
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
    }
}