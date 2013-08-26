using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The Message Filter Manager
    /// </summary>
    public interface IMessageFilterManager
    {
        ///<summary>
        /// The filter dictionary of domain object interfaces you can subscribe to.
        ///</summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<Type, IList<Type>> FilterDictionary { get; }

        /// <summary>
        /// Lookups the type.
        /// </summary>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <returns></returns>
        string LookupType(Type domainObjectType);
    }
}