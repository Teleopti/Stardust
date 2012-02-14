namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// ThreadPoolThreadSettings for the Broker Service server side.
    /// </summary>
    public enum ThreadPoolThreadSetting
    {
        /// <summary>
        /// The General number of threads for 
        /// the Broker Service server side. 
        /// This number can be high. 5 - 10 Max.
        /// </summary>
        GeneralThreadPoolThreads = 0,
        /// <summary>
        /// The number of database threads
        /// the Broker Service uses server side
        /// </summary>
        DatabaseThreadPoolThreads = 1,
        /// <summary>
        /// The number of receipt threads
        /// the Broker Service uses server side
        /// </summary>
        ReceiptThreadPoolThreads = 2,
        /// <summary>
        /// The number of heartbeat threads
        /// the Broker Service uses server side
        /// </summary>
        HeartbeatThreadPoolThreads = 3
    }
}
