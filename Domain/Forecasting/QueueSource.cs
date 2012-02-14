using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Source class
    /// </summary>
    public class QueueSource : AggregateRoot, IQueueSource, IDeleteTag
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private int _queueOriginalId;
        private int _queueAggId;
        private string _logObjectName = string.Empty;
        private int _dataSourceId;
        private IList<ILayer<IStat>> _layerCollection = new List<ILayer<IStat>>();
        private int _queueMartId;
        private bool _isDeleted;

        /// <summary>
        /// For Hibernate
        /// </summary>
        public QueueSource()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The custom description.</param>
        /// <param name="queueOriginalId">The original queue id from the CTI platform.</param>
        /// <param name="queueAggId">The queue id from aggregation database.</param>
        /// <param name="dataSourceId">The data source id from the data mart.</param>
        /// <param name="queueMartId">The data mart queue id.</param>
        public QueueSource(string name, string description, int queueOriginalId, int queueAggId, int queueMartId, int dataSourceId)
        {
            _name = name;
            _description = description;
            _queueOriginalId = queueOriginalId;
            _queueAggId = queueAggId;
            _dataSourceId = dataSourceId;
            _queueMartId = queueMartId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The custom description.</param>
        /// <param name="queueAggId">The queue id from aggregation database.</param>
        public QueueSource(string name, string description, int queueAggId)
        {
            _name = name;
            _description = description;
            _queueAggId = queueAggId;
        }

        /// <summary>
        /// Property Name
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The custom Description of queue.
        /// </summary>
        public virtual string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets or sets the original CTI queue id.
        /// </summary>
        /// <value>The original cti queue id.</value>
        public virtual int QueueOriginalId
        {
            get { return _queueOriginalId; }
            set { _queueOriginalId = value; }
        }

        /// <summary>
        /// Gets the layer collection.
        /// </summary>
        /// <value>The layer collection.</value>
        public virtual ILayerCollection<IStat> LayerCollection
        {
            get { return (new LayerCollection<IStat>(this, _layerCollection)); }
        }

        /// <summary>
        /// Gets the log object name.
        /// </summary>
        /// <value>The log object name.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-07
        /// </remarks>
        public virtual string LogObjectName
        {
            get { return _logObjectName; }
            set { _logObjectName = value; }
        }

        /// <summary>
        /// Gets or sets the agg queue id.
        /// </summary>
        /// <value>The agg queue id.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-07
        /// </remarks>
        public virtual int QueueAggId
        {
            get { return _queueAggId; }
            set { _queueAggId = value; }
        }

        /// <summary>
        /// Gets or sets the data mart queue id.
        /// </summary>
        /// <value>The data mart queue id.</value>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2009-06-30
        /// </remarks>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2009-06-30
        /// </remarks>
        public virtual int QueueMartId
        {
            get{ return _queueMartId; }
            set { _queueMartId = value; }
        }

        /// <summary>
        /// Gets or sets the data source id from the data mart.
        /// </summary>
        /// <value>The data source id.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-05-07
        /// </remarks>
        public virtual int DataSourceId
        {
            get { return _dataSourceId; }
            set { _dataSourceId = value; }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        /// <summary>
        /// Called before layer is added to collection.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-25
        /// </remarks>
        public virtual void OnAdd(ILayer<IStat> layer)
        {

        }


        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}
