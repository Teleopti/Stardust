using System.Collections.Generic;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// Object reader is the base repository reader interface.1
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectReader<T>
    {
        /// <summary>
        /// Execute a read statement, or a store proc, returns a list.
        /// </summary>
        /// <returns></returns>
        IList<T> Execute();
        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        T Execute(int id);
    }
}