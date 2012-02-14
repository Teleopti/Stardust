using System.Collections.Generic;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// Base repository interface for insertion of an object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectInserter<T>
    {
        /// <summary>
        /// Executes an insert statement or a store procedure.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        bool Execute(ICollection<T> collection);
    }
}