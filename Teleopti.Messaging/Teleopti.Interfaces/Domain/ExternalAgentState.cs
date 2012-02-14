using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Container for external agent states
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-11-19
    /// </remarks>
    [Serializable]
    public class ExternalAgentState : IExternalAgentState
    {
        #region Implementation of IExternalAgentState

        private DateTime _timestamp;
        private TimeSpan _timeInState;
        private string _stateCode;
        private string _externalLogOn;
        private Guid _platformTypeId;
        private DateTime _batchId;
        private bool _isSnapshot;
        private int _dataSourceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalAgentState"/> class.
        /// </summary>
        /// <param name="externalLogOn">The external log on.</param>
        /// <param name="stateCode">The state code.</param>
        /// <param name="timeInState">State of the time in.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="platformTypeId">The platform type id.</param>
        /// <param name="dataSourceId">The data source id</param>
        /// <param name="batchId">The batch id</param>
        /// <param name="isSnapshot">if set to <c>true</c> agent state belongs to a snapshot.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-19
        /// </remarks>
        public ExternalAgentState(string externalLogOn, string stateCode, TimeSpan timeInState, DateTime timestamp, Guid platformTypeId, int dataSourceId, DateTime batchId, bool isSnapshot)
        {
            _externalLogOn = externalLogOn;
            _platformTypeId = platformTypeId;
            _timestamp = timestamp;
            _timeInState = timeInState;
            _stateCode = stateCode;
            _dataSourceId = dataSourceId;
            _batchId = batchId;
            _isSnapshot = isSnapshot;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalAgentState"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-25
        /// </remarks>
        public ExternalAgentState()
        {}

        /// <summary>
        /// Gets the time stamp.
        /// </summary>
        /// <value>The time stamp.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-06
        /// </remarks>
        public DateTime Timestamp
        {
            get { return _timestamp; }
            set
            {
                _timestamp = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Gets the time agent has been in this state.
        /// </summary>
        /// <value>The time in state.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-06
        /// </remarks>
        public TimeSpan TimeInState
        {
            get { return _timeInState; }
            set { _timeInState = value; }
        }

        /// <summary>
        /// Gets the state code.
        /// </summary>
        /// <value>The state code.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-06
        /// </remarks>
        public string StateCode
        {
            get { return _stateCode; }
            set { _stateCode = value; }
        }

        /// <summary>
        /// Gets the external log on.
        /// </summary>
        /// <value>The external log on.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-06
        /// </remarks>
        public string ExternalLogOn
        {
            get { return _externalLogOn; }
            set { _externalLogOn = value; }
        }

        /// <summary>
        /// Gets the platform type id.
        /// </summary>
        /// <value>The platform type id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-06
        /// </remarks>
        public Guid PlatformTypeId
        {
            get { return _platformTypeId; }
            set { _platformTypeId = value; }
        }

        /// <summary>
        /// Gets the data source id. Use togheter with ExternalLogon to map with correct agent.
        /// </summary>
        /// <value>The data source id.</value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-12-11
        /// </remarks>
        public int DataSourceId
        {
            get { return _dataSourceId; }
            set { _dataSourceId = value; }
        }

        /// <summary>
        /// Gets the batch id that keeps track of state changes that belongs to the same state snapshot from the platform. 
        /// When batch id is equal to SqlDateTime.MinValue.Value then the state change is not part of snapshot.
        /// </summary>
        /// <value>The batch id.</value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-12-11
        /// </remarks>
        public DateTime BatchId
        {
            get { return _batchId; }
            set { _batchId = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance belongs to a snapshot.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is snapshot; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-12-17
        /// </remarks>
        public bool IsSnapshot
        {
            get { return _isSnapshot; }
            set { _isSnapshot = value;}
        }

        #endregion
    }
}
