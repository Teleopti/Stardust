namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Source class
    /// </summary>
    public interface IQueueSource : IAggregateRoot
    {
        /// <summary>
        /// Property Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The custom Description of queue.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the original CTI queue id.
        /// </summary>
        /// <value>The original cti queue id.</value>
        string QueueOriginalId { get; set; }

        /// <summary>
        /// Gets the log object name.
        /// </summary>
        /// <value>The log object name.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-07
        /// </remarks>
        string LogObjectName { get; set; }

        /// <summary>
        /// Gets or sets the agg queue id.
        /// </summary>
        /// <value>The agg queue id.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-07
        /// </remarks>
        string QueueAggId { get; set; }


        /// <summary>
        /// Gets or sets the data mart queue id.
        /// </summary>
        /// <value>The data mart queue id.</value>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2009-06-30
        /// </remarks>
        int QueueMartId { get; set; }

        /// <summary>
        /// Gets or sets the data source id from the data mart.
        /// </summary>
        /// <value>The data source id.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-07
        /// </remarks>
        int DataSourceId { get; set; }
    }
}
