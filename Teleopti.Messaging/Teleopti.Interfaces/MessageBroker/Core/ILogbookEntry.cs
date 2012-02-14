using System;
using System.Runtime.Serialization;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// Log book entry used for the Message Broker GUI.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-07
    /// </remarks>
    public interface ILogbookEntry : ISerializable, IComparable
    {
        /// <summary>
        /// Gets the log date time.
        /// </summary>
        /// <value>The log date time.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        DateTime LogDateTime { get; }
        /// <summary>
        /// Gets the type of the log.
        /// </summary>
        /// <value>The type of the log.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        string LogType { get; }
        /// <summary>
        /// Gets the type of the class.
        /// </summary>
        /// <value>The type of the class.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        string ClassType { get; }
        /// <summary>
        /// Gets the log message.
        /// </summary>
        /// <value>The log message.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        string LogMessage { get; }
        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        string UserName { get; }
    }
}