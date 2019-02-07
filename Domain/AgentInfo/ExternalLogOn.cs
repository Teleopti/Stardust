using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    /// <summary>
    /// ACD Login from Matrix
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-06-17
    /// </remarks>
    public class ExternalLogOn : AggregateRoot_Events_ChangeInfo_Versioned, IExternalLogOn, IDeleteTag, IComparable
    {
        private int _acdLogOnMartId;
        private int _acdLogOnAggId;
        private string _acdLogOnOriginalId;
        private string _acdLogOnName;
        private bool _active;
        private int _dataSourceId = -1;
        private bool _isDeleted;
        private string _dataSourceName = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalLogOn"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-17
        /// </remarks>
        public ExternalLogOn()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalLogOn"/> class.
        /// </summary>
        /// <param name="acdLogOnMartId">The log on id.</param>
        /// <param name="acdLogOnAggId">The log on agg id.</param>
        /// <param name="acdLogOnOriginalId">The log on code.</param>
        /// <param name="acdLogOnName">Name of the log on.</param>
        /// <param name="active">if set to <c>true</c> [active].</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        public ExternalLogOn(int acdLogOnMartId, int acdLogOnAggId, string acdLogOnOriginalId, string acdLogOnName, bool active) : this()
        {
            _acdLogOnMartId = acdLogOnMartId;
            _acdLogOnAggId = acdLogOnAggId;
            _acdLogOnOriginalId = acdLogOnOriginalId;
            _acdLogOnName = acdLogOnName;
            _active = active;
        }

        /// <summary>
        /// Gets the log on id.
        /// </summary>
        /// <value>The log on id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        public virtual int AcdLogOnMartId
        {
            get { return _acdLogOnMartId; }
            set { _acdLogOnMartId = value; }
        }

        /// <summary>
        /// Gets the log on agg id.
        /// </summary>
        /// <value>The log on agg id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        public virtual int AcdLogOnAggId
        {
            get { return _acdLogOnAggId; }
            set { _acdLogOnAggId = value; }
        }

        /// <summary>
        /// Gets the log on code.
        /// </summary>
        /// <value>The log on code.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        public virtual string AcdLogOnOriginalId
        {
            get { return _acdLogOnOriginalId; }
            set { _acdLogOnOriginalId = value; }
        }

        /// <summary>
        /// Gets the name of the log on.
        /// </summary>
        /// <value>The name of the log on.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        public virtual string AcdLogOnName
        {
            get { return _acdLogOnName; }
            set { _acdLogOnName = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ExternalLogOn"/> is active.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        public virtual bool Active
        {
            get { return _active; }
            set { _active = value; }
        }

        /// <summary>
        /// Gets the data source id.
        /// </summary>
        /// <value>The data source id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        public virtual int DataSourceId
        {
            get { return _dataSourceId; }
            set { _dataSourceId = value; }
        }

        /// <summary>
        /// This method will return the data source name
        /// </summary>
        public virtual string DataSourceName
        {
            get { return _dataSourceName; }
            set { _dataSourceName = value; }
        }

        public virtual bool IsDeleted => _isDeleted;

		public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        public virtual int CompareTo(object obj)
        {
            return string.Compare(AcdLogOnName, ((ExternalLogOn)obj).AcdLogOnName, true, CultureInfo.CurrentCulture);
        }
    }
}
