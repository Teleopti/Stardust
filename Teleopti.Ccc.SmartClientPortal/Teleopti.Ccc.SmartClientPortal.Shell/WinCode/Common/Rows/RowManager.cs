using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows
{
    public class RowManager<TRow, TSource> : IRowManager<TRow, TSource> where TRow : IGridRow
    {
        private readonly IList<TRow> _rows = new List<TRow>();
        private IList<TSource> _dataSource = new List<TSource>();
        private IList<IntervalDefinition> _intervals;
        private int _intervalLength;
        private readonly WeakReference _grid;

        /// <summary>
        /// Initializes a new instance of the <see cref="RowManager&lt;T, Y&gt;"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="intervals">The intervals.</param>
        /// <param name="intervalLength">Length of the interval.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        public RowManager(ITeleoptiGridControl grid, IList<IntervalDefinition> intervals, int intervalLength)
        {
            _intervals = intervals;
            _intervalLength = intervalLength;
            _grid = new WeakReference(grid);
        }

        /// <summary>
        /// Gets or sets the base date.
        /// </summary>
        /// <value>The base date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// Gets the time zone info.
        /// </summary>
        /// <value>The time zone info.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-03-21
        /// </remarks>
        public virtual TimeZoneInfo TimeZoneInfo => TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;

		/// <summary>
        /// Gets the grid.
        /// </summary>
        /// <value>The grid.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        public ITeleoptiGridControl Grid => _grid.Target as ITeleoptiGridControl;

		/// <summary>
        /// Gets the length of the interval.
        /// </summary>
        /// <value>The length of the interval.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        public int IntervalLength
        {
            get { return _intervalLength; }
            set { _intervalLength = value; }
        }

        /// <summary>
        /// Gets the intervals.
        /// </summary>
        /// <value>The intervals.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        //Will fix supression later
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<IntervalDefinition> Intervals
        {
            get { return _intervals; }
            set { _intervals = value; }
        }

        /// <summary>
        /// Gets the data source.
        /// </summary>
        /// <value>The data source.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        public IList<TSource> DataSource => _dataSource;

		/// <summary>
        /// Sets the data source.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        public void SetDataSource(IEnumerable<TSource> dataSource)
        {
            _dataSource.Clear();
            if (dataSource == null) return;
            _dataSource = new List<TSource>(dataSource);
        }

        /// <summary>
        /// Adds the row.
        /// </summary>
        /// <param name="newRow">The new row.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        public TRow AddRow(TRow newRow)
        {
            _rows.Add(newRow);
            return newRow;
        }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        /// <value>The rows.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        public IList<TRow> Rows => _rows;
	}
}