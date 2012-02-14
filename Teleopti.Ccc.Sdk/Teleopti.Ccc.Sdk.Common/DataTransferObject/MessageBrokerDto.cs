using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a MessageBrokerDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class MessageBrokerDto : IExtensibleDataObject
    {

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        [DataMember]
        public int Port { set; get; }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>The server.</value>
        [DataMember]
        public string Server { set; get; }

        /// <summary>
        /// Gets or sets the threads.
        /// </summary>
        /// <value>The threads.</value>
        [DataMember]
        public int Threads { set; get; }

        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>The interval.</value>
        [DataMember]
        public double Interval { set; get; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        [DataMember]
        public string ConnectionString { set; get; }

        /// <summary>
        /// Gets or sets the general thread pool threads.
        /// </summary>
        /// <value>The general thread pool threads.</value>
        [DataMember]
        public int GeneralThreadPoolThreads { set; get; }

        /// <summary>
        /// Gets or sets the database thread pool threads.
        /// </summary>
        /// <value>The database thread pool threads.</value>
        [DataMember]
        public int DatabaseThreadPoolThreads { set; get; }

        /// <summary>
        /// Gets or sets the receipt thread pool threads.
        /// </summary>
        /// <value>The receipt thread pool threads.</value>
        [DataMember]
        public int ReceiptThreadPoolThreads { set; get; }

        /// <summary>
        /// Gets or sets the heartbeat thread pool threads.
        /// </summary>
        /// <value>The heartbeat thread pool threads.</value>
        [DataMember]
        public int HeartbeatThreadPoolThreads { set; get; }

        public ExtensionDataObject ExtensionData
        {
            get;set;
        }
    }
}