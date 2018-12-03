using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows
{
    /// <summary>
    /// Non-generic interface for row manager
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-10-23
    /// </remarks>
    public interface IRowManager
    {
        /// <summary>
        /// Gets or sets the base date.
        /// </summary>
        /// <value>The base date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        DateTime BaseDate { get; set; }

        /// <summary>
        /// Gets or sets the time zone info.
        /// </summary>
        /// <value>The time zone info.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-03-21
        /// </remarks>
        TimeZoneInfo TimeZoneInfo { get; }

        /// <summary>
        /// Gets the grid.
        /// </summary>
        /// <value>The grid.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        ITeleoptiGridControl Grid { get; }

        /// <summary>
        /// Gets the length of the interval.
        /// </summary>
        /// <value>The length of the interval.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        int IntervalLength { get; }

        /// <summary>
        /// Gets the intervals.
        /// </summary>
        /// <value>The intervals.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        IList<IntervalDefinition> Intervals { get; }
    }

    /// <summary>
    /// Typed interface for row manager
    /// </summary>
    /// <typeparam name="TRow">The type of the row.</typeparam>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-10-23
    /// </remarks>
    public interface IRowManager<TRow, TSource> : IRowManager where TRow : IGridRow
    {
        /// <summary>
        /// Gets the data source.
        /// </summary>
        /// <value>The data source.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        IList<TSource> DataSource { get; }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        /// <value>The rows.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        IList<TRow> Rows { get; }

        /// <summary>
        /// Sets the data source.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        void SetDataSource(IEnumerable<TSource> dataSource);

        /// <summary>
        /// Adds the row.
        /// </summary>
        /// <param name="newRow">The new row.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-26
        /// </remarks>
        TRow AddRow(TRow newRow);
    }
}