using System;

namespace Teleopti.Ccc.Domain.MessageBroker.Client
{
    /// <summary>
    /// The Message Filter Manager
    /// </summary>
    public interface IMessageFilterManager
    {
        /// <summary>
        /// Determines if the typ exists in the filter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool HasType(Type type);

        /// <summary>
        /// Finds the type to notify as
        /// </summary>
        /// <param name="domainObjectType"></param>
        /// <returns></returns>
        /// <remarks>This is not the type that you subscribe to!</remarks>
        string LookupTypeToSend(Type domainObjectType);

        /// <summary>
        /// Lookups the type.
        /// </summary>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <returns></returns>
        Type LookupType(Type domainObjectType);
    }
}