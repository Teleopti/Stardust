using System;

namespace Teleopti.Interfaces.MessageBroker.Events
{
    public interface IEventMessage : IComparable
    {
        Guid EventId { get; set; }
        DateTime EventStartDate { get; set; }
        DateTime EventEndDate { get; set; }
        Guid ModuleId { get; set; }
        Guid ReferenceObjectId { get; set; }
        Guid DomainObjectId { get; set; }
        string DomainObjectType { get; set; }
        Type InterfaceType { get; set; }
        DomainUpdateType DomainUpdateType { get; set; }
        byte[] DomainObject { get; set; }
        string ChangedBy { get; set; }
        DateTime ChangedDateTime { get; set; }
        Type DomainObjectTypeCache { get; }
    }
}